// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpFileStream
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.IO;
using System.Threading;

namespace Renci.SshNet.Sftp
{
  public class SftpFileStream : Stream
  {
    private byte[] _handle;
    private ISftpSession _session;
    private readonly int _readBufferSize;
    private byte[] _readBuffer;
    private readonly int _writeBufferSize;
    private byte[] _writeBuffer;
    private int _bufferPosition;
    private int _bufferLen;
    private long _position;
    private bool _bufferOwnedByWrite;
    private bool _canRead;
    private bool _canSeek;
    private bool _canWrite;
    private readonly object _lock = new object();

    public override bool CanRead => this._canRead;

    public override bool CanSeek => this._canSeek;

    public override bool CanWrite => this._canWrite;

    public override bool CanTimeout => true;

    public override long Length
    {
      get
      {
        lock (this._lock)
        {
          this.CheckSessionIsOpen();
          if (!this.CanSeek)
            throw new NotSupportedException("Seek operation is not supported.");
          if (this._bufferOwnedByWrite)
            this.FlushWriteBuffer();
          return (this._session.RequestFStat(this._handle, true) ?? throw new IOException("Seek operation failed.")).Size;
        }
      }
    }

    public override long Position
    {
      get
      {
        this.CheckSessionIsOpen();
        if (!this.CanSeek)
          throw new NotSupportedException("Seek operation not supported.");
        return this._position;
      }
      set => this.Seek(value, SeekOrigin.Begin);
    }

    public string Name { get; private set; }

    public virtual byte[] Handle
    {
      get
      {
        this.Flush();
        return this._handle;
      }
    }

    public TimeSpan Timeout { get; set; }

    internal SftpFileStream(
      ISftpSession session,
      string path,
      FileMode mode,
      FileAccess access,
      int bufferSize)
    {
      if (session == null)
        throw new SshConnectionException("Client not connected.");
      if (path == null)
        throw new ArgumentNullException(nameof (path));
      if (bufferSize <= 0)
        throw new ArgumentOutOfRangeException(nameof (bufferSize));
      this.Timeout = TimeSpan.FromSeconds(30.0);
      this.Name = path;
      this._session = session;
      this._canRead = (access & FileAccess.Read) != 0;
      this._canSeek = true;
      this._canWrite = (access & FileAccess.Write) != 0;
      Flags flags1 = Flags.None;
      Flags flags2;
      switch (access)
      {
        case FileAccess.Read:
          flags2 = flags1 | Flags.Read;
          break;
        case FileAccess.Write:
          flags2 = flags1 | Flags.Write;
          break;
        case FileAccess.ReadWrite:
          flags2 = flags1 | Flags.Read | Flags.Write;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof (access));
      }
      if ((access & FileAccess.Read) != (FileAccess) 0 && mode == FileMode.Append)
        throw new ArgumentException(string.Format("{0} mode can be requested only when combined with write-only access.", (object) mode.ToString("G")));
      if ((access & FileAccess.Write) == (FileAccess) 0 && (mode == FileMode.Create || mode == FileMode.CreateNew || mode == FileMode.Truncate || mode == FileMode.Append))
        throw new ArgumentException(string.Format("Combining {0}: {1} with {2}: {3} is invalid.", (object) typeof (FileMode).Name, (object) mode, (object) typeof (FileAccess).Name, (object) access));
      switch (mode)
      {
        case FileMode.CreateNew:
          flags2 |= Flags.CreateNew;
          goto case FileMode.Open;
        case FileMode.Create:
          this._handle = this._session.RequestOpen(path, flags2 | Flags.Truncate, true);
          if (this._handle == null)
          {
            flags2 |= Flags.CreateNew;
            goto case FileMode.Open;
          }
          else
          {
            flags2 |= Flags.Truncate;
            goto case FileMode.Open;
          }
        case FileMode.Open:
          if (this._handle == null)
            this._handle = this._session.RequestOpen(path, flags2);
          this._readBufferSize = (int) session.CalculateOptimalReadLength((uint) bufferSize);
          this._writeBufferSize = (int) session.CalculateOptimalWriteLength((uint) bufferSize, this._handle);
          if (mode != FileMode.Append)
            break;
          this._position = this._session.RequestFStat(this._handle, false).Size;
          break;
        case FileMode.OpenOrCreate:
          flags2 |= Flags.CreateNewOrOpen;
          goto case FileMode.Open;
        case FileMode.Truncate:
          flags2 |= Flags.Truncate;
          goto case FileMode.Open;
        case FileMode.Append:
          flags2 |= Flags.Append | Flags.CreateNewOrOpen;
          goto case FileMode.Open;
        default:
          throw new ArgumentOutOfRangeException(nameof (mode));
      }
    }

