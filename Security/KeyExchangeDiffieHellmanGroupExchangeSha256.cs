﻿// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.KeyExchangeDiffieHellmanGroupExchangeSha256
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
  internal class KeyExchangeDiffieHellmanGroupExchangeSha256 : 
    KeyExchangeDiffieHellmanGroupExchangeShaBase
  {
    public override string Name => "diffie-hellman-group-exchange-sha256";

    protected override int HashSize => 256;

    protected override byte[] Hash(byte[] hashBytes)
    {
      using (SHA256 shA256 = CryptoAbstraction.CreateSHA256())
        return shA256.ComputeHash(hashBytes);
    }
  }
}
