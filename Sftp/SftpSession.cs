// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpSession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Requests;
using Renci.SshNet.Sftp.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Renci.SshNet.Sftp
{
  internal class SftpSession : SubsystemSession, ISftpSession, ISubsystemSession, IDisposable
  {
    internal const int MaximumSupportedVersion = 3;
    private const int MinimumSupportedVersion = 0;
    private readonly Dictionary<uint, SftpRequest> _requests = new Dictionary<uint, SftpRequest>();
    private readonly ISftpResponseFactory _sftpResponseFactory;
    private readonly List<byte> _data = new List<byte>(32768);
    private EventWaitHandle _sftpVersionConfirmed = (EventWaitHandle) new AutoResetEvent(false);
    private IDictionary<string, string> _supportedExtensions;
    private long _requestId;

    protected Encoding Encoding { get; private set; }

    public string WorkingDirectory { get; private set; }

    public uint ProtocolVersion { get; private set; }

    public uint NextRequestId => (uint) Interlocked.Increment(ref this._requestId);

    public SftpSession(
      ISession session,
      int operationTimeout,
      Encoding encoding,
      ISftpResponseFactory sftpResponseFactory)
      : base(session, "sftp", operationTimeout)
    {
      this.Encoding = encoding;
      this._sftpResponseFactory = sftpResponseFactory;
    }

    public void ChangeDirectory(string path)
    {
      string canonicalPath = this.GetCanonicalPath(path);
      this.RequestClose(this.RequestOpenDir(canonicalPath, false));
      this.WorkingDirectory = canonicalPath;
    }

    internal void SendMessage(SftpMessage sftpMessage) => this.SendData(sftpMessage.GetBytes());

    public string GetCanonicalPath(string path)
    {
      string fullRemotePath = this.GetFullRemotePath(path);
      string canonicalPath = string.Empty;
      KeyValuePair<string, SftpFileAttributes>[] keyValuePairArray1 = this.RequestRealPath(fullRemotePath, true);
      if (keyValuePairArray1 != null)
        canonicalPath = keyValuePairArray1[0].Key;
      if (!string.IsNullOrEmpty(canonicalPath))
        return canonicalPath;
      if (fullRemotePath.EndsWith("/.", StringComparison.OrdinalIgnoreCase) || fullRemotePath.EndsWith("/..", StringComparison.OrdinalIgnoreCase) || fullRemotePath.Equals("/", StringComparison.OrdinalIgnoreCase) || fullRemotePath.IndexOf('/') < 0)
        return fullRemotePath;
      string[] strArray = fullRemotePath.Split('/');
      string path1 = string.Join("/", strArray, 0, strArray.Length - 1);
      if (string.IsNullOrEmpty(path1))
        path1 = "/";
      KeyValuePair<string, SftpFileAttributes>[] keyValuePairArray2 = this.RequestRealPath(path1, true);
      if (keyValuePairArray2 != null)
        canonicalPath = keyValuePairArray2[0].Key;
      if (string.IsNullOrEmpty(canonicalPath))
        return fullRemotePath;
      string str = string.Empty;
      if (canonicalPath[canonicalPath.Length - 1] != '/')
        str = "/";
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}{2}", (object) canonicalPath, (object) str, (object) strArray[strArray.Length - 1]);
    }

    public ISftpFileReader CreateFileReader(
      byte[] handle,
      ISftpSession sftpSession,
      uint chunkSize,
      int maxPendingReads,
      long? fileSize)
    {
      return (ISftpFileReader) new SftpFileReader(handle, sftpSession, chunkSize, maxPendingReads, fileSize);
    }

    internal string GetFullRemotePath(string path)
    {
      string fullRemotePath = path;
      if (!string.IsNullOrEmpty(path) && path[0] != '/' && this.WorkingDirectory != null)
        fullRemotePath = this.WorkingDirectory[this.WorkingDirectory.Length - 1] != '/' ? string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}/{1}", (object) this.WorkingDirectory, (object) path) : string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}", (object) this.WorkingDirectory, (object) path);
      return fullRemotePath;
    }

    protected override void OnChannelOpen()
    {
      this.SendMessage((SftpMessage) new SftpInitRequest(3U));
      this.WaitOnHandle((WaitHandle) this._sftpVersionConfirmed, this.OperationTimeout);
      if (this.ProtocolVersion > 3U || this.ProtocolVersion < 0U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Server SFTP version {0} is not supported.", (object) this.ProtocolVersion));
      this.WorkingDirectory = this.RequestRealPath(".")[0].Key;
    }

    protected override void OnDataReceived(byte[] data)
    {
      int srcOffset = 0;
      int length = data.Length;
      if (this._data.Count == 0)
      {
        while (length >= 5)
        {
          int count = (int) data[srcOffset] << 24 | (int) data[srcOffset + 1] << 16 | (int) data[srcOffset + 2] << 8 | (int) data[srcOffset + 3];
          int num = count + 4;
          if (length >= num)
          {
            if (!this.TryLoadSftpMessage(data, srcOffset + 4, count))
              return;
            length -= num;
            srcOffset += num;
          }
          else
            break;
        }
        if (length == 0)
          return;
        if (srcOffset > 0)
        {
          byte[] numArray = new byte[length];
          Buffer.BlockCopy((Array) data, srcOffset, (Array) numArray, 0, length);
          this._data.AddRange((IEnumerable<byte>) numArray);
        }
        else
          this._data.AddRange((IEnumerable<byte>) data);
      }
      else
      {
        this._data.AddRange((IEnumerable<byte>) data);
        while (this._data.Count >= 5)
        {
          int count1 = (int) this._data[0] << 24 | (int) this._data[1] << 16 | (int) this._data[2] << 8 | (int) this._data[3];
          int count2 = count1 + 4;
          if (this._data.Count < count2)
            break;
          byte[] numArray = new byte[count1];
          this._data.CopyTo(4, numArray, 0, count1);
          if (this._data.Count == count2)
            this._data.Clear();
          else
            this._data.RemoveRange(0, count2);
          if (!this.TryLoadSftpMessage(numArray, 0, count1))
            break;
        }
      }
    }

    private bool TryLoadSftpMessage(byte[] packetData, int offset, int count)
    {
      SftpMessage response = this._sftpResponseFactory.Create(this.ProtocolVersion, packetData[offset], this.Encoding);
      response.Load(packetData, offset + 1, count - 1);
      try
      {
        if (response is SftpVersionResponse sftpVersionResponse)
        {
          this.ProtocolVersion = sftpVersionResponse.Version;
          this._supportedExtensions = sftpVersionResponse.Extentions;
          this._sftpVersionConfirmed.Set();
        }
        else
          this.HandleResponse(response as SftpResponse);
        return true;
      }
      catch (Exception ex)
      {
        this.RaiseError(ex);
        return false;
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      EventWaitHandle versionConfirmed = this._sftpVersionConfirmed;
      if (versionConfirmed != null)
      {
        this._sftpVersionConfirmed = (EventWaitHandle) null;
        versionConfirmed.Dispose();
      }
    }

    private void SendRequest(SftpRequest request)
    {
      lock (this._requests)
        this._requests.Add(request.RequestId, request);
      this.SendMessage((SftpMessage) request);
    }

    public byte[] RequestOpen(string path, Flags flags, bool nullOnError = false)
    {
      byte[] handle = (byte[]) null;
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpOpenRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, flags, (Action<SftpHandleResponse>) (response =>
        {
          handle = response.Handle;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return handle;
    }

    public SftpOpenAsyncResult BeginOpen(
      string path,
      Flags flags,
      AsyncCallback callback,
      object state)
    {
      SftpOpenAsyncResult asyncResult = new SftpOpenAsyncResult(callback, state);
      this.SendRequest((SftpRequest) new SftpOpenRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, flags, (Action<SftpHandleResponse>) (response => asyncResult.SetAsCompleted(response.Handle, false)), (Action<SftpStatusResponse>) (response => asyncResult.SetAsCompleted((Exception) SftpSession.GetSftpException(response), false))));
      return asyncResult;
    }

    public byte[] EndOpen(SftpOpenAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (asyncResult.EndInvokeCalled)
        throw new InvalidOperationException("EndOpen has already been called.");
      if (asyncResult.IsCompleted)
        return asyncResult.EndInvoke();
      using (WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle)
      {
        this.WaitOnHandle(asyncWaitHandle, this.OperationTimeout);
        return asyncResult.EndInvoke();
      }
    }

    public void RequestClose(byte[] handle)
    {
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpCloseRequest(this.ProtocolVersion, this.NextRequestId, handle, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public SftpCloseAsyncResult BeginClose(
      byte[] handle,
      AsyncCallback callback,
      object state)
    {
      SftpCloseAsyncResult asyncResult = new SftpCloseAsyncResult(callback, state);
      this.SendRequest((SftpRequest) new SftpCloseRequest(this.ProtocolVersion, this.NextRequestId, handle, (Action<SftpStatusResponse>) (response => asyncResult.SetAsCompleted((Exception) SftpSession.GetSftpException(response), false))));
      return asyncResult;
    }

    public void EndClose(SftpCloseAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (asyncResult.EndInvokeCalled)
        throw new InvalidOperationException("EndClose has already been called.");
      if (asyncResult.IsCompleted)
      {
        asyncResult.EndInvoke();
      }
      else
      {
        using (WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle)
        {
          this.WaitOnHandle(asyncWaitHandle, this.OperationTimeout);
          asyncResult.EndInvoke();
        }
      }
    }

    public SftpReadAsyncResult BeginRead(
      byte[] handle,
      ulong offset,
      uint length,
      AsyncCallback callback,
      object state)
    {
      SftpReadAsyncResult asyncResult = new SftpReadAsyncResult(callback, state);
      this.SendRequest((SftpRequest) new SftpReadRequest(this.ProtocolVersion, this.NextRequestId, handle, offset, length, (Action<SftpDataResponse>) (response => asyncResult.SetAsCompleted(response.Data, false)), (Action<SftpStatusResponse>) (response =>
      {
        if (response.StatusCode != StatusCodes.Eof)
          asyncResult.SetAsCompleted((Exception) SftpSession.GetSftpException(response), false);
        else
          asyncResult.SetAsCompleted(Array<byte>.Empty, false);
      })));
      return asyncResult;
    }

    public byte[] EndRead(SftpReadAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (asyncResult.EndInvokeCalled)
        throw new InvalidOperationException("EndRead has already been called.");
      if (asyncResult.IsCompleted)
        return asyncResult.EndInvoke();
      using (WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle)
      {
        this.WaitOnHandle(asyncWaitHandle, this.OperationTimeout);
        return asyncResult.EndInvoke();
      }
    }

    public byte[] RequestRead(byte[] handle, ulong offset, uint length)
    {
      SshException exception = (SshException) null;
      byte[] data = (byte[]) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpReadRequest(this.ProtocolVersion, this.NextRequestId, handle, offset, length, (Action<SftpDataResponse>) (response =>
        {
          data = response.Data;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          if (response.StatusCode != StatusCodes.Eof)
            exception = SftpSession.GetSftpException(response);
          else
            data = Array<byte>.Empty;
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
      return data;
    }

    public void RequestWrite(
      byte[] handle,
      ulong serverOffset,
      byte[] data,
      int offset,
      int length,
      AutoResetEvent wait,
      Action<SftpStatusResponse> writeCompleted = null)
    {
      SshException exception = (SshException) null;
      this.SendRequest((SftpRequest) new SftpWriteRequest(this.ProtocolVersion, this.NextRequestId, handle, serverOffset, data, offset, length, (Action<SftpStatusResponse>) (response =>
      {
        if (writeCompleted != null)
          writeCompleted(response);
        exception = SftpSession.GetSftpException(response);
        wait?.Set();
      })));
      if (wait != null)
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      if (exception != null)
        throw exception;
    }

    public SftpFileAttributes RequestLStat(string path)
    {
      SshException exception = (SshException) null;
      SftpFileAttributes attributes = (SftpFileAttributes) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpLStatRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpAttrsResponse>) (response =>
        {
          attributes = response.Attributes;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
      return attributes;
    }

    public SFtpStatAsyncResult BeginLStat(
      string path,
      AsyncCallback callback,
      object state)
    {
      SFtpStatAsyncResult asyncResult = new SFtpStatAsyncResult(callback, state);
      this.SendRequest((SftpRequest) new SftpLStatRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpAttrsResponse>) (response => asyncResult.SetAsCompleted(response.Attributes, false)), (Action<SftpStatusResponse>) (response => asyncResult.SetAsCompleted((Exception) SftpSession.GetSftpException(response), false))));
      return asyncResult;
    }

    public SftpFileAttributes EndLStat(SFtpStatAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (asyncResult.EndInvokeCalled)
        throw new InvalidOperationException("EndLStat has already been called.");
      if (asyncResult.IsCompleted)
        return asyncResult.EndInvoke();
      using (WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle)
      {
        this.WaitOnHandle(asyncWaitHandle, this.OperationTimeout);
        return asyncResult.EndInvoke();
      }
    }

    public SftpFileAttributes RequestFStat(byte[] handle, bool nullOnError)
    {
      SshException exception = (SshException) null;
      SftpFileAttributes attributes = (SftpFileAttributes) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpFStatRequest(this.ProtocolVersion, this.NextRequestId, handle, (Action<SftpAttrsResponse>) (response =>
        {
          attributes = response.Attributes;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null && !nullOnError)
        throw exception;
      return attributes;
    }

    public void RequestSetStat(string path, SftpFileAttributes attributes)
    {
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpSetStatRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, attributes, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public void RequestFSetStat(byte[] handle, SftpFileAttributes attributes)
    {
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpFSetStatRequest(this.ProtocolVersion, this.NextRequestId, handle, attributes, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public byte[] RequestOpenDir(string path, bool nullOnError = false)
    {
      SshException exception = (SshException) null;
      byte[] handle = (byte[]) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpOpenDirRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpHandleResponse>) (response =>
        {
          handle = response.Handle;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return handle;
    }

    public KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(
      byte[] handle)
    {
      SshException exception = (SshException) null;
      KeyValuePair<string, SftpFileAttributes>[] result = (KeyValuePair<string, SftpFileAttributes>[]) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpReadDirRequest(this.ProtocolVersion, this.NextRequestId, handle, (Action<SftpNameResponse>) (response =>
        {
          result = response.Files;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          if (response.StatusCode != StatusCodes.Eof)
            exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
      return result;
    }

    public void RequestRemove(string path)
    {
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpRemoveRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public void RequestMkDir(string path)
    {
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpMkDirRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public void RequestRmDir(string path)
    {
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpRmDirRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    internal KeyValuePair<string, SftpFileAttributes>[] RequestRealPath(
      string path,
      bool nullOnError = false)
    {
      SshException exception = (SshException) null;
      KeyValuePair<string, SftpFileAttributes>[] result = (KeyValuePair<string, SftpFileAttributes>[]) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpRealPathRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpNameResponse>) (response =>
        {
          result = response.Files;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return result;
    }

    public SftpRealPathAsyncResult BeginRealPath(
      string path,
      AsyncCallback callback,
      object state)
    {
      SftpRealPathAsyncResult asyncResult = new SftpRealPathAsyncResult(callback, state);
      this.SendRequest((SftpRequest) new SftpRealPathRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpNameResponse>) (response => asyncResult.SetAsCompleted(response.Files[0].Key, false)), (Action<SftpStatusResponse>) (response => asyncResult.SetAsCompleted((Exception) SftpSession.GetSftpException(response), false))));
      return asyncResult;
    }

    public string EndRealPath(SftpRealPathAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (asyncResult.EndInvokeCalled)
        throw new InvalidOperationException("EndRealPath has already been called.");
      if (asyncResult.IsCompleted)
        return asyncResult.EndInvoke();
      using (WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle)
      {
        this.WaitOnHandle(asyncWaitHandle, this.OperationTimeout);
        return asyncResult.EndInvoke();
      }
    }

    public SftpFileAttributes RequestStat(string path, bool nullOnError = false)
    {
      SshException exception = (SshException) null;
      SftpFileAttributes attributes = (SftpFileAttributes) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpStatRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpAttrsResponse>) (response =>
        {
          attributes = response.Attributes;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return attributes;
    }

    public SFtpStatAsyncResult BeginStat(
      string path,
      AsyncCallback callback,
      object state)
    {
      SFtpStatAsyncResult asyncResult = new SFtpStatAsyncResult(callback, state);
      this.SendRequest((SftpRequest) new SftpStatRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpAttrsResponse>) (response => asyncResult.SetAsCompleted(response.Attributes, false)), (Action<SftpStatusResponse>) (response => asyncResult.SetAsCompleted((Exception) SftpSession.GetSftpException(response), false))));
      return asyncResult;
    }

    public SftpFileAttributes EndStat(SFtpStatAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (asyncResult.EndInvokeCalled)
        throw new InvalidOperationException("EndStat has already been called.");
      if (asyncResult.IsCompleted)
        return asyncResult.EndInvoke();
      using (WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle)
      {
        this.WaitOnHandle(asyncWaitHandle, this.OperationTimeout);
        return asyncResult.EndInvoke();
      }
    }

    public void RequestRename(string oldPath, string newPath)
    {
      if (this.ProtocolVersion < 2U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_RENAME operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpRenameRequest(this.ProtocolVersion, this.NextRequestId, oldPath, newPath, this.Encoding, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    internal KeyValuePair<string, SftpFileAttributes>[] RequestReadLink(
      string path,
      bool nullOnError = false)
    {
      if (this.ProtocolVersion < 3U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_READLINK operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      KeyValuePair<string, SftpFileAttributes>[] result = (KeyValuePair<string, SftpFileAttributes>[]) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpReadLinkRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpNameResponse>) (response =>
        {
          result = response.Files;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return result;
    }

    public void RequestSymLink(string linkpath, string targetpath)
    {
      if (this.ProtocolVersion < 3U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_SYMLINK operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        this.SendRequest((SftpRequest) new SftpSymLinkRequest(this.ProtocolVersion, this.NextRequestId, linkpath, targetpath, this.Encoding, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        })));
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public void RequestPosixRename(string oldPath, string newPath)
    {
      if (this.ProtocolVersion < 3U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        PosixRenameRequest request = new PosixRenameRequest(this.ProtocolVersion, this.NextRequestId, oldPath, newPath, this.Encoding, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        }));
        if (!this._supportedExtensions.ContainsKey(request.Name))
          throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", (object) request.Name));
        this.SendRequest((SftpRequest) request);
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public SftpFileSytemInformation RequestStatVfs(
      string path,
      bool nullOnError = false)
    {
      if (this.ProtocolVersion < 3U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      SftpFileSytemInformation information = (SftpFileSytemInformation) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        StatVfsRequest request = new StatVfsRequest(this.ProtocolVersion, this.NextRequestId, path, this.Encoding, (Action<SftpExtendedReplyResponse>) (response =>
        {
          information = response.GetReply<StatVfsReplyInfo>().Information;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        }));
        if (!this._supportedExtensions.ContainsKey(request.Name))
          throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", (object) request.Name));
        this.SendRequest((SftpRequest) request);
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return information;
    }

    internal SftpFileSytemInformation RequestFStatVfs(
      byte[] handle,
      bool nullOnError = false)
    {
      if (this.ProtocolVersion < 3U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      SftpFileSytemInformation information = (SftpFileSytemInformation) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        FStatVfsRequest request = new FStatVfsRequest(this.ProtocolVersion, this.NextRequestId, handle, (Action<SftpExtendedReplyResponse>) (response =>
        {
          information = response.GetReply<StatVfsReplyInfo>().Information;
          wait.Set();
        }), (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        }));
        if (!this._supportedExtensions.ContainsKey(request.Name))
          throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", (object) request.Name));
        this.SendRequest((SftpRequest) request);
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (!nullOnError && exception != null)
        throw exception;
      return information;
    }

    internal void HardLink(string oldPath, string newPath)
    {
      if (this.ProtocolVersion < 3U)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SSH_FXP_EXTENDED operation is not supported in {0} version that server operates in.", (object) this.ProtocolVersion));
      SshException exception = (SshException) null;
      using (AutoResetEvent wait = new AutoResetEvent(false))
      {
        HardLinkRequest request = new HardLinkRequest(this.ProtocolVersion, this.NextRequestId, oldPath, newPath, (Action<SftpStatusResponse>) (response =>
        {
          exception = SftpSession.GetSftpException(response);
          wait.Set();
        }));
        if (!this._supportedExtensions.ContainsKey(request.Name))
          throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Extension method {0} currently not supported by the server.", (object) request.Name));
        this.SendRequest((SftpRequest) request);
        this.WaitOnHandle((WaitHandle) wait, this.OperationTimeout);
      }
      if (exception != null)
        throw exception;
    }

    public uint CalculateOptimalReadLength(uint bufferSize)
    {
      uint localPacketSize = this.Channel.LocalPacketSize;
      return Math.Min(bufferSize, localPacketSize) - 13U;
    }

    public uint CalculateOptimalWriteLength(uint bufferSize, byte[] handle)
    {
      uint num = (uint) (25 + handle.Length);
      uint remotePacketSize = this.Channel.RemotePacketSize;
      return Math.Min(bufferSize, remotePacketSize) - num;
    }

    private static SshException GetSftpException(SftpStatusResponse response)
    {
      switch (response.StatusCode)
      {
        case StatusCodes.Ok:
          return (SshException) null;
        case StatusCodes.NoSuchFile:
          return (SshException) new SftpPathNotFoundException(response.ErrorMessage);
        case StatusCodes.PermissionDenied:
          return (SshException) new SftpPermissionDeniedException(response.ErrorMessage);
        default:
          return new SshException(response.ErrorMessage);
      }
    }

    private void HandleResponse(SftpResponse response)
    {
      SftpRequest sftpRequest;
      lock (this._requests)
      {
        this._requests.TryGetValue(response.ResponseId, out sftpRequest);
        if (sftpRequest != null)
          this._requests.Remove(response.ResponseId);
      }
      if (sftpRequest == null)
        throw new InvalidOperationException("Invalid response.");
      sftpRequest.Complete(response);
    }
  }
}
