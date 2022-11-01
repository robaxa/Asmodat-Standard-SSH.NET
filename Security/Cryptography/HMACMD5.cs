// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.HMACMD5
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
  public class HMACMD5 : System.Security.Cryptography.HMACMD5
  {
    private readonly int _hashSize;

    public HMACMD5(byte[] key)
      : base(key)
    {
      this._hashSize = base.HashSize;
    }

    public HMACMD5(byte[] key, int hashSize)
      : base(key)
    {
      this._hashSize = hashSize;
    }

    public override int HashSize => this._hashSize;

    protected override byte[] HashFinal() => base.HashFinal().Take(this.HashSize / 8);
  }
}
