// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.DsaDigitalSignature
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
  public class DsaDigitalSignature : DigitalSignature, IDisposable
  {
    private HashAlgorithm _hash;
    private readonly DsaKey _key;
    private bool _isDisposed;

    public DsaDigitalSignature(DsaKey key)
    {
      this._key = key != null ? key : throw new ArgumentNullException(nameof (key));
      this._hash = (HashAlgorithm) CryptoAbstraction.CreateSHA1();
    }

    public override bool Verify(byte[] input, byte[] signature)
    {
      BigInteger bigInteger1 = new BigInteger(this._hash.ComputeHash(input).Reverse<byte>().Concat(new byte[1]));
      if (signature.Length != 40)
        throw new InvalidOperationException("Invalid signature.");
      byte[] numArray1 = new byte[21];
      byte[] numArray2 = new byte[21];
      int index = 0;
      int num = 20;
      while (index < 20)
      {
        numArray1[index] = signature[num - 1];
        numArray2[index] = signature[num + 20 - 1];
        ++index;
        --num;
      }
      BigInteger bigInteger2 = new BigInteger(numArray1);
      BigInteger bi = new BigInteger(numArray2);
      if (bigInteger2 <= 0L || bigInteger2 >= this._key.Q || bi <= 0L || bi >= this._key.Q)
        return false;
      BigInteger bigInteger3 = BigInteger.ModInverse(bi, this._key.Q);
      BigInteger exponent1 = bigInteger1 * bigInteger3 % this._key.Q;
      BigInteger exponent2 = bigInteger2 * bigInteger3 % this._key.Q;
      return BigInteger.ModPow(this._key.G, exponent1, this._key.P) * BigInteger.ModPow(this._key.Y, exponent2, this._key.P) % this._key.P % this._key.Q == bigInteger2;
    }

    public override byte[] Sign(byte[] input)
    {
      BigInteger bigInteger1 = new BigInteger(this._hash.ComputeHash(input).Reverse<byte>().Concat(new byte[1]));
      BigInteger bigInteger2;
      BigInteger bigInteger3;
      do
      {
        BigInteger bigInteger4 = BigInteger.Zero;
        do
        {
          int bitLength = this._key.Q.BitLength;
          if (this._key.Q < BigInteger.Zero)
            throw new SshException("Invalid DSA key.");
          while (bigInteger4 <= 0L || bigInteger4 >= this._key.Q)
            bigInteger4 = BigInteger.Random(bitLength);
          bigInteger2 = BigInteger.ModPow(this._key.G, bigInteger4, this._key.P) % this._key.Q;
        }
        while (bigInteger2.IsZero);
        bigInteger3 = BigInteger.ModInverse(bigInteger4, this._key.Q) * (bigInteger1 + this._key.X * bigInteger2) % this._key.Q;
      }
      while (bigInteger3.IsZero);
      byte[] destinationArray = new byte[40];
      byte[] sourceArray1 = bigInteger2.ToByteArray().Reverse<byte>().TrimLeadingZeros();
      Array.Copy((Array) sourceArray1, 0, (Array) destinationArray, 20 - sourceArray1.Length, sourceArray1.Length);
      byte[] sourceArray2 = bigInteger3.ToByteArray().Reverse<byte>().TrimLeadingZeros();
      Array.Copy((Array) sourceArray2, 0, (Array) destinationArray, 40 - sourceArray2.Length, sourceArray2.Length);
      return destinationArray;
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
      HashAlgorithm hash = this._hash;
      if (hash != null)
      {
        hash.Dispose();
        this._hash = (HashAlgorithm) null;
      }
      this._isDisposed = true;
    }

    ~DsaDigitalSignature() => this.Dispose(false);
  }
}
