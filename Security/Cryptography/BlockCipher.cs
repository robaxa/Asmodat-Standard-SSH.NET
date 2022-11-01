// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.BlockCipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Security.Cryptography.Ciphers;
using System;

namespace Renci.SshNet.Security.Cryptography
{
  public abstract class BlockCipher : SymmetricCipher
  {
    private readonly CipherMode _mode;
    private readonly CipherPadding _padding;
    private readonly byte _blockSize;

    public override byte MinimumSize => this.BlockSize;

    public byte BlockSize => this._blockSize;

    protected BlockCipher(byte[] key, byte blockSize, CipherMode mode, CipherPadding padding)
      : base(key)
    {
      this._blockSize = blockSize;
      this._mode = mode;
      this._padding = padding;
      if (this._mode == null)
        return;
      this._mode.Init(this);
    }

    public override byte[] Encrypt(byte[] data, int offset, int length)
    {
      if (length % (int) this._blockSize > 0)
      {
        if (this._padding == null)
          throw new ArgumentException(nameof (data));
        int paddinglength = (int) this._blockSize - length % (int) this._blockSize;
        data = this._padding.Pad(data, offset, length, paddinglength);
        length += paddinglength;
        offset = 0;
      }
      byte[] outputBuffer = new byte[length];
      int num = 0;
      for (int index = 0; index < length / (int) this._blockSize; ++index)
      {
        if (this._mode == null)
          num += this.EncryptBlock(data, offset + index * (int) this._blockSize, (int) this._blockSize, outputBuffer, index * (int) this._blockSize);
        else
          num += this._mode.EncryptBlock(data, offset + index * (int) this._blockSize, (int) this._blockSize, outputBuffer, index * (int) this._blockSize);
      }
      if (num < length)
        throw new InvalidOperationException("Encryption error.");
      return outputBuffer;
    }

    public override byte[] Decrypt(byte[] data) => this.Decrypt(data, 0, data.Length);

    public override byte[] Decrypt(byte[] data, int offset, int length)
    {
      if (length % (int) this._blockSize > 0)
      {
        if (this._padding == null)
          throw new ArgumentException(nameof (data));
        data = this._padding.Pad((int) this._blockSize, data, offset, length);
        offset = 0;
        length = data.Length;
      }
      byte[] outputBuffer = new byte[length];
      int num = 0;
      for (int index = 0; index < length / (int) this._blockSize; ++index)
      {
        if (this._mode == null)
          num += this.DecryptBlock(data, offset + index * (int) this._blockSize, (int) this._blockSize, outputBuffer, index * (int) this._blockSize);
        else
          num += this._mode.DecryptBlock(data, offset + index * (int) this._blockSize, (int) this._blockSize, outputBuffer, index * (int) this._blockSize);
      }
      if (num < length)
        throw new InvalidOperationException("Encryption error.");
      return outputBuffer;
    }
  }
}
