// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelDataMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_DATA", 94)]
  public class ChannelDataMessage : ChannelMessage
  {
    internal const byte MessageNumber = 94;

    public byte[] Data { get; private set; }

    public int Offset { get; set; }

    public int Size { get; set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Size;

    internal override void Process(Session session) => session.OnChannelDataReceived(this);

    public ChannelDataMessage()
    {
    }

    public ChannelDataMessage(uint localChannelNumber, byte[] data)
      : base(localChannelNumber)
    {
      this.Data = data != null ? data : throw new ArgumentNullException(nameof (data));
      this.Offset = 0;
      this.Size = data.Length;
    }

    public ChannelDataMessage(uint localChannelNumber, byte[] data, int offset, int size)
      : base(localChannelNumber)
    {
      this.Data = data != null ? data : throw new ArgumentNullException(nameof (data));
      this.Offset = offset;
      this.Size = size;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Data = this.ReadBinary();
      this.Offset = 0;
      this.Size = this.Data.Length;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinary(this.Data, this.Offset, this.Size);
    }
  }
}
