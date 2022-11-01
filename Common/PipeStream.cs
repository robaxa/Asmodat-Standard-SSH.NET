// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.PipeStream
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Renci.SshNet.Common
{
  public class PipeStream : Stream
  {
    private readonly Queue<byte> _buffer = new Queue<byte>();
    private bool _isFlushed;
    private long _maxBufferLength = 209715200;
    private bool _canBlockLastRead;
    private bool _isDisposed;

    public long MaxBufferLength
    {
      get => this._maxBufferLength;
      set => this._maxBufferLength = value;
    }

    public bool BlockLastReadBuffer
    {
      get
      {
        if (this._isDisposed)
          throw this.CreateObjectDisposedException();
        return this._canBlockLastRead;
      }
      set
      {
        if (this._isDisposed)
          throw this.CreateObjectDisposedException();
        this._canBlockLastRead = value;
        if (this._canBlockLastRead)
          return;
        lock (this._buffer)
          Monitor.Pulse((object) this._buffer);
      }
    }

    public override void Flush()
    {
      if (this._isDisposed)
        throw this.CreateObjectDisposedException();
      this._isFlushed = true;
      lock (this._buffer)
        Monitor.Pulse((object) this._buffer);
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (offset != 0)
        throw new NotSupportedException("Offsets with value of non-zero are not supported");
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (offset + count > buffer.Length)
        throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
      if (offset < 0 || count < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), "offset or count is negative.");
      if (this.BlockLastReadBuffer && (long) count >= this._maxBufferLength)
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "count({0}) > mMaxBufferLength({1})", (object) count, (object) this._maxBufferLength));
      if (this._isDisposed)
        throw this.CreateObjectDisposedException();
      if (count == 0)
        return 0;
      int index = 0;
      lock (this._buffer)
      {
        while (!this._isDisposed && !this.ReadAvailable(count))
          Monitor.Wait((object) this._buffer);
        if (this._isDisposed)
          return 0;
        for (; index < count && this._buffer.Count > 0; ++index)
          buffer[index] = this._buffer.Dequeue();
        Monitor.Pulse((object) this._buffer);
      }
      return index;
    }

    private bool ReadAvailable(int count)
    {
      long length = this.Length;
      return (this._isFlushed || length >= (long) count) && (length >= (long) (count + 1) || !this.BlockLastReadBuffer);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (offset + count > buffer.Length)
        throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
      if (offset < 0 || count < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), "offset or count is negative.");
      if (this._isDisposed)
        throw this.CreateObjectDisposedException();
      if (count == 0)
        return;
      lock (this._buffer)
      {
        while (this.Length >= this._maxBufferLength)
          Monitor.Wait((object) this._buffer);
        this._isFlushed = false;
        for (int index = offset; index < offset + count; ++index)
          this._buffer.Enqueue(buffer[index]);
        Monitor.Pulse((object) this._buffer);
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this._isDisposed)
        return;
      lock (this._buffer)
      {
        this._isDisposed = true;
        Monitor.Pulse((object) this._buffer);
      }
    }

    public override bool CanRead => !this._isDisposed;

    public override bool CanSeek => false;

    public override bool CanWrite => !this._isDisposed;

    public override long Length
    {
      get
      {
        if (this._isDisposed)
          throw this.CreateObjectDisposedException();
        return (long) this._buffer.Count;
      }
    }

    public override long Position
    {
      get => 0;
      set => throw new NotSupportedException();
    }

    private ObjectDisposedException CreateObjectDisposedException() => new ObjectDisposedException(this.GetType().FullName);
  }
}
