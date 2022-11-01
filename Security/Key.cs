// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Key
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using System;
using System.Collections.Generic;

namespace Renci.SshNet.Security
{
  public abstract class Key
  {
    protected BigInteger[] _privateKey;

    protected abstract DigitalSignature DigitalSignature { get; }

    public abstract BigInteger[] Public { get; set; }

    public abstract int KeyLength { get; }

    protected Key(byte[] data)
    {
      DerData derData = data != null ? new DerData(data) : throw new ArgumentNullException(nameof (data));
      derData.ReadBigInteger();
      List<BigInteger> bigIntegerList = new List<BigInteger>();
      while (!derData.IsEndOfData)
        bigIntegerList.Add(derData.ReadBigInteger());
      this._privateKey = bigIntegerList.ToArray();
    }

    protected Key()
    {
    }

    public byte[] Sign(byte[] data) => this.DigitalSignature.Sign(data);

    public bool VerifySignature(byte[] data, byte[] signature) => this.DigitalSignature.Verify(data, signature);
  }
}
