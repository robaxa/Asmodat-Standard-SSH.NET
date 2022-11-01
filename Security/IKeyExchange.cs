// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.IKeyExchange
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security.Cryptography;
using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
  public interface IKeyExchange : IDisposable
  {
    event EventHandler<HostKeyEventArgs> HostKeyReceived;

    string Name { get; }

    byte[] ExchangeHash { get; }

    void Start(Session session, KeyExchangeInitMessage message);

    void Finish();

    Cipher CreateClientCipher();

    Cipher CreateServerCipher();

    HashAlgorithm CreateServerHash();

    HashAlgorithm CreateClientHash();

    Compressor CreateCompressor();

    Compressor CreateDecompressor();
  }
}
