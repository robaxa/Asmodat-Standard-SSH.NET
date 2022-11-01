// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.SftpClient
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Renci.SshNet
{
  public class SftpClient : BaseClient
  {
    private static readonly Encoding Utf8NoBOM = (Encoding) new UTF8Encoding(false, true);
    private ISftpSession _sftpSession;
    private int _operationTimeout;
    private uint _bufferSize;

    public TimeSpan OperationTimeout
    {
      get
      {
        this.CheckDisposed();
        return TimeSpan.FromMilliseconds((double) this._operationTimeout);
      }
      set
      {
        this.CheckDisposed();
        double totalMilliseconds = value.TotalMilliseconds;
        this._operationTimeout = totalMilliseconds >= -1.0 && totalMilliseconds <= (double) int.MaxValue ? (int) totalMilliseconds : throw new ArgumentOutOfRangeException(nameof (value), "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");
      }
    }

    public uint BufferSize
    {
      get
      {
        this.CheckDisposed();
        return this._bufferSize;
      }
      set
      {
        this.CheckDisposed();
        this._bufferSize = value;
      }
    }

    public string WorkingDirectory
    {
      get
      {
        this.CheckDisposed();
        return this._sftpSession != null ? this._sftpSession.WorkingDirectory : throw new SshConnectionException("Client not connected.");
      }
    }

    public int ProtocolVersion
    {
      get
      {
        this.CheckDisposed();
        return this._sftpSession != null ? (int) this._sftpSession.ProtocolVersion : throw new SshConnectionException("Client not connected.");
      }
    }

    internal ISftpSession SftpSession => this._sftpSession;

    public SftpClient(ConnectionInfo connectionInfo)
      : this(connectionInfo, false)
    {
    }

    public SftpClient(string host, int port, string username, string password)
      : this((ConnectionInfo) new PasswordConnectionInfo(host, port, username, password), true)
    {
    }

    public SftpClient(string host, string username, string password)
      : this(host, ConnectionInfo.DefaultPort, username, password)
    {
    }

    public SftpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
      : this((ConnectionInfo) new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
    {
    }

    public SftpClient(string host, string username, params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
    {
    }

    private SftpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
      : this(connectionInfo, ownsConnectionInfo, (IServiceFactory) new ServiceFactory())
    {
    }

    internal SftpClient(
      ConnectionInfo connectionInfo,
      bool ownsConnectionInfo,
      IServiceFactory serviceFactory)
      : base(connectionInfo, ownsConnectionInfo, serviceFactory)
    {
      this._operationTimeout = Session.Infinite;
      this._bufferSize = 32768U;
    }

    public void ChangeDirectory(string path)
    {
      this.CheckDisposed();
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      this._sftpSession.ChangeDirectory(path);
    }

    public void ChangePermissions(string path, short mode) => this.Get(path).SetPermissions(mode);

    public void CreateDirectory(string path)
    {
      this.CheckDisposed();
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(path);
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      this._sftpSession.RequestMkDir(this._sftpSession.GetCanonicalPath(path));
    }

    public void DeleteDirectory(string path)
    {
      this.CheckDisposed();
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      this._sftpSession.RequestRmDir(this._sftpSession.GetCanonicalPath(path));
    }

    public void DeleteFile(string path)
    {
      this.CheckDisposed();
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      this._sftpSession.RequestRemove(this._sftpSession.GetCanonicalPath(path));
    }

    public void RenameFile(string oldPath, string newPath) => this.RenameFile(oldPath, newPath, false);

    public void RenameFile(string oldPath, string newPath, bool isPosix)
    {
      this.CheckDisposed();
      if (oldPath == null)
        throw new ArgumentNullException(nameof (oldPath));
      if (newPath == null)
        throw new ArgumentNullException(nameof (newPath));
      string oldPath1 = this._sftpSession != null ? this._sftpSession.GetCanonicalPath(oldPath) : throw new SshConnectionException("Client not connected.");
      string canonicalPath = this._sftpSession.GetCanonicalPath(newPath);
      if (isPosix)
        this._sftpSession.RequestPosixRename(oldPath1, canonicalPath);
      else
        this._sftpSession.RequestRename(oldPath1, canonicalPath);
    }

    public void SymbolicLink(string path, string linkPath)
    {
      this.CheckDisposed();
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      if (linkPath.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (linkPath));
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      this._sftpSession.RequestSymLink(this._sftpSession.GetCanonicalPath(path), this._sftpSession.GetCanonicalPath(linkPath));
    }

    public IEnumerable<SftpFile> ListDirectory(
      string path,
      Action<int> listCallback = null)
    {
      this.CheckDisposed();
      return this.InternalListDirectory(path, listCallback);
    }

    public IAsyncResult BeginListDirectory(
      string path,
      AsyncCallback asyncCallback,
      object state,
      Action<int> listCallback = null)
    {
      this.CheckDisposed();
      SftpListDirectoryAsyncResult asyncResult = new SftpListDirectoryAsyncResult(asyncCallback, state);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          asyncResult.SetAsCompleted(this.InternalListDirectory(path, (Action<int>) (count =>
          {
            asyncResult.Update(count);
            if (listCallback == null)
              return;
            listCallback(count);
          })), false);
        }
        catch (Exception ex)
        {
          asyncResult.SetAsCompleted(ex, false);
        }
      }));
      return (IAsyncResult) asyncResult;
    }

    public IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult)
    {
      if (!(asyncResult is SftpListDirectoryAsyncResult directoryAsyncResult) || directoryAsyncResult.EndInvokeCalled)
        throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");
      return directoryAsyncResult.EndInvoke();
    }

    public SftpFile Get(string path)
    {
      this.CheckDisposed();
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      string str = this._sftpSession != null ? this._sftpSession.GetCanonicalPath(path) : throw new SshConnectionException("Client not connected.");
      SftpFileAttributes attributes = this._sftpSession.RequestLStat(str);
      return new SftpFile(this._sftpSession, str, attributes);
    }

    public bool Exists(string path)
    {
      this.CheckDisposed();
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      string path1 = this._sftpSession != null ? this._sftpSession.GetCanonicalPath(path) : throw new SshConnectionException("Client not connected.");
      try
      {
        this._sftpSession.RequestLStat(path1);
        return true;
      }
      catch (SftpPathNotFoundException ex)
      {
        return false;
      }
    }

    public void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null)
    {
      this.CheckDisposed();
      this.InternalDownloadFile(path, output, (SftpDownloadAsyncResult) null, downloadCallback);
    }

    public IAsyncResult BeginDownloadFile(string path, Stream output) => this.BeginDownloadFile(path, output, (AsyncCallback) null, (object) null);

    public IAsyncResult BeginDownloadFile(
      string path,
      Stream output,
      AsyncCallback asyncCallback)
    {
      return this.BeginDownloadFile(path, output, asyncCallback, (object) null);
    }

    public IAsyncResult BeginDownloadFile(
      string path,
      Stream output,
      AsyncCallback asyncCallback,
      object state,
      Action<ulong> downloadCallback = null)
    {
      this.CheckDisposed();
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      if (output == null)
        throw new ArgumentNullException(nameof (output));
      SftpDownloadAsyncResult asyncResult = new SftpDownloadAsyncResult(asyncCallback, state);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          this.InternalDownloadFile(path, output, asyncResult, (Action<ulong>) (offset =>
          {
            asyncResult.Update(offset);
            if (downloadCallback == null)
              return;
            downloadCallback(offset);
          }));
          asyncResult.SetAsCompleted((Exception) null, false);
        }
        catch (Exception ex)
        {
          asyncResult.SetAsCompleted(ex, false);
        }
      }));
      return (IAsyncResult) asyncResult;
    }

    public void EndDownloadFile(IAsyncResult asyncResult)
    {
      if (!(asyncResult is SftpDownloadAsyncResult downloadAsyncResult) || downloadAsyncResult.EndInvokeCalled)
        throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");
      downloadAsyncResult.EndInvoke();
    }

    public void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null) => this.UploadFile(input, path, true, uploadCallback);

    public void UploadFile(
      Stream input,
      string path,
      bool canOverride,
      Action<ulong> uploadCallback = null)
    {
      this.CheckDisposed();
      Flags flags1 = Flags.Write | Flags.Truncate;
      Flags flags2 = !canOverride ? flags1 | Flags.CreateNew : flags1 | Flags.CreateNewOrOpen;
      this.InternalUploadFile(input, path, flags2, (SftpUploadAsyncResult) null, uploadCallback);
    }

    public IAsyncResult BeginUploadFile(Stream input, string path) => this.BeginUploadFile(input, path, true, (AsyncCallback) null, (object) null);

    public IAsyncResult BeginUploadFile(
      Stream input,
      string path,
      AsyncCallback asyncCallback)
    {
      return this.BeginUploadFile(input, path, true, asyncCallback, (object) null);
    }

    public IAsyncResult BeginUploadFile(
      Stream input,
      string path,
      AsyncCallback asyncCallback,
      object state,
      Action<ulong> uploadCallback = null)
    {
      return this.BeginUploadFile(input, path, true, asyncCallback, state, uploadCallback);
    }

    public IAsyncResult BeginUploadFile(
      Stream input,
      string path,
      bool canOverride,
      AsyncCallback asyncCallback,
      object state,
      Action<ulong> uploadCallback = null)
    {
      this.CheckDisposed();
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      Flags flags = Flags.Write | Flags.Truncate;
      if (canOverride)
        flags |= Flags.CreateNewOrOpen;
      else
        flags |= Flags.CreateNew;
      SftpUploadAsyncResult asyncResult = new SftpUploadAsyncResult(asyncCallback, state);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          this.InternalUploadFile(input, path, flags, asyncResult, (Action<ulong>) (offset =>
          {
            asyncResult.Update(offset);
            if (uploadCallback == null)
              return;
            uploadCallback(offset);
          }));
          asyncResult.SetAsCompleted((Exception) null, false);
        }
        catch (Exception ex)
        {
          asyncResult.SetAsCompleted(ex, false);
        }
      }));
      return (IAsyncResult) asyncResult;
    }

    public void EndUploadFile(IAsyncResult asyncResult)
    {
      if (!(asyncResult is SftpUploadAsyncResult uploadAsyncResult) || uploadAsyncResult.EndInvokeCalled)
        throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");
      uploadAsyncResult.EndInvoke();
    }

    public SftpFileSytemInformation GetStatus(string path)
    {
      this.CheckDisposed();
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      return this._sftpSession != null ? this._sftpSession.RequestStatVfs(this._sftpSession.GetCanonicalPath(path)) : throw new SshConnectionException("Client not connected.");
    }

    public void AppendAllLines(string path, IEnumerable<string> contents)
    {
      this.CheckDisposed();
      if (contents == null)
        throw new ArgumentNullException(nameof (contents));
      using (StreamWriter streamWriter = this.AppendText(path))
      {
        foreach (string content in contents)
          streamWriter.WriteLine(content);
      }
    }

    public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
      this.CheckDisposed();
      if (contents == null)
        throw new ArgumentNullException(nameof (contents));
      using (StreamWriter streamWriter = this.AppendText(path, encoding))
      {
        foreach (string content in contents)
          streamWriter.WriteLine(content);
      }
    }

    public void AppendAllText(string path, string contents)
    {
      using (StreamWriter streamWriter = this.AppendText(path))
        streamWriter.Write(contents);
    }

    public void AppendAllText(string path, string contents, Encoding encoding)
    {
      using (StreamWriter streamWriter = this.AppendText(path, encoding))
        streamWriter.Write(contents);
    }

    public StreamWriter AppendText(string path) => this.AppendText(path, SftpClient.Utf8NoBOM);

    public StreamWriter AppendText(string path, Encoding encoding)
    {
      this.CheckDisposed();
      if (encoding == null)
        throw new ArgumentNullException(nameof (encoding));
      return new StreamWriter((Stream) new SftpFileStream(this._sftpSession, path, FileMode.Append, FileAccess.Write, (int) this._bufferSize), encoding);
    }

    public SftpFileStream Create(string path)
    {
      this.CheckDisposed();
      return new SftpFileStream(this._sftpSession, path, FileMode.Create, FileAccess.ReadWrite, (int) this._bufferSize);
    }

    public SftpFileStream Create(string path, int bufferSize)
    {
      this.CheckDisposed();
      return new SftpFileStream(this._sftpSession, path, FileMode.Create, FileAccess.ReadWrite, bufferSize);
    }

    public StreamWriter CreateText(string path) => this.CreateText(path, SftpClient.Utf8NoBOM);

    public StreamWriter CreateText(string path, Encoding encoding)
    {
      this.CheckDisposed();
      return new StreamWriter((Stream) this.OpenWrite(path), encoding);
    }

    public void Delete(string path) => this.Get(path).Delete();

    public DateTime GetLastAccessTime(string path) => this.Get(path).LastAccessTime;

    public DateTime GetLastAccessTimeUtc(string path) => this.GetLastAccessTime(path).ToUniversalTime();

    public DateTime GetLastWriteTime(string path) => this.Get(path).LastWriteTime;

    public DateTime GetLastWriteTimeUtc(string path) => this.GetLastWriteTime(path).ToUniversalTime();

    public SftpFileStream Open(string path, FileMode mode) => this.Open(path, mode, FileAccess.ReadWrite);

    public SftpFileStream Open(string path, FileMode mode, FileAccess access)
    {
      this.CheckDisposed();
      return new SftpFileStream(this._sftpSession, path, mode, access, (int) this._bufferSize);
    }

    public SftpFileStream OpenRead(string path) => this.Open(path, FileMode.Open, FileAccess.Read);

    public StreamReader OpenText(string path) => new StreamReader((Stream) this.OpenRead(path), Encoding.UTF8);

    public SftpFileStream OpenWrite(string path)
    {
      this.CheckDisposed();
      return new SftpFileStream(this._sftpSession, path, FileMode.OpenOrCreate, FileAccess.Write, (int) this._bufferSize);
    }

    public byte[] ReadAllBytes(string path)
    {
      using (SftpFileStream sftpFileStream = this.OpenRead(path))
      {
        byte[] buffer = new byte[sftpFileStream.Length];
        sftpFileStream.Read(buffer, 0, buffer.Length);
        return buffer;
      }
    }

    public string[] ReadAllLines(string path) => this.ReadAllLines(path, Encoding.UTF8);

    public string[] ReadAllLines(string path, Encoding encoding)
    {
      List<string> stringList = new List<string>();
      using (StreamReader streamReader = new StreamReader((Stream) this.OpenRead(path), encoding))
      {
        while (!streamReader.EndOfStream)
          stringList.Add(streamReader.ReadLine());
      }
      return stringList.ToArray();
    }

    public string ReadAllText(string path) => this.ReadAllText(path, Encoding.UTF8);

    public string ReadAllText(string path, Encoding encoding)
    {
      using (StreamReader streamReader = new StreamReader((Stream) this.OpenRead(path), encoding))
        return streamReader.ReadToEnd();
    }

    public IEnumerable<string> ReadLines(string path) => (IEnumerable<string>) this.ReadAllLines(path);

    public IEnumerable<string> ReadLines(string path, Encoding encoding) => (IEnumerable<string>) this.ReadAllLines(path, encoding);

    [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
    public void SetLastAccessTime(string path, DateTime lastAccessTime) => throw new NotImplementedException();

    [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) => throw new NotImplementedException();

    [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
    public void SetLastWriteTime(string path, DateTime lastWriteTime) => throw new NotImplementedException();

    [Obsolete("Note: This method currently throws NotImplementedException because it has not yet been implemented.")]
    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => throw new NotImplementedException();

    public void WriteAllBytes(string path, byte[] bytes)
    {
      using (SftpFileStream sftpFileStream = this.OpenWrite(path))
        sftpFileStream.Write(bytes, 0, bytes.Length);
    }

    public void WriteAllLines(string path, IEnumerable<string> contents) => this.WriteAllLines(path, contents, SftpClient.Utf8NoBOM);

    public void WriteAllLines(string path, string[] contents) => this.WriteAllLines(path, contents, SftpClient.Utf8NoBOM);

    public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
    {
      using (StreamWriter text = this.CreateText(path, encoding))
      {
        foreach (string content in contents)
          text.WriteLine(content);
      }
    }

    public void WriteAllLines(string path, string[] contents, Encoding encoding)
    {
      using (StreamWriter text = this.CreateText(path, encoding))
      {
        foreach (string content in contents)
          text.WriteLine(content);
      }
    }

    public void WriteAllText(string path, string contents)
    {
      using (StreamWriter text = this.CreateText(path))
        text.Write(contents);
    }

    public void WriteAllText(string path, string contents, Encoding encoding)
    {
      using (StreamWriter text = this.CreateText(path, encoding))
        text.Write(contents);
    }

    public SftpFileAttributes GetAttributes(string path)
    {
      this.CheckDisposed();
      return this._sftpSession != null ? this._sftpSession.RequestLStat(this._sftpSession.GetCanonicalPath(path)) : throw new SshConnectionException("Client not connected.");
    }

    public void SetAttributes(string path, SftpFileAttributes fileAttributes)
    {
      this.CheckDisposed();
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      this._sftpSession.RequestSetStat(this._sftpSession.GetCanonicalPath(path), fileAttributes);
    }

    public IEnumerable<FileInfo> SynchronizeDirectories(
      string sourcePath,
      string destinationPath,
      string searchPattern)
    {
      if (sourcePath == null)
        throw new ArgumentNullException(nameof (sourcePath));
      if (destinationPath.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (destinationPath));
      return this.InternalSynchronizeDirectories(sourcePath, destinationPath, searchPattern, (SftpSynchronizeDirectoriesAsyncResult) null);
    }

    public IAsyncResult BeginSynchronizeDirectories(
      string sourcePath,
      string destinationPath,
      string searchPattern,
      AsyncCallback asyncCallback,
      object state)
    {
      if (sourcePath == null)
        throw new ArgumentNullException(nameof (sourcePath));
      if (destinationPath.IsNullOrWhiteSpace())
        throw new ArgumentException("destDir");
      SftpSynchronizeDirectoriesAsyncResult asyncResult = new SftpSynchronizeDirectoriesAsyncResult(asyncCallback, state);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          asyncResult.SetAsCompleted(this.InternalSynchronizeDirectories(sourcePath, destinationPath, searchPattern, asyncResult), false);
        }
        catch (Exception ex)
        {
          asyncResult.SetAsCompleted(ex, false);
        }
      }));
      return (IAsyncResult) asyncResult;
    }

    public IEnumerable<FileInfo> EndSynchronizeDirectories(
      IAsyncResult asyncResult)
    {
      if (!(asyncResult is SftpSynchronizeDirectoriesAsyncResult directoriesAsyncResult) || directoriesAsyncResult.EndInvokeCalled)
        throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");
      return directoriesAsyncResult.EndInvoke();
    }

    private IEnumerable<FileInfo> InternalSynchronizeDirectories(
      string sourcePath,
      string destinationPath,
      string searchPattern,
      SftpSynchronizeDirectoriesAsyncResult asynchResult)
    {
      if (!Directory.Exists(sourcePath))
        throw new FileNotFoundException(string.Format("Source directory not found: {0}", (object) sourcePath));
      List<FileInfo> fileInfoList = new List<FileInfo>();
      List<FileInfo> list = FileSystemAbstraction.EnumerateFiles(new DirectoryInfo(sourcePath), searchPattern).ToList<FileInfo>();
      if (list.Count == 0)
        return (IEnumerable<FileInfo>) fileInfoList;
      IEnumerable<SftpFile> sftpFiles = this.InternalListDirectory(destinationPath, (Action<int>) null);
      Dictionary<string, SftpFile> dictionary = new Dictionary<string, SftpFile>();
      foreach (SftpFile sftpFile in sftpFiles)
      {
        if (!sftpFile.IsDirectory)
          dictionary.Add(sftpFile.Name, sftpFile);
      }
      foreach (FileInfo fileInfo in list)
      {
        bool flag = !dictionary.ContainsKey(fileInfo.Name);
        if (!flag)
        {
          SftpFile sftpFile = dictionary[fileInfo.Name];
          flag = fileInfo.Length != sftpFile.Length;
        }
        if (flag)
        {
          string path = string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}/{1}", (object) destinationPath, (object) fileInfo.Name);
          try
          {
            using (FileStream input = File.OpenRead(fileInfo.FullName))
              this.InternalUploadFile((Stream) input, path, Flags.Write | Flags.CreateNewOrOpen | Flags.Truncate, (SftpUploadAsyncResult) null, (Action<ulong>) null);
            fileInfoList.Add(fileInfo);
            asynchResult?.Update(fileInfoList.Count);
          }
          catch (Exception ex)
          {
            throw new Exception(string.Format("Failed to upload {0} to {1}", (object) fileInfo.FullName, (object) path), ex);
          }
        }
      }
      return (IEnumerable<FileInfo>) fileInfoList;
    }

    private IEnumerable<SftpFile> InternalListDirectory(
      string path,
      Action<int> listCallback)
    {
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      string path1 = this._sftpSession != null ? this._sftpSession.GetCanonicalPath(path) : throw new SshConnectionException("Client not connected.");
      byte[] handle = this._sftpSession.RequestOpenDir(path1);
      string basePath = path1;
      if (!basePath.EndsWith("/"))
        basePath = string.Format("{0}/", (object) path1);
      List<SftpFile> result = new List<SftpFile>();
      for (KeyValuePair<string, SftpFileAttributes>[] source = this._sftpSession.RequestReadDir(handle); source != null; source = this._sftpSession.RequestReadDir(handle))
      {
        result.AddRange(((IEnumerable<KeyValuePair<string, SftpFileAttributes>>) source).Select<KeyValuePair<string, SftpFileAttributes>, SftpFile>((Func<KeyValuePair<string, SftpFileAttributes>, SftpFile>) (f => new SftpFile(this._sftpSession, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}{1}", (object) basePath, (object) f.Key), f.Value))));
        if (listCallback != null)
          ThreadAbstraction.ExecuteThread((Action) (() => listCallback(result.Count)));
      }
      this._sftpSession.RequestClose(handle);
      return (IEnumerable<SftpFile>) result;
    }

    private void InternalDownloadFile(
      string path,
      Stream output,
      SftpDownloadAsyncResult asyncResult,
      Action<ulong> downloadCallback)
    {
      if (output == null)
        throw new ArgumentNullException(nameof (output));
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      using (ISftpFileReader sftpFileReader = this.ServiceFactory.CreateSftpFileReader(this._sftpSession.GetCanonicalPath(path), this._sftpSession, this._bufferSize))
      {
        ulong num = 0;
        while (true)
        {
          if (asyncResult == null || !asyncResult.IsDownloadCanceled)
          {
            byte[] buffer = sftpFileReader.Read();
            if (buffer.Length != 0)
            {
              output.Write(buffer, 0, buffer.Length);
              num += (ulong) buffer.Length;
              if (downloadCallback != null)
              {
                ulong downloadOffset = num;
                ThreadAbstraction.ExecuteThread((Action) (() => downloadCallback(downloadOffset)));
              }
            }
            else
              goto label_9;
          }
          else
            break;
        }
        return;
label_9:;
      }
    }

    private void InternalUploadFile(
      Stream input,
      string path,
      Flags flags,
      SftpUploadAsyncResult asyncResult,
      Action<ulong> uploadCallback)
    {
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (path));
      if (this._sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      byte[] handle = this._sftpSession.RequestOpen(this._sftpSession.GetCanonicalPath(path), flags);
      ulong serverOffset = 0;
      byte[] numArray = new byte[(int) this._sftpSession.CalculateOptimalWriteLength(this._bufferSize, handle)];
      int length = input.Read(numArray, 0, numArray.Length);
      int expectedResponses = 0;
      AutoResetEvent responseReceivedWaitHandle = new AutoResetEvent(false);
      while (asyncResult == null || !asyncResult.IsUploadCanceled)
      {
        if (length > 0)
        {
          ulong writtenBytes = serverOffset + (ulong) length;
          this._sftpSession.RequestWrite(handle, serverOffset, numArray, 0, length, (AutoResetEvent) null, (Action<SftpStatusResponse>) (s =>
          {
            if (s.StatusCode != StatusCodes.Ok)
              return;
            Interlocked.Decrement(ref expectedResponses);
            responseReceivedWaitHandle.Set();
            if (uploadCallback != null)
              ThreadAbstraction.ExecuteThread((Action) (() => uploadCallback(writtenBytes)));
          }));
          Interlocked.Increment(ref expectedResponses);
          serverOffset += (ulong) length;
          length = input.Read(numArray, 0, numArray.Length);
        }
        else if (expectedResponses > 0)
          this._sftpSession.WaitOnHandle((WaitHandle) responseReceivedWaitHandle, this._operationTimeout);
        if (expectedResponses <= 0 && length <= 0)
          break;
      }
      this._sftpSession.RequestClose(handle);
    }

    protected override void OnConnected()
    {
      base.OnConnected();
      this._sftpSession = this.CreateAndConnectToSftpSession();
    }

    protected override void OnDisconnecting()
    {
      base.OnDisconnecting();
      ISftpSession sftpSession = this._sftpSession;
      if (sftpSession == null)
        return;
      this._sftpSession = (ISftpSession) null;
      sftpSession.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      ISftpSession sftpSession = this._sftpSession;
      if (sftpSession != null)
      {
        this._sftpSession = (ISftpSession) null;
        sftpSession.Dispose();
      }
    }

    private ISftpSession CreateAndConnectToSftpSession()
    {
      ISftpSession sftpSession = this.ServiceFactory.CreateSftpSession(this.Session, this._operationTimeout, this.ConnectionInfo.Encoding, this.ServiceFactory.CreateSftpResponseFactory());
      try
      {
        sftpSession.Connect();
        return sftpSession;
      }
      catch
      {
        sftpSession.Dispose();
        throw;
      }
    }
  }
}
