// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.KeyExchangeDiffieHellman
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System;
using System.Text;

namespace Renci.SshNet.Security
{
  internal abstract class KeyExchangeDiffieHellman : KeyExchange
  {
    protected BigInteger _group;
    protected BigInteger _prime;
    protected byte[] _clientPayload;
    protected byte[] _serverPayload;
    protected BigInteger _clientExchangeValue;
    protected BigInteger _serverExchangeValue;
    protected BigInteger _privateExponent;
    protected byte[] _hostKey;
    protected byte[] _signature;

    protected abstract int HashSize { get; }

    protected override bool ValidateExchangeHash()
    {
      byte[] hash = this.CalculateHash();
      string key = Encoding.UTF8.GetString(this._hostKey, 4, (int) Pack.BigEndianToUInt32(this._hostKey));
      KeyHostAlgorithm host = this.Session.ConnectionInfo.HostKeyAlgorithms[key](this._hostKey);
      this.Session.ConnectionInfo.CurrentHostKeyAlgorithm = key;
      return this.CanTrustHostKey(host) && host.VerifySignature(hash, this._signature);
    }

    public override void Start(Session session, KeyExchangeInitMessage message)
    {
      base.Start(session, message);
      this._serverPayload = message.GetBytes();
      this._clientPayload = this.Session.ClientInitMessage.GetBytes();
    }

    protected void PopulateClientExchangeValue()
    {
      if (this._group.IsZero)
        throw new ArgumentNullException("_group");
      if (this._prime.IsZero)
        throw new ArgumentNullException("_prime");
      int bitLength = Math.Max(this.HashSize * 2, 1024);
      do
      {
        this._privateExponent = BigInteger.Random(bitLength);
        this._clientExchangeValue = BigInteger.ModPow(this._group, this._privateExponent, this._prime);
      }
      while (this._clientExchangeValue < 1L || this._clientExchangeValue > this._prime - (BigInteger) 1);
    }

    protected virtual void HandleServerDhReply(
      byte[] hostKey,
      BigInteger serverExchangeValue,
      byte[] signature)
    {
      this._serverExchangeValue = serverExchangeValue;
      this._hostKey = hostKey;
      this.SharedKey = BigInteger.ModPow(serverExchangeValue, this._privateExponent, this._prime);
      this._signature = signature;
    }
  }
}