    ~SftpFileStream() => this.Dispose(false);

    public override void Flush()
    {
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        if (this._bufferOwnedByWrite)
          this.FlushWriteBuffer();
        else
          this.FlushReadBuffer();
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int num = 0;
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count));
      if (buffer.Length - offset < count)
        throw new ArgumentException("Invalid array range.");
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        this.SetupRead();
        while (count > 0)
        {
          int count1 = this._bufferLen - this._bufferPosition;
          if (count1 <= 0)
          {
            byte[] src = this._session.RequestRead(this._handle, (ulong) this._position, (uint) this._readBufferSize);
            if (src.Length == 0)
            {
              this._bufferPosition = 0;
              this._bufferLen = 0;
              break;
            }
            int count2 = count;
            if (count2 >= src.Length)
            {
              count2 = src.Length;
              this._bufferPosition = 0;
              this._bufferLen = 0;
            }
            else
            {
              int count3 = src.Length - count2;
              Buffer.BlockCopy((Array) src, count, (Array) this.GetOrCreateReadBuffer(), 0, count3);
              this._bufferPosition = 0;
              this._bufferLen = count3;
            }
            Buffer.BlockCopy((Array) src, 0, (Array) buffer, offset, count2);
            this._position += (long) count2;
            num += count2;
            if (src.Length >= this._readBufferSize)
            {
              offset += count2;
              count -= count2;
            }
            else
              break;
          }
          else
          {
            if (count1 > count)
              count1 = count;
            Buffer.BlockCopy((Array) this.GetOrCreateReadBuffer(), this._bufferPosition, (Array) buffer, offset, count1);
            this._bufferPosition += count1;
            this._position += (long) count1;
            num += count1;
            offset += count1;
            count -= count1;
          }
        }
      }
      return num;
    }

    public override int ReadByte()
    {
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        this.SetupRead();
        byte[] readBuffer;
        if (this._bufferPosition >= this._bufferLen)
        {
          byte[] src = this._session.RequestRead(this._handle, (ulong) this._position, (uint) this._readBufferSize);
          if (src.Length == 0)
            return -1;
          readBuffer = this.GetOrCreateReadBuffer();
          Buffer.BlockCopy((Array) src, 0, (Array) readBuffer, 0, src.Length);
          this._bufferPosition = 0;
          this._bufferLen = src.Length;
        }
        else
          readBuffer = this.GetOrCreateReadBuffer();
        ++this._position;
        return (int) readBuffer[this._bufferPosition++];
      }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      long num = -1;
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        if (!this.CanSeek)
          throw new NotSupportedException("Seek is not supported.");
        if (origin == SeekOrigin.Begin && offset == this._position)
          return offset;
        if (origin == SeekOrigin.Current && offset == 0L)
          return this._position;
        if (this._bufferOwnedByWrite)
        {
          this.FlushWriteBuffer();
          switch (origin)
          {
            case SeekOrigin.Begin:
              num = offset;
              break;
            case SeekOrigin.Current:
              num = this._position + offset;
              break;
            case SeekOrigin.End:
              num = this._session.RequestFStat(this._handle, false).Size - offset;
              break;
          }
          this._position = num != -1L ? num : throw new EndOfStreamException("End of stream.");
        }
        else
        {
          switch (origin)
          {
            case SeekOrigin.Begin:
              num = this._position - (long) this._bufferPosition;
              if (offset >= num && offset < num + (long) this._bufferLen)
              {
                this._bufferPosition = (int) (offset - num);
                this._position = offset;
                return this._position;
              }
              break;
            case SeekOrigin.Current:
              num = this._position + offset;
              if (num >= this._position - (long) this._bufferPosition && num < this._position - (long) this._bufferPosition + (long) this._bufferLen)
              {
                this._bufferPosition = (int) (num - (this._position - (long) this._bufferPosition));
                this._position = num;
                return this._position;
              }
              break;
          }
          this._bufferPosition = 0;
          this._bufferLen = 0;
          switch (origin)
          {
            case SeekOrigin.Begin:
              num = offset;
              break;
            case SeekOrigin.Current:
              num = this._position + offset;
              break;
            case SeekOrigin.End:
              num = this._session.RequestFStat(this._handle, false).Size - offset;
              break;
          }
          this._position = num >= 0L ? num : throw new EndOfStreamException();
        }
        return this._position;
      }
    }

    public override void SetLength(long value)
    {
      if (value < 0L)
        throw new ArgumentOutOfRangeException(nameof (value));
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        if (!this.CanSeek)
          throw new NotSupportedException("Seek is not supported.");
        if (this._bufferOwnedByWrite)
          this.FlushWriteBuffer();
        else
          this.SetupWrite();
        SftpFileAttributes attributes = this._session.RequestFStat(this._handle, false);
        attributes.Size = value;
        this._session.RequestFSetStat(this._handle, attributes);
        if (this._position <= value)
          return;
        this._position = value;
      }
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count));
      if (buffer.Length - offset < count)
        throw new ArgumentException("Invalid array range.");
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        this.SetupWrite();
        int num;
        for (; count > 0; count -= num)
        {
          num = this._writeBufferSize - this._bufferPosition;
          if (num <= 0)
          {
            this.FlushWriteBuffer();
            num = this._writeBufferSize;
          }
          if (num > count)
            num = count;
          if (this._bufferPosition == 0 && num == this._writeBufferSize)
          {
            using (AutoResetEvent wait = new AutoResetEvent(false))
              this._session.RequestWrite(this._handle, (ulong) this._position, buffer, offset, num, wait);
          }
          else
          {
            Buffer.BlockCopy((Array) buffer, offset, (Array) this.GetOrCreateWriteBuffer(), this._bufferPosition, num);
            this._bufferPosition += num;
          }
          this._position += (long) num;
          offset += num;
        }
        if (this._bufferPosition < this._writeBufferSize)
          return;
        using (AutoResetEvent wait = new AutoResetEvent(false))
          this._session.RequestWrite(this._handle, (ulong) this._position - (ulong) this._bufferPosition, this.GetOrCreateWriteBuffer(), 0, this._bufferPosition, wait);
        this._bufferPosition = 0;
      }
    }

    public override void WriteByte(byte value)
    {
      lock (this._lock)
      {
        this.CheckSessionIsOpen();
        this.SetupWrite();
        byte[] writeBuffer = this.GetOrCreateWriteBuffer();
        if (this._bufferPosition >= this._writeBufferSize)
        {
          using (AutoResetEvent wait = new AutoResetEvent(false))
            this._session.RequestWrite(this._handle, (ulong) this._position - (ulong) this._bufferPosition, writeBuffer, 0, this._bufferPosition, wait);
          this._bufferPosition = 0;
        }
        writeBuffer[this._bufferPosition++] = value;
        ++this._position;
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this._session == null || !disposing)
        return;
      lock (this._lock)
      {
        if (this._session != null)
        {
          this._canRead = false;
          this._canSeek = false;
          this._canWrite = false;
          if (this._handle != null)
          {
            if (this._session.IsOpen)
            {
              if (this._bufferOwnedByWrite)
                this.FlushWriteBuffer();
              this._session.RequestClose(this._handle);
            }
            this._handle = (byte[]) null;
          }
          this._session = (ISftpSession) null;
        }
      }
    }

    private byte[] GetOrCreateReadBuffer()
    {
      if (this._readBuffer == null)
        this._readBuffer = new byte[this._readBufferSize];
      return this._readBuffer;
    }

    private byte[] GetOrCreateWriteBuffer()
    {
      if (this._writeBuffer == null)
        this._writeBuffer = new byte[this._writeBufferSize];
      return this._writeBuffer;
    }

    private void FlushReadBuffer()
    {
      this._bufferPosition = 0;
      this._bufferLen = 0;
    }

    private void FlushWriteBuffer()
    {
      if (this._bufferPosition <= 0)
        return;
      using (AutoResetEvent wait = new AutoResetEvent(false))
        this._session.RequestWrite(this._handle, (ulong) this._position - (ulong) this._bufferPosition, this._writeBuffer, 0, this._bufferPosition, wait);
      this._bufferPosition = 0;
    }

    private void SetupRead()
    {
      if (!this.CanRead)
        throw new NotSupportedException("Read not supported.");
      if (!this._bufferOwnedByWrite)
        return;
      this.FlushWriteBuffer();
      this._bufferOwnedByWrite = false;
    }

    private void SetupWrite()
    {
      if (!this.CanWrite)
        throw new NotSupportedException("Write not supported.");
      if (this._bufferOwnedByWrite)
        return;
      this.FlushReadBuffer();
      this._bufferOwnedByWrite = true;
    }

    private void CheckSessionIsOpen()
    {
      if (this._session == null)
        throw new ObjectDisposedException(this.GetType().FullName);
      if (!this._session.IsOpen)
        throw new ObjectDisposedException(this.GetType().FullName, "Cannot access a closed SFTP session.");
    }
  }
}
