// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.RsaDigitalSignature
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography
{
  public class RsaDigitalSignature : CipherDigitalSignature, IDisposable
  {
    private HashAlgorithm _hash;
    private bool _isDisposed;

    public RsaDigitalSignature(RsaKey rsaKey)
      : base(new ObjectIdentifier(new ulong[6]
      {
        1UL,
        3UL,
        14UL,
        3UL,
        2UL,
        26UL
      }), (AsymmetricCipher) new RsaCipher(rsaKey))
    {
      this._hash = (HashAlgorithm) CryptoAbstraction.CreateSHA1();
    }

    protected override byte[] Hash(byte[] input) => this._hash.ComputeHash(input);

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

    ~RsaDigitalSignature() => this.Dispose(false);
  }
}
