// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.CertificateHostAlgorithm
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Security
{
  public class CertificateHostAlgorithm : HostAlgorithm
  {
    public override byte[] Data => throw new NotImplementedException();

    public CertificateHostAlgorithm(string name)
      : base(name)
    {
    }

    public override byte[] Sign(byte[] data) => throw new NotImplementedException();

    public override bool VerifySignature(byte[] data, byte[] signature) => throw new NotImplementedException();
  }
}
