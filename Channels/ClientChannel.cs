// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.ClientChannel
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System;

namespace Renci.SshNet.Channels
{
  internal abstract class ClientChannel : Channel
  {
    protected ClientChannel(
      ISession session,
      uint localChannelNumber,
      uint localWindowSize,
      uint localPacketSize)
      : base(session, localChannelNumber, localWindowSize, localPacketSize)
    {
      session.ChannelOpenConfirmationReceived += new EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>>(this.OnChannelOpenConfirmation);
      session.ChannelOpenFailureReceived += new EventHandler<MessageEventArgs<ChannelOpenFailureMessage>>(this.OnChannelOpenFailure);
    }

    public event EventHandler<ChannelOpenConfirmedEventArgs> OpenConfirmed;

    public event EventHandler<ChannelOpenFailedEventArgs> OpenFailed;

    protected virtual void OnOpenConfirmation(
      uint remoteChannelNumber,
      uint initialWindowSize,
      uint maximumPacketSize)
    {
      this.InitializeRemoteInfo(remoteChannelNumber, initialWindowSize, maximumPacketSize);
      this.IsOpen = true;
      EventHandler<ChannelOpenConfirmedEventArgs> openConfirmed = this.OpenConfirmed;
      if (openConfirmed == null)
        return;
      openConfirmed((object) this, new ChannelOpenConfirmedEventArgs(remoteChannelNumber, initialWindowSize, maximumPacketSize));
    }

    protected void SendMessage(ChannelOpenMessage message) => this.Session.SendMessage((Message) message);

    protected virtual void OnOpenFailure(uint reasonCode, string description, string language)
    {
      EventHandler<ChannelOpenFailedEventArgs> openFailed = this.OpenFailed;
      if (openFailed == null)
        return;
      openFailed((object) this, new ChannelOpenFailedEventArgs(this.LocalChannelNumber, reasonCode, description, language));
    }

    private void OnChannelOpenConfirmation(
      object sender,
      MessageEventArgs<ChannelOpenConfirmationMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnOpenConfirmation(e.Message.RemoteChannelNumber, e.Message.InitialWindowSize, e.Message.MaximumPacketSize);
      }
      catch (Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelOpenFailure(object sender, MessageEventArgs<ChannelOpenFailureMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnOpenFailure(e.Message.ReasonCode, e.Message.Description, e.Message.Language);
      }
      catch (Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    protected override void Dispose(bool disposing)
    {
      this.UnsubscribeFromSessionEvents(this.Session);
      base.Dispose(disposing);
    }

    private void UnsubscribeFromSessionEvents(ISession session)
    {
      if (session == null)
        return;
      session.ChannelOpenConfirmationReceived -= new EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>>(this.OnChannelOpenConfirmation);
      session.ChannelOpenFailureReceived -= new EventHandler<MessageEventArgs<ChannelOpenFailureMessage>>(this.OnChannelOpenFailure);
    }
  }
}
