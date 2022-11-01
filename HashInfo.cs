// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.HashInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet
{
  public class HashInfo
  {
    public int KeySize { get; private set; }

    public Func<byte[], System.Security.Cryptography.HashAlgorithm> HashAlgorithm { get; private set; }

    public HashInfo(int keySize, Func<byte[], System.Security.Cryptography.HashAlgorithm> hash)
    {
      HashInfo hashInfo = this;
      this.KeySize = keySize;
      this.HashAlgorithm = (Func<byte[], System.Security.Cryptography.HashAlgorithm>) (key => hash(key.Take(hashInfo.KeySize / 8)));
    }
  }
}
