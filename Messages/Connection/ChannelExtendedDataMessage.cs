// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelExtendedDataMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_EXTENDED_DATA", 95)]
  public class ChannelExtendedDataMessage : ChannelMessage
  {
    public uint DataTypeCode { get; private set; }

    public byte[] Data { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + 4 + this.Data.Length;

    public ChannelExtendedDataMessage()
    {
    }

    public ChannelExtendedDataMessage(uint localChannelNumber, uint dataTypeCode, byte[] data)
      : base(localChannelNumber)
    {
      this.DataTypeCode = dataTypeCode;
      this.Data = data;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.DataTypeCode = this.ReadUInt32();
      this.Data = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.DataTypeCode);
      this.WriteBinaryString(this.Data);
    }

    internal override void Process(Session session) => session.OnChannelExtendedDataReceived(this);
  }
}
