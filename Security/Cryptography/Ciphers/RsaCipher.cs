// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.RsaCipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public class RsaCipher : AsymmetricCipher
  {
    private readonly bool _isPrivate;
    private readonly RsaKey _key;

    public RsaCipher(RsaKey key)
    {
      this._key = key != null ? key : throw new ArgumentNullException(nameof (key));
      this._isPrivate = !this._key.D.IsZero;
    }

    public override byte[] Encrypt(byte[] data, int offset, int length)
    {
      int bitLength = this._key.Modulus.BitLength;
      byte[] numArray = new byte[bitLength / 8 + (bitLength % 8 > 0 ? 1 : 0) - 1];
      numArray[0] = (byte) 1;
      for (int index = 1; index < numArray.Length - length - 1; ++index)
        numArray[index] = byte.MaxValue;
      Buffer.BlockCopy((Array) data, offset, (Array) numArray, numArray.Length - length, length);
      return this.Transform(numArray);
    }

    public override byte[] Decrypt(byte[] data) => this.Decrypt(data, 0, data.Length);

    public override byte[] Decrypt(byte[] data, int offset, int length)
    {
      byte[] src = this.Transform(data, offset, length);
      if (src[0] != (byte) 1 && src[0] != (byte) 2)
        throw new NotSupportedException("Only block type 01 or 02 are supported.");
      int index = 1;
      while (index < src.Length && src[index] > (byte) 0)
        ++index;
      int srcOffset = index + 1;
      byte[] dst = new byte[src.Length - srcOffset];
      Buffer.BlockCopy((Array) src, srcOffset, (Array) dst, 0, dst.Length);
      return dst;
    }

    private byte[] Transform(byte[] data) => this.Transform(data, 0, data.Length);

    private byte[] Transform(byte[] data, int offset, int length)
    {
      Array.Reverse<byte>(data, offset, length);
      byte[] dst = new byte[length + 1];
      Buffer.BlockCopy((Array) data, offset, (Array) dst, 0, length);
      BigInteger bigInteger1 = new BigInteger(dst);
      BigInteger bigInteger2;
      if (this._isPrivate)
      {
        BigInteger bi = BigInteger.One;
        BigInteger bigInteger3 = this._key.Modulus - (BigInteger) 1;
        int bitLength = this._key.Modulus.BitLength;
        if (bigInteger3 < BigInteger.One)
          throw new SshException("Invalid RSA key.");
        while (bi <= BigInteger.One || bi >= bigInteger3)
          bi = BigInteger.Random(bitLength);
        BigInteger bigInteger4 = BigInteger.PositiveMod(BigInteger.ModPow(bi, this._key.Exponent, this._key.Modulus) * bigInteger1, this._key.Modulus);
        BigInteger bigInteger5 = BigInteger.ModPow(bigInteger4 % this._key.P, this._key.DP, this._key.P);
        BigInteger bigInteger6 = BigInteger.ModPow(bigInteger4 % this._key.Q, this._key.DQ, this._key.Q);
        bigInteger2 = BigInteger.PositiveMod((BigInteger.PositiveMod((bigInteger5 - bigInteger6) * this._key.InverseQ, this._key.P) * this._key.Q + bigInteger6) * BigInteger.ModInverse(bi, this._key.Modulus), this._key.Modulus);
      }
      else
        bigInteger2 = BigInteger.ModPow(bigInteger1, this._key.Exponent, this._key.Modulus);
      return bigInteger2.ToByteArray().Reverse<byte>();
    }
  }
}
