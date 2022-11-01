// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.TripleDesCipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public sealed class TripleDesCipher : DesCipher
  {
    private int[] _encryptionKey1;
    private int[] _encryptionKey2;
    private int[] _encryptionKey3;
    private int[] _decryptionKey1;
    private int[] _decryptionKey2;
    private int[] _decryptionKey3;

    public TripleDesCipher(byte[] key, CipherMode mode, CipherPadding padding)
      : base(key, mode, padding)
    {
    }

    public override int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputOffset + (int) this.BlockSize > inputBuffer.Length)
        throw new IndexOutOfRangeException("input buffer too short");
      if (outputOffset + (int) this.BlockSize > outputBuffer.Length)
        throw new IndexOutOfRangeException("output buffer too short");
      if (this._encryptionKey1 == null || this._encryptionKey2 == null || this._encryptionKey3 == null)
      {
        byte[] numArray1 = new byte[8];
        byte[] numArray2 = new byte[8];
        Buffer.BlockCopy((Array) this.Key, 0, (Array) numArray1, 0, 8);
        Buffer.BlockCopy((Array) this.Key, 8, (Array) numArray2, 0, 8);
        this._encryptionKey1 = this.GenerateWorkingKey(true, numArray1);
        this._encryptionKey2 = this.GenerateWorkingKey(false, numArray2);
        if (this.Key.Length == 24)
        {
          byte[] numArray3 = new byte[8];
          Buffer.BlockCopy((Array) this.Key, 16, (Array) numArray3, 0, 8);
          this._encryptionKey3 = this.GenerateWorkingKey(true, numArray3);
        }
        else
          this._encryptionKey3 = this._encryptionKey1;
      }
      byte[] numArray = new byte[(int) this.BlockSize];
      DesCipher.DesFunc(this._encryptionKey1, inputBuffer, inputOffset, numArray, 0);
      DesCipher.DesFunc(this._encryptionKey2, numArray, 0, numArray, 0);
      DesCipher.DesFunc(this._encryptionKey3, numArray, 0, outputBuffer, outputOffset);
      return (int) this.BlockSize;
    }

    public override int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputOffset + (int) this.BlockSize > inputBuffer.Length)
        throw new IndexOutOfRangeException("input buffer too short");
      if (outputOffset + (int) this.BlockSize > outputBuffer.Length)
        throw new IndexOutOfRangeException("output buffer too short");
      if (this._decryptionKey1 == null || this._decryptionKey2 == null || this._decryptionKey3 == null)
      {
        byte[] numArray1 = new byte[8];
        byte[] numArray2 = new byte[8];
        Buffer.BlockCopy((Array) this.Key, 0, (Array) numArray1, 0, 8);
        Buffer.BlockCopy((Array) this.Key, 8, (Array) numArray2, 0, 8);
        this._decryptionKey1 = this.GenerateWorkingKey(false, numArray1);
        this._decryptionKey2 = this.GenerateWorkingKey(true, numArray2);
        if (this.Key.Length == 24)
        {
          byte[] numArray3 = new byte[8];
          Buffer.BlockCopy((Array) this.Key, 16, (Array) numArray3, 0, 8);
          this._decryptionKey3 = this.GenerateWorkingKey(false, numArray3);
        }
        else
          this._decryptionKey3 = this._decryptionKey1;
      }
      byte[] numArray = new byte[(int) this.BlockSize];
      DesCipher.DesFunc(this._decryptionKey3, inputBuffer, inputOffset, numArray, 0);
      DesCipher.DesFunc(this._decryptionKey2, numArray, 0, numArray, 0);
      DesCipher.DesFunc(this._decryptionKey1, numArray, 0, outputBuffer, outputOffset);
      return (int) this.BlockSize;
    }

    protected override void ValidateKey()
    {
      int num = this.Key.Length * 8;
      if (num != 128 && num != 192)
        throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", (object) num));
    }
  }
}
