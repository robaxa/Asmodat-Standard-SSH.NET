// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Compression.Compressor
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Security;
using System;
using System.IO;

namespace Renci.SshNet.Compression
{
  public abstract class Compressor : Algorithm, IDisposable
  {
    private readonly ZlibStream _compressor;
    private readonly ZlibStream _decompressor;
    private MemoryStream _compressorStream;
    private MemoryStream _decompressorStream;
    private bool _isDisposed;

    protected bool IsActive { get; set; }

    protected Session Session { get; private set; }

    protected Compressor()
    {
      this._compressorStream = new MemoryStream();
      this._decompressorStream = new MemoryStream();
      this._compressor = new ZlibStream((Stream) this._compressorStream, CompressionMode.Compress);
      this._decompressor = new ZlibStream((Stream) this._decompressorStream, CompressionMode.Decompress);
    }

    public virtual void Init(Session session) => this.Session = session;

    public virtual byte[] Compress(byte[] data) => this.Compress(data, 0, data.Length);

    public virtual byte[] Compress(byte[] data, int offset, int length)
    {
      if (!this.IsActive)
      {
        if (offset == 0 && length == data.Length)
          return data;
        byte[] dst = new byte[length];
        Buffer.BlockCopy((Array) data, offset, (Array) dst, 0, length);
        return dst;
      }
      this._compressorStream.SetLength(0L);
      this._compressor.Write(data, offset, length);
      return this._compressorStream.ToArray();
    }

    public virtual byte[] Decompress(byte[] data) => this.Decompress(data, 0, data.Length);

    public virtual byte[] Decompress(byte[] data, int offset, int length)
    {
      if (!this.IsActive)
      {
        if (offset == 0 && length == data.Length)
          return data;
        byte[] dst = new byte[length];
        Buffer.BlockCopy((Array) data, offset, (Array) dst, 0, length);
        return dst;
      }
      this._decompressorStream.SetLength(0L);
      this._decompressor.Write(data, offset, length);
      return this._decompressorStream.ToArray();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._isDisposed || !disposing)
        return;
      MemoryStream compressorStream = this._compressorStream;
      if (compressorStream != null)
      {
        compressorStream.Dispose();
        this._compressorStream = (MemoryStream) null;
      }
      MemoryStream decompressorStream = this._decompressorStream;
      if (decompressorStream != null)
      {
        decompressorStream.Dispose();
        this._decompressorStream = (MemoryStream) null;
      }
      this._isDisposed = true;
    }

    ~Compressor() => this.Dispose(false);
  }
}
