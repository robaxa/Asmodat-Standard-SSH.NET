// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.CipherMode
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public abstract class CipherMode
  {
    protected BlockCipher Cipher;
    protected byte[] IV;
    protected int _blockSize;

    protected CipherMode(byte[] iv) => this.IV = iv;

    internal void Init(BlockCipher cipher)
    {
      this.Cipher = cipher;
      this._blockSize = (int) cipher.BlockSize;
      this.IV = this.IV.Take(this._blockSize);
    }

    public abstract int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset);

    public abstract int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset);
  }
}
