// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.RsaKey
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using System;

namespace Renci.SshNet.Security
{
  public class RsaKey : Key, IDisposable
  {
    private RsaDigitalSignature _digitalSignature;
    private bool _isDisposed;

    public BigInteger Modulus => this._privateKey[0];

    public BigInteger Exponent => this._privateKey[1];

    public BigInteger D => this._privateKey.Length > 2 ? this._privateKey[2] : BigInteger.Zero;

    public BigInteger P => this._privateKey.Length > 3 ? this._privateKey[3] : BigInteger.Zero;

    public BigInteger Q => this._privateKey.Length > 4 ? this._privateKey[4] : BigInteger.Zero;

    public BigInteger DP => this._privateKey.Length > 5 ? this._privateKey[5] : BigInteger.Zero;

    public BigInteger DQ => this._privateKey.Length > 6 ? this._privateKey[6] : BigInteger.Zero;

    public BigInteger InverseQ => this._privateKey.Length > 7 ? this._privateKey[7] : BigInteger.Zero;

    public override int KeyLength => this.Modulus.BitLength;

    protected override DigitalSignature DigitalSignature
    {
      get
      {
        if (this._digitalSignature == null)
          this._digitalSignature = new RsaDigitalSignature(this);
        return (DigitalSignature) this._digitalSignature;
      }
    }

    public override BigInteger[] Public
    {
      get => new BigInteger[2]
      {
        this.Exponent,
        this.Modulus
      };
      set => this._privateKey = value.Length == 2 ? new BigInteger[2]
      {
        value[1],
        value[0]
      } : throw new InvalidOperationException("Invalid private key.");
    }

    public RsaKey()
    {
    }

    public RsaKey(byte[] data)
      : base(data)
    {
      if (this._privateKey.Length != 8)
        throw new InvalidOperationException("Invalid private key.");
    }

    public RsaKey(
      BigInteger modulus,
      BigInteger exponent,
      BigInteger d,
      BigInteger p,
      BigInteger q,
      BigInteger inverseQ)
    {
      this._privateKey = new BigInteger[8];
      this._privateKey[0] = modulus;
      this._privateKey[1] = exponent;
      this._privateKey[2] = d;
      this._privateKey[3] = p;
      this._privateKey[4] = q;
      this._privateKey[5] = RsaKey.PrimeExponent(d, p);
      this._privateKey[6] = RsaKey.PrimeExponent(d, q);
      this._privateKey[7] = inverseQ;
    }

    private static BigInteger PrimeExponent(BigInteger privateExponent, BigInteger prime)
    {
      BigInteger bigInteger = prime - new BigInteger(1);
      return privateExponent % bigInteger;
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
      RsaDigitalSignature digitalSignature = this._digitalSignature;
      if (digitalSignature != null)
      {
        digitalSignature.Dispose();
        this._digitalSignature = (RsaDigitalSignature) null;
      }
      this._isDisposed = true;
    }

    ~RsaKey() => this.Dispose(false);
  }
}
