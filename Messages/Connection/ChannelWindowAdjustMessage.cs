// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelWindowAdjustMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_WINDOW_ADJUST", 93)]
  public class ChannelWindowAdjustMessage : ChannelMessage
  {
    public uint BytesToAdd { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4;

    public ChannelWindowAdjustMessage()
    {
    }

    public ChannelWindowAdjustMessage(uint localChannelNumber, uint bytesToAdd)
      : base(localChannelNumber)
    {
      this.BytesToAdd = bytesToAdd;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.BytesToAdd = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.BytesToAdd);
    }

    internal override void Process(Session session) => session.OnChannelWindowAdjustReceived(this);
  }
}
