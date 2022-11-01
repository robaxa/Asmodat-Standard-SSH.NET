// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.KeyExchangeDiffieHellmanGroupExchangeShaBase
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using System;

namespace Renci.SshNet.Security
{
  internal abstract class KeyExchangeDiffieHellmanGroupExchangeShaBase : KeyExchangeDiffieHellman
  {
    private const int MinimumGroupSize = 1024;
    private const int PreferredGroupSize = 1024;
    private const int MaximumProupSize = 8192;

    protected override byte[] CalculateHash() => this.Hash(new GroupExchangeHashData()
    {
      ClientVersion = this.Session.ClientVersion,
      ServerVersion = this.Session.ServerVersion,
      ClientPayload = this._clientPayload,
      ServerPayload = this._serverPayload,
      HostKey = this._hostKey,
      MinimumGroupSize = 1024U,
      PreferredGroupSize = 1024U,
      MaximumGroupSize = 8192U,
      Prime = this._prime,
      SubGroup = this._group,
      ClientExchangeValue = this._clientExchangeValue,
      ServerExchangeValue = this._serverExchangeValue,
      SharedKey = this.SharedKey
    }.GetBytes());

    public override void Start(Session session, KeyExchangeInitMessage message)
    {
      base.Start(session, message);
      this.Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");
      this.Session.KeyExchangeDhGroupExchangeGroupReceived += new EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeGroup>>(this.Session_KeyExchangeDhGroupExchangeGroupReceived);
      this.SendMessage((Message) new KeyExchangeDhGroupExchangeRequest(1024U, 1024U, 8192U));
    }

    public override void Finish()
    {
      base.Finish();
      this.Session.KeyExchangeDhGroupExchangeGroupReceived -= new EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeGroup>>(this.Session_KeyExchangeDhGroupExchangeGroupReceived);
      this.Session.KeyExchangeDhGroupExchangeReplyReceived -= new EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeReply>>(this.Session_KeyExchangeDhGroupExchangeReplyReceived);
    }

    private void Session_KeyExchangeDhGroupExchangeGroupReceived(
      object sender,
      MessageEventArgs<KeyExchangeDhGroupExchangeGroup> e)
    {
      KeyExchangeDhGroupExchangeGroup message = e.Message;
      this.Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_GROUP");
      this.Session.KeyExchangeDhGroupExchangeGroupReceived -= new EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeGroup>>(this.Session_KeyExchangeDhGroupExchangeGroupReceived);
      this.Session.RegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");
      this.Session.KeyExchangeDhGroupExchangeReplyReceived += new EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeReply>>(this.Session_KeyExchangeDhGroupExchangeReplyReceived);
      this._prime = message.SafePrime;
      this._group = message.SubGroup;
      this.PopulateClientExchangeValue();
      this.SendMessage((Message) new KeyExchangeDhGroupExchangeInit(this._clientExchangeValue));
    }

    private void Session_KeyExchangeDhGroupExchangeReplyReceived(
      object sender,
      MessageEventArgs<KeyExchangeDhGroupExchangeReply> e)
    {
      KeyExchangeDhGroupExchangeReply message = e.Message;
      this.Session.UnRegisterMessage("SSH_MSG_KEX_DH_GEX_REPLY");
      this.Session.KeyExchangeDhGroupExchangeReplyReceived -= new EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeReply>>(this.Session_KeyExchangeDhGroupExchangeReplyReceived);
      this.HandleServerDhReply(message.HostKey, message.F, message.Signature);
      this.Finish();
    }
  }
}
