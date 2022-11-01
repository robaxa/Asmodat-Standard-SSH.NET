// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.DsaKey
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using System;

namespace Renci.SshNet.Security
{
  public class DsaKey : Key, IDisposable
  {
    private DsaDigitalSignature _digitalSignature;
    private bool _isDisposed;

    public BigInteger P => this._privateKey[0];

    public BigInteger Q => this._privateKey[1];

    public BigInteger G => this._privateKey[2];

    public BigInteger Y => this._privateKey[3];

    public BigInteger X => this._privateKey[4];

    public override int KeyLength => this.P.BitLength;

    protected override DigitalSignature DigitalSignature
    {
      get
      {
        if (this._digitalSignature == null)
          this._digitalSignature = new DsaDigitalSignature(this);
        return (DigitalSignature) this._digitalSignature;
      }
    }

    public override BigInteger[] Public
    {
      get => new BigInteger[4]
      {
        this.P,
        this.Q,
        this.G,
        this.Y
      };
      set => this._privateKey = value.Length == 4 ? value : throw new InvalidOperationException("Invalid public key.");
    }

    public DsaKey() => this._privateKey = new BigInteger[5];

    public DsaKey(byte[] data)
      : base(data)
    {
      if (this._privateKey.Length != 5)
        throw new InvalidOperationException("Invalid private key.");
    }

    public DsaKey(BigInteger p, BigInteger q, BigInteger g, BigInteger y, BigInteger x)
    {
      this._privateKey = new BigInteger[5];
      this._privateKey[0] = p;
      this._privateKey[1] = q;
      this._privateKey[2] = g;
      this._privateKey[3] = y;
      this._privateKey[4] = x;
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
      DsaDigitalSignature digitalSignature = this._digitalSignature;
      if (digitalSignature != null)
      {
        digitalSignature.Dispose();
        this._digitalSignature = (DsaDigitalSignature) null;
      }
      this._isDisposed = true;
    }

    ~DsaKey() => this.Dispose(false);
  }
}
