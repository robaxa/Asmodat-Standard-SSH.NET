// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpFileReader
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Renci.SshNet.Sftp
{
  internal class SftpFileReader : ISftpFileReader, IDisposable
  {
    private const int ReadAheadWaitTimeoutInMilliseconds = 1000;
    private readonly byte[] _handle;
    private readonly ISftpSession _sftpSession;
    private readonly uint _chunkSize;
    private ulong _offset;
    private readonly long? _fileSize;
    private readonly Dictionary<int, SftpFileReader.BufferedRead> _queue;
    private readonly WaitHandle[] _waitHandles;
    private int _readAheadChunkIndex;
    private ulong _readAheadOffset;
    private readonly ManualResetEvent _readAheadCompleted;
    private int _nextChunkIndex;
    private bool _endOfFileReceived;
    private bool _isEndOfFileRead;
    private readonly SemaphoreLight _semaphore;
    private readonly object _readLock;
    private readonly ManualResetEvent _disposingWaitHandle;
    private bool _disposingOrDisposed;
    private Exception _exception;

    public SftpFileReader(
      byte[] handle,
      ISftpSession sftpSession,
      uint chunkSize,
      int maxPendingReads,
      long? fileSize)
    {
      this._handle = handle;
      this._sftpSession = sftpSession;
      this._chunkSize = chunkSize;
      this._fileSize = fileSize;
      this._semaphore = new SemaphoreLight(maxPendingReads);
      this._queue = new Dictionary<int, SftpFileReader.BufferedRead>(maxPendingReads);
      this._readLock = new object();
      this._readAheadCompleted = new ManualResetEvent(false);
      this._disposingWaitHandle = new ManualResetEvent(false);
      this._waitHandles = this._sftpSession.CreateWaitHandleArray((WaitHandle) this._disposingWaitHandle, this._semaphore.AvailableWaitHandle);
      this.StartReadAhead();
    }

    public byte[] Read()
    {
      if (this._disposingOrDisposed)
        throw new ObjectDisposedException(this.GetType().FullName);
      if (this._exception != null)
        throw this._exception;
      if (this._isEndOfFileRead)
        throw new SshException("Attempting to read beyond the end of the file.");
      SftpFileReader.BufferedRead bufferedRead;
      lock (this._readLock)
      {
        while (!this._queue.TryGetValue(this._nextChunkIndex, out bufferedRead) && this._exception == null)
          Monitor.Wait(this._readLock);
        if (this._exception != null)
          throw this._exception;
        byte[] data = bufferedRead.Data;
        if ((long) bufferedRead.Offset == (long) this._offset)
        {
          if (data.Length == 0)
          {
            this._isEndOfFileRead = true;
          }
          else
          {
            this._queue.Remove(this._nextChunkIndex);
            this._offset += (ulong) data.Length;
            ++this._nextChunkIndex;
          }
          this._semaphore.Release();
          return data;
        }
        if (data.Length == 0 && this._fileSize.HasValue && (long) this._offset == this._fileSize.Value)
        {
          this._isEndOfFileRead = true;
          this._semaphore.Release();
          return bufferedRead.Data;
        }
      }
      byte[] numArray = this._sftpSession.RequestRead(this._handle, this._offset, (uint) (bufferedRead.Offset - this._offset));
      if (numArray.Length == 0)
      {
        lock (this._readLock)
        {
          if (bufferedRead.Data.Length == 0)
          {
            this._isEndOfFileRead = true;
            if (!this._disposingOrDisposed)
              this._semaphore.Release();
            return numArray;
          }
          this._exception = (Exception) new SshException("Unexpectedly reached end of file.");
          if (!this._disposingOrDisposed)
            this._semaphore.Release();
          throw this._exception;
        }
      }
      else
      {
        this._offset += (ulong) (uint) numArray.Length;
        return numArray;
      }
    }

    ~SftpFileReader() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (this._disposingOrDisposed)
        return;
      this._disposingOrDisposed = true;
      if (!disposing)
        return;
      this._exception = (Exception) new ObjectDisposedException(this.GetType().FullName);
      this._disposingWaitHandle.Set();
      this._readAheadCompleted.WaitOne();
      lock (this._readLock)
      {
        this._semaphore.Dispose();
        Monitor.PulseAll(this._readLock);
      }
      this._readAheadCompleted.Dispose();
      this._disposingWaitHandle.Dispose();
      if (this._sftpSession.IsOpen)
      {
        try
        {
          this._sftpSession.EndClose(this._sftpSession.BeginClose(this._handle, (AsyncCallback) null, (object) null));
        }
        catch (Exception ex)
        {
          DiagnosticAbstraction.Log("Failure closing handle: " + ex?.ToString());
        }
      }
    }

    private void StartReadAhead() => ThreadAbstraction.ExecuteThread((Action) (() =>
    {
      while (!this._endOfFileReceived && this._exception == null)
      {
        if (!this.ContinueReadAhead())
        {
          lock (this._readLock)
          {
            Monitor.PulseAll(this._readLock);
            break;
          }
        }
        else if (this._semaphore.Wait(1000))
        {
          if (!this._endOfFileReceived && this._exception == null)
          {
            SftpFileReader.BufferedRead bufferedRead = new SftpFileReader.BufferedRead(this._readAheadChunkIndex, this._readAheadOffset);
            try
            {
              if (this._fileSize.HasValue && (long) this._readAheadOffset > this._fileSize.Value)
              {
                byte[] data = this._sftpSession.EndRead(this._sftpSession.BeginRead(this._handle, this._readAheadOffset, this._chunkSize, (AsyncCallback) null, (object) bufferedRead));
                this.ReadCompletedCore(bufferedRead, data);
              }
              else
                this._sftpSession.BeginRead(this._handle, this._readAheadOffset, this._chunkSize, new AsyncCallback(this.ReadCompleted), (object) bufferedRead);
            }
            catch (Exception ex)
            {
              this.HandleFailure(ex);
              break;
            }
            this._readAheadOffset += (ulong) this._chunkSize;
            ++this._readAheadChunkIndex;
          }
          else
            break;
        }
      }
      this._readAheadCompleted.Set();
    }));

    private bool ContinueReadAhead()
    {
      try
      {
        int num = this._sftpSession.WaitAny(this._waitHandles, this._sftpSession.OperationTimeout);
        switch (num)
        {
          case 0:
            return false;
          case 1:
            return true;
          default:
            throw new NotImplementedException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", (object) num));
        }
      }
      catch (Exception ex)
      {
        Interlocked.CompareExchange<Exception>(ref this._exception, ex, (Exception) null);
        return false;
      }
    }

    private void ReadCompleted(IAsyncResult result)
    {
      if (this._disposingOrDisposed)
        return;
      SftpReadAsyncResult sftpReadAsyncResult = (SftpReadAsyncResult) result;
      byte[] data;
      try
      {
        data = sftpReadAsyncResult.EndInvoke();
      }
      catch (Exception ex)
      {
        this.HandleFailure(ex);
        return;
      }
      this.ReadCompletedCore((SftpFileReader.BufferedRead) sftpReadAsyncResult.AsyncState, data);
    }

    private void ReadCompletedCore(SftpFileReader.BufferedRead bufferedRead, byte[] data)
    {
      bufferedRead.Complete(data);
      lock (this._readLock)
      {
        this._queue.Add(bufferedRead.ChunkIndex, bufferedRead);
        Monitor.PulseAll(this._readLock);
      }
      if (data.Length != 0)
        return;
      this._endOfFileReceived = true;
    }

    private void HandleFailure(Exception cause)
    {
      Interlocked.CompareExchange<Exception>(ref this._exception, cause, (Exception) null);
      this._semaphore.Release();
      lock (this._readLock)
        Monitor.PulseAll(this._readLock);
    }

    internal class BufferedRead
    {
      public int ChunkIndex { get; private set; }

      public byte[] Data { get; private set; }

      public ulong Offset { get; private set; }

      public BufferedRead(int chunkIndex, ulong offset)
      {
        this.ChunkIndex = chunkIndex;
        this.Offset = offset;
      }

      public void Complete(byte[] data) => this.Data = data;
    }
  }
}
