// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelRequestMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_REQUEST", 98)]
  public class ChannelRequestMessage : ChannelMessage
  {
    private string _requestName;
    private byte[] _requestNameBytes;

    public string RequestName
    {
      get => this._requestName;
      private set
      {
        this._requestName = value;
        this._requestNameBytes = SshData.Ascii.GetBytes(value);
      }
    }

    public byte[] RequestData { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._requestNameBytes.Length + this.RequestData.Length;

    public ChannelRequestMessage()
    {
    }

    public ChannelRequestMessage(uint localChannelNumber, RequestInfo info)
      : base(localChannelNumber)
    {
      this.RequestName = info.RequestName;
      this.RequestData = info.GetBytes();
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._requestNameBytes = this.ReadBinary();
      this._requestName = SshData.Ascii.GetString(this._requestNameBytes, 0, this._requestNameBytes.Length);
      this.RequestData = this.ReadBytes();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._requestNameBytes);
      this.Write(this.RequestData);
    }

    internal override void Process(Session session) => session.OnChannelRequestReceived(this);
  }
}
