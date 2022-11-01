// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.KeyExchange
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
  public abstract class KeyExchange : Algorithm, IKeyExchange, IDisposable
  {
    private CipherInfo _clientCipherInfo;
    private CipherInfo _serverCipherInfo;
    private HashInfo _clientHashInfo;
    private HashInfo _serverHashInfo;
    private Type _compressionType;
    private Type _decompressionType;
    private byte[] _exchangeHash;

    protected Session Session { get; private set; }

    public BigInteger SharedKey { get; protected set; }

    public byte[] ExchangeHash
    {
      get
      {
        if (this._exchangeHash == null)
          this._exchangeHash = this.CalculateHash();
        return this._exchangeHash;
      }
    }

    public event EventHandler<HostKeyEventArgs> HostKeyReceived;

    public virtual void Start(Session session, KeyExchangeInitMessage message)
    {
      this.Session = session;
      this.SendMessage(session.ClientInitMessage);
      string key1 = session.ConnectionInfo.Encryptions.Keys.SelectMany((Func<string, IEnumerable<string>>) (b => (IEnumerable<string>) message.EncryptionAlgorithmsClientToServer), (b, a) => new
      {
        b = b,
        a = a
      }).Where(_param1 => _param1.a == _param1.b).Select(_param1 => _param1.a).FirstOrDefault<string>();
      session.ConnectionInfo.CurrentClientEncryption = !string.IsNullOrEmpty(key1) ? key1 : throw new SshConnectionException("Client encryption algorithm not found", DisconnectReason.KeyExchangeFailed);
      string key2 = session.ConnectionInfo.Encryptions.Keys.SelectMany((Func<string, IEnumerable<string>>) (b => (IEnumerable<string>) message.EncryptionAlgorithmsServerToClient), (b, a) => new
      {
        b = b,
        a = a
      }).Where(_param1 => _param1.a == _param1.b).Select(_param1 => _param1.a).FirstOrDefault<string>();
      session.ConnectionInfo.CurrentServerEncryption = !string.IsNullOrEmpty(key2) ? key2 : throw new SshConnectionException("Server decryption algorithm not found", DisconnectReason.KeyExchangeFailed);
      string key3 = session.ConnectionInfo.HmacAlgorithms.Keys.SelectMany((Func<string, IEnumerable<string>>) (b => (IEnumerable<string>) message.MacAlgorithmsClientToServer), (b, a) => new
      {
        b = b,
        a = a
      }).Where(_param1 => _param1.a == _param1.b).Select(_param1 => _param1.a).FirstOrDefault<string>();
      session.ConnectionInfo.CurrentClientHmacAlgorithm = !string.IsNullOrEmpty(key3) ? key3 : throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReason.KeyExchangeFailed);
      string key4 = session.ConnectionInfo.HmacAlgorithms.Keys.SelectMany((Func<string, IEnumerable<string>>) (b => (IEnumerable<string>) message.MacAlgorithmsServerToClient), (b, a) => new
      {
        b = b,
        a = a
      }).Where(_param1 => _param1.a == _param1.b).Select(_param1 => _param1.a).FirstOrDefault<string>();
      session.ConnectionInfo.CurrentServerHmacAlgorithm = !string.IsNullOrEmpty(key4) ? key4 : throw new SshConnectionException("Server HMAC algorithm not found", DisconnectReason.KeyExchangeFailed);
      string key5 = session.ConnectionInfo.CompressionAlgorithms.Keys.SelectMany((Func<string, IEnumerable<string>>) (b => (IEnumerable<string>) message.CompressionAlgorithmsClientToServer), (b, a) => new
      {
        b = b,
        a = a
      }).Where(_param1 => _param1.a == _param1.b).Select(_param1 => _param1.a).LastOrDefault<string>();
      session.ConnectionInfo.CurrentClientCompressionAlgorithm = !string.IsNullOrEmpty(key5) ? key5 : throw new SshConnectionException("Compression algorithm not found", DisconnectReason.KeyExchangeFailed);
      string key6 = session.ConnectionInfo.CompressionAlgorithms.Keys.SelectMany((Func<string, IEnumerable<string>>) (b => (IEnumerable<string>) message.CompressionAlgorithmsServerToClient), (b, a) => new
      {
        b = b,
        a = a
      }).Where(_param1 => _param1.a == _param1.b).Select(_param1 => _param1.a).LastOrDefault<string>();
      session.ConnectionInfo.CurrentServerCompressionAlgorithm = !string.IsNullOrEmpty(key6) ? key6 : throw new SshConnectionException("Decompression algorithm not found", DisconnectReason.KeyExchangeFailed);
      this._clientCipherInfo = session.ConnectionInfo.Encryptions[key1];
      this._serverCipherInfo = session.ConnectionInfo.Encryptions[key2];
      this._clientHashInfo = session.ConnectionInfo.HmacAlgorithms[key3];
      this._serverHashInfo = session.ConnectionInfo.HmacAlgorithms[key4];
      this._compressionType = session.ConnectionInfo.CompressionAlgorithms[key5];
      this._decompressionType = session.ConnectionInfo.CompressionAlgorithms[key6];
    }

    public virtual void Finish()
    {
      if (!this.ValidateExchangeHash())
        throw new SshConnectionException("Key exchange negotiation failed.", DisconnectReason.KeyExchangeFailed);
      this.SendMessage((Message) new NewKeysMessage());
    }

    public Cipher CreateServerCipher()
    {
      byte[] sessionId = this.Session.SessionId ?? this.ExchangeHash;
      byte[] bytes = this.Hash(KeyExchange.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'B', sessionId));
      byte[] sessionKey = this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, this.Hash(KeyExchange.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'D', sessionId)), this._serverCipherInfo.KeySize / 8);
      DiagnosticAbstraction.Log(string.Format("[{0}] Creating server cipher (Name:{1},Key:{2},IV:{3})", (object) Session.ToHex(this.Session.SessionId), (object) this.Session.ConnectionInfo.CurrentServerEncryption, (object) Session.ToHex(sessionKey), (object) Session.ToHex(bytes)));
      return this._serverCipherInfo.Cipher(sessionKey, bytes);
    }

    public Cipher CreateClientCipher()
    {
      byte[] sessionId = this.Session.SessionId ?? this.ExchangeHash;
      byte[] numArray = this.Hash(KeyExchange.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'A', sessionId));
      return this._clientCipherInfo.Cipher(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, this.Hash(KeyExchange.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'C', sessionId)), this._clientCipherInfo.KeySize / 8), numArray);
    }

    public HashAlgorithm CreateServerHash() => this._serverHashInfo.HashAlgorithm(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, this.Hash(KeyExchange.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'F', this.Session.SessionId ?? this.ExchangeHash)), this._serverHashInfo.KeySize / 8));

    public HashAlgorithm CreateClientHash() => this._clientHashInfo.HashAlgorithm(this.GenerateSessionKey(this.SharedKey, this.ExchangeHash, this.Hash(KeyExchange.GenerateSessionKey(this.SharedKey, this.ExchangeHash, 'E', this.Session.SessionId ?? this.ExchangeHash)), this._clientHashInfo.KeySize / 8));

    public Compressor CreateCompressor()
    {
      if (this._compressionType == (Type) null)
        return (Compressor) null;
      Compressor instance = this._compressionType.CreateInstance<Compressor>();
      instance.Init(this.Session);
      return instance;
    }

    public Compressor CreateDecompressor()
    {
      if (this._compressionType == (Type) null)
        return (Compressor) null;
      Compressor instance = this._decompressionType.CreateInstance<Compressor>();
      instance.Init(this.Session);
      return instance;
    }

    protected bool CanTrustHostKey(KeyHostAlgorithm host)
    {
      EventHandler<HostKeyEventArgs> hostKeyReceived = this.HostKeyReceived;
      if (hostKeyReceived == null)
        return true;
      HostKeyEventArgs e = new HostKeyEventArgs(host);
      hostKeyReceived((object) this, e);
      return e.CanTrust;
    }

    protected abstract bool ValidateExchangeHash();

    protected abstract byte[] CalculateHash();

    protected virtual byte[] Hash(byte[] hashData)
    {
      using (SHA1 shA1 = CryptoAbstraction.CreateSHA1())
        return shA1.ComputeHash(hashData, 0, hashData.Length);
    }

    protected void SendMessage(Message message) => this.Session.SendMessage(message);

    private byte[] GenerateSessionKey(
      BigInteger sharedKey,
      byte[] exchangeHash,
      byte[] key,
      int size)
    {
      List<byte> byteList = new List<byte>((IEnumerable<byte>) key);
      while (size > byteList.Count)
        byteList.AddRange((IEnumerable<byte>) this.Hash(new KeyExchange._SessionKeyAdjustment()
        {
          SharedKey = sharedKey,
          ExchangeHash = exchangeHash,
          Key = key
        }.GetBytes()));
      return byteList.ToArray();
    }

    private static byte[] GenerateSessionKey(
      BigInteger sharedKey,
      byte[] exchangeHash,
      char p,
      byte[] sessionId)
    {
      return new KeyExchange._SessionKeyGeneration()
      {
        SharedKey = sharedKey,
        ExchangeHash = exchangeHash,
        Char = p,
        SessionId = sessionId
      }.GetBytes();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    ~KeyExchange() => this.Dispose(false);

    private class _SessionKeyGeneration : SshData
    {
      private byte[] _sharedKey;

      public BigInteger SharedKey
      {
        private get => this._sharedKey.ToBigInteger();
        set => this._sharedKey = value.ToByteArray().Reverse<byte>();
      }

      public byte[] ExchangeHash { get; set; }

      public char Char { get; set; }

      public byte[] SessionId { get; set; }

      protected override int BufferCapacity => base.BufferCapacity + 4 + this._sharedKey.Length + this.ExchangeHash.Length + 1 + this.SessionId.Length;

      protected override void LoadData() => throw new NotImplementedException();

      protected override void SaveData()
      {
        this.WriteBinaryString(this._sharedKey);
        this.Write(this.ExchangeHash);
        this.Write((byte) this.Char);
        this.Write(this.SessionId);
      }
    }

    private class _SessionKeyAdjustment : SshData
    {
      private byte[] _sharedKey;

      public BigInteger SharedKey
      {
        private get => this._sharedKey.ToBigInteger();
        set => this._sharedKey = value.ToByteArray().Reverse<byte>();
      }

      public byte[] ExchangeHash { get; set; }

      public byte[] Key { get; set; }

      protected override int BufferCapacity => base.BufferCapacity + 4 + this._sharedKey.Length + this.ExchangeHash.Length + this.Key.Length;

      protected override void LoadData() => throw new NotImplementedException();

      protected override void SaveData()
      {
        this.WriteBinaryString(this._sharedKey);
        this.Write(this.ExchangeHash);
        this.Write(this.Key);
      }
    }
  }
}
