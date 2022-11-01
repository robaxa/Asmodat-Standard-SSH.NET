// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.CipherDigitalSignature
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Security.Cryptography
{
  public abstract class CipherDigitalSignature : DigitalSignature
  {
    private readonly AsymmetricCipher _cipher;
    private readonly ObjectIdentifier _oid;

    protected CipherDigitalSignature(ObjectIdentifier oid, AsymmetricCipher cipher)
    {
      this._cipher = cipher != null ? cipher : throw new ArgumentNullException(nameof (cipher));
      this._oid = oid;
    }

    public override bool Verify(byte[] input, byte[] signature)
    {
      byte[] right = this._cipher.Decrypt(signature);
      return this.DerEncode(this.Hash(input)).IsEqualTo(right);
    }

    public override byte[] Sign(byte[] input) => this._cipher.Encrypt(this.DerEncode(this.Hash(input))).TrimLeadingZeros();

    protected abstract byte[] Hash(byte[] input);

    protected byte[] DerEncode(byte[] hashData)
    {
      DerData data = new DerData();
      data.Write(this._oid);
      data.WriteNull();
      DerData derData = new DerData();
      derData.Write(data);
      derData.Write(hashData);
      return derData.Encode();
    }
  }
}
