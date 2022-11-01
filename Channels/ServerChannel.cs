// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.ServerChannel
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
  internal abstract class ServerChannel : Channel
  {
    protected ServerChannel(
      ISession session,
      uint localChannelNumber,
      uint localWindowSize,
      uint localPacketSize,
      uint remoteChannelNumber,
      uint remoteWindowSize,
      uint remotePacketSize)
      : base(session, localChannelNumber, localWindowSize, localPacketSize)
    {
      this.InitializeRemoteInfo(remoteChannelNumber, remoteWindowSize, remotePacketSize);
    }

    protected void SendMessage(ChannelOpenConfirmationMessage message)
    {
      this.Session.SendMessage((Message) message);
      this.IsOpen = true;
    }
  }
}
