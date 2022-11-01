// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.HMACSHA1
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
  public class HMACSHA1 : System.Security.Cryptography.HMACSHA1
  {
    private readonly int _hashSize;

    public HMACSHA1(byte[] key)
      : base(key)
    {
      this._hashSize = base.HashSize;
    }

    public HMACSHA1(byte[] key, int hashSize)
      : base(key)
    {
      this._hashSize = hashSize;
    }

    public override int HashSize => this._hashSize;

    protected override byte[] HashFinal() => base.HashFinal().Take(this.HashSize / 8);
  }
}
