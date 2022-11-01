// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.Modes.CtrCipherMode
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
  public class CtrCipherMode : CipherMode
  {
    private readonly byte[] _ivOutput;

    public CtrCipherMode(byte[] iv)
      : base(iv)
    {
      this._ivOutput = new byte[iv.Length];
    }

    public override int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputBuffer.Length - inputOffset < this._blockSize)
        throw new ArgumentException("Invalid input buffer");
      if (outputBuffer.Length - outputOffset < this._blockSize)
        throw new ArgumentException("Invalid output buffer");
      if (inputCount != this._blockSize)
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "inputCount must be {0}.", (object) this._blockSize));
      this.Cipher.EncryptBlock(this.IV, 0, this.IV.Length, this._ivOutput, 0);
      for (int index = 0; index < this._blockSize; ++index)
        outputBuffer[outputOffset + index] = (byte) ((uint) this._ivOutput[index] ^ (uint) inputBuffer[inputOffset + index]);
      int length = this.IV.Length;
      do
        ;
      while (--length >= 0 && ++this.IV[length] == (byte) 0);
      return this._blockSize;
    }

    public override int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputBuffer.Length - inputOffset < this._blockSize)
        throw new ArgumentException("Invalid input buffer");
      if (outputBuffer.Length - outputOffset < this._blockSize)
        throw new ArgumentException("Invalid output buffer");
      if (inputCount != this._blockSize)
        throw new ArgumentException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "inputCount must be {0}.", (object) this._blockSize));
      this.Cipher.EncryptBlock(this.IV, 0, this.IV.Length, this._ivOutput, 0);
      for (int index = 0; index < this._blockSize; ++index)
        outputBuffer[outputOffset + index] = (byte) ((uint) this._ivOutput[index] ^ (uint) inputBuffer[inputOffset + index]);
      int length = this.IV.Length;
      do
        ;
      while (--length >= 0 && ++this.IV[length] == (byte) 0);
      return this._blockSize;
    }
  }
}
