// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.Modes.CbcCipherMode
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
  public class CbcCipherMode : CipherMode
  {
    public CbcCipherMode(byte[] iv)
      : base(iv)
    {
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
      for (int index = 0; index < this._blockSize; ++index)
        this.IV[index] ^= inputBuffer[inputOffset + index];
      this.Cipher.EncryptBlock(this.IV, 0, inputCount, outputBuffer, outputOffset);
      Buffer.BlockCopy((Array) outputBuffer, outputOffset, (Array) this.IV, 0, this.IV.Length);
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
      this.Cipher.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
      for (int index = 0; index < this._blockSize; ++index)
        outputBuffer[outputOffset + index] ^= this.IV[index];
      Buffer.BlockCopy((Array) inputBuffer, inputOffset, (Array) this.IV, 0, this.IV.Length);
      return this._blockSize;
    }
  }
}
