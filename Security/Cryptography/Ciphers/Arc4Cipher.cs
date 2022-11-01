// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.Arc4Cipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public sealed class Arc4Cipher : StreamCipher
  {
    private static readonly int STATE_LENGTH = 256;
    private byte[] _engineState;
    private int _x;
    private int _y;
    private byte[] _workingKey;

    public override byte MinimumSize => 0;

    public Arc4Cipher(byte[] key, bool dischargeFirstBytes)
      : base(key)
    {
      this._workingKey = key;
      this.SetKey(this._workingKey);
      if (!dischargeFirstBytes)
        return;
      this.Encrypt(new byte[1536]);
    }

    public override int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      return this.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    public override int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      return this.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }

    public override byte[] Encrypt(byte[] input, int offset, int length)
    {
      byte[] outputBuffer = new byte[length];
      this.ProcessBytes(input, offset, length, outputBuffer, 0);
      return outputBuffer;
    }

    public override byte[] Decrypt(byte[] input) => this.Decrypt(input, 0, input.Length);

    public override byte[] Decrypt(byte[] input, int offset, int length)
    {
      byte[] outputBuffer = new byte[length];
      this.ProcessBytes(input, offset, length, outputBuffer, 0);
      return outputBuffer;
    }

    private int ProcessBytes(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputOffset + inputCount > inputBuffer.Length)
        throw new IndexOutOfRangeException("input buffer too short");
      if (outputOffset + inputCount > outputBuffer.Length)
        throw new IndexOutOfRangeException("output buffer too short");
      for (int index = 0; index < inputCount; ++index)
      {
        this._x = this._x + 1 & (int) byte.MaxValue;
        this._y = (int) this._engineState[this._x] + this._y & (int) byte.MaxValue;
        byte num = this._engineState[this._x];
        this._engineState[this._x] = this._engineState[this._y];
        this._engineState[this._y] = num;
        outputBuffer[index + outputOffset] = (byte) ((uint) inputBuffer[index + inputOffset] ^ (uint) this._engineState[(int) this._engineState[this._x] + (int) this._engineState[this._y] & (int) byte.MaxValue]);
      }
      return inputCount;
    }

    private void SetKey(byte[] keyBytes)
    {
      this._workingKey = keyBytes;
      this._x = 0;
      this._y = 0;
      if (this._engineState == null)
        this._engineState = new byte[Arc4Cipher.STATE_LENGTH];
      for (int index = 0; index < Arc4Cipher.STATE_LENGTH; ++index)
        this._engineState[index] = (byte) index;
      int index1 = 0;
      int index2 = 0;
      for (int index3 = 0; index3 < Arc4Cipher.STATE_LENGTH; ++index3)
      {
        index2 = ((int) keyBytes[index1] & (int) byte.MaxValue) + (int) this._engineState[index3] + index2 & (int) byte.MaxValue;
        byte num = this._engineState[index3];
        this._engineState[index3] = this._engineState[index2];
        this._engineState[index2] = num;
        index1 = (index1 + 1) % keyBytes.Length;
      }
    }
  }
}
