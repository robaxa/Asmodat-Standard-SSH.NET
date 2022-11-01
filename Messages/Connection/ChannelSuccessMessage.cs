// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelSuccessMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_SUCCESS", 99)]
  public class ChannelSuccessMessage : ChannelMessage
  {
    public ChannelSuccessMessage()
    {
    }

    public ChannelSuccessMessage(uint localChannelNumber)
      : base(localChannelNumber)
    {
    }

    internal override void Process(Session session) => session.OnChannelSuccessReceived(this);
  }
}
