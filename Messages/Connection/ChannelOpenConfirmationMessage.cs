// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelOpenConfirmationMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_OPEN_CONFIRMATION", 91)]
  public class ChannelOpenConfirmationMessage : ChannelMessage
  {
    public uint RemoteChannelNumber { get; private set; }

    public uint InitialWindowSize { get; private set; }

    public uint MaximumPacketSize { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + 4 + 4;

    public ChannelOpenConfirmationMessage()
    {
    }

    public ChannelOpenConfirmationMessage(
      uint localChannelNumber,
      uint initialWindowSize,
      uint maximumPacketSize,
      uint remoteChannelNumber)
      : base(localChannelNumber)
    {
      this.InitialWindowSize = initialWindowSize;
      this.MaximumPacketSize = maximumPacketSize;
      this.RemoteChannelNumber = remoteChannelNumber;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.RemoteChannelNumber = this.ReadUInt32();
      this.InitialWindowSize = this.ReadUInt32();
      this.MaximumPacketSize = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.RemoteChannelNumber);
      this.Write(this.InitialWindowSize);
      this.Write(this.MaximumPacketSize);
    }

    internal override void Process(Session session) => session.OnChannelOpenConfirmationReceived(this);
  }
}
