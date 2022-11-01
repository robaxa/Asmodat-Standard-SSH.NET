// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelOpenFailureMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_OPEN_FAILURE", 92)]
  public class ChannelOpenFailureMessage : ChannelMessage
  {
    internal const uint AdministrativelyProhibited = 1;
    internal const uint ConnectFailed = 2;
    internal const uint UnknownChannelType = 3;
    internal const uint ResourceShortage = 4;
    private byte[] _description;
    private byte[] _language;

    public uint ReasonCode { get; private set; }

    public string Description
    {
      get => SshData.Utf8.GetString(this._description, 0, this._description.Length);
      private set => this._description = SshData.Utf8.GetBytes(value);
    }

    public string Language
    {
      get => SshData.Utf8.GetString(this._language, 0, this._language.Length);
      private set => this._language = SshData.Utf8.GetBytes(value);
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + 4 + this._description.Length + 4 + this._language.Length;

    public ChannelOpenFailureMessage()
    {
    }

    public ChannelOpenFailureMessage(uint localChannelNumber, string description, uint reasonCode)
      : this(localChannelNumber, description, reasonCode, "en")
    {
    }

    public ChannelOpenFailureMessage(
      uint localChannelNumber,
      string description,
      uint reasonCode,
      string language)
      : base(localChannelNumber)
    {
      this.Description = description;
      this.ReasonCode = reasonCode;
      this.Language = language;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.ReasonCode = this.ReadUInt32();
      this._description = this.ReadBinary();
      this._language = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.ReasonCode);
      this.WriteBinaryString(this._description);
      this.WriteBinaryString(this._language);
    }

    internal override void Process(Session session) => session.OnChannelOpenFailureReceived(this);
  }
}
