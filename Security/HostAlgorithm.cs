// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.HostAlgorithm
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Security
{
  public abstract class HostAlgorithm
  {
    public string Name { get; private set; }

    public abstract byte[] Data { get; }

    protected HostAlgorithm(string name) => this.Name = name;

    public abstract byte[] Sign(byte[] data);

    public abstract bool VerifySignature(byte[] data, byte[] signature);
  }
}
