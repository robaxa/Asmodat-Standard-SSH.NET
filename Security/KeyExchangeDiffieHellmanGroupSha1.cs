// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.KeyExchangeDiffieHellmanGroupSha1
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using System;

namespace Renci.SshNet.Security
{
  internal abstract class KeyExchangeDiffieHellmanGroupSha1 : KeyExchangeDiffieHellman
  {
    public abstract BigInteger GroupPrime { get; }

    protected override int HashSize => 160;

    protected override byte[] CalculateHash() => this.Hash(new KeyExchangeDiffieHellmanGroupSha1._ExchangeHashData()
    {
      ClientVersion = this.Session.ClientVersion,
      ServerVersion = this.Session.ServerVersion,
      ClientPayload = this._clientPayload,
      ServerPayload = this._serverPayload,
      HostKey = this._hostKey,
      ClientExchangeValue = this._clientExchangeValue,
      ServerExchangeValue = this._serverExchangeValue,
      SharedKey = this.SharedKey
    }.GetBytes());

    public override void Start(Session session, KeyExchangeInitMessage message)
    {
      base.Start(session, message);
      this.Session.RegisterMessage("SSH_MSG_KEXDH_REPLY");
      this.Session.KeyExchangeDhReplyMessageReceived += new EventHandler<MessageEventArgs<KeyExchangeDhReplyMessage>>(this.Session_KeyExchangeDhReplyMessageReceived);
      this._prime = this.GroupPrime;
      this._group = new BigInteger(new byte[1]{ (byte) 2 });
      this.PopulateClientExchangeValue();
      this.SendMessage((Message) new KeyExchangeDhInitMessage(this._clientExchangeValue));
    }

    public override void Finish()
    {
      base.Finish();
      this.Session.KeyExchangeDhReplyMessageReceived -= new EventHandler<MessageEventArgs<KeyExchangeDhReplyMessage>>(this.Session_KeyExchangeDhReplyMessageReceived);
    }

    private void Session_KeyExchangeDhReplyMessageReceived(
      object sender,
      MessageEventArgs<KeyExchangeDhReplyMessage> e)
    {
      KeyExchangeDhReplyMessage message = e.Message;
      this.Session.UnRegisterMessage("SSH_MSG_KEXDH_REPLY");
      this.HandleServerDhReply(message.HostKey, message.F, message.Signature);
      this.Finish();
    }

    private class _ExchangeHashData : SshData
    {
      private byte[] _serverVersion;
      private byte[] _clientVersion;
      private byte[] _clientExchangeValue;
      private byte[] _serverExchangeValue;
      private byte[] _sharedKey;

      public string ServerVersion
      {
        private get => SshData.Utf8.GetString(this._serverVersion, 0, this._serverVersion.Length);
        set => this._serverVersion = SshData.Utf8.GetBytes(value);
      }

      public string ClientVersion
      {
        private get => SshData.Utf8.GetString(this._clientVersion, 0, this._clientVersion.Length);
        set => this._clientVersion = SshData.Utf8.GetBytes(value);
      }

      public byte[] ClientPayload { get; set; }

      public byte[] ServerPayload { get; set; }

      public byte[] HostKey { get; set; }

      public BigInteger ClientExchangeValue
      {
        private get => this._clientExchangeValue.ToBigInteger();
        set => this._clientExchangeValue = value.ToByteArray().Reverse<byte>();
      }

      public BigInteger ServerExchangeValue
      {
        private get => this._serverExchangeValue.ToBigInteger();
        set => this._serverExchangeValue = value.ToByteArray().Reverse<byte>();
      }

      public BigInteger SharedKey
      {
        private get => this._sharedKey.ToBigInteger();
        set => this._sharedKey = value.ToByteArray().Reverse<byte>();
      }

      protected override int BufferCapacity => base.BufferCapacity + 4 + this._clientVersion.Length + 4 + this._serverVersion.Length + 4 + this.ClientPayload.Length + 4 + this.ServerPayload.Length + 4 + this.HostKey.Length + 4 + this._clientExchangeValue.Length + 4 + this._serverExchangeValue.Length + 4 + this._sharedKey.Length;

      protected override void LoadData() => throw new NotImplementedException();

      protected override void SaveData()
      {
        this.WriteBinaryString(this._clientVersion);
        this.WriteBinaryString(this._serverVersion);
        this.WriteBinaryString(this.ClientPayload);
        this.WriteBinaryString(this.ServerPayload);
        this.WriteBinaryString(this.HostKey);
        this.WriteBinaryString(this._clientExchangeValue);
        this.WriteBinaryString(this._serverExchangeValue);
        this.WriteBinaryString(this._sharedKey);
      }
    }
  }
}
