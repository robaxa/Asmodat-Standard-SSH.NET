// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.DisconnectMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_DISCONNECT", 1)]
  public class DisconnectMessage : Message, IKeyExchangedAllowed
  {
    private byte[] _description;
    private byte[] _language;

    public DisconnectReason ReasonCode { get; private set; }

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

    public DisconnectMessage()
    {
    }

    public DisconnectMessage(DisconnectReason reasonCode, string message)
    {
      this.ReasonCode = reasonCode;
      this.Description = message;
      this.Language = "en";
    }

    protected override void LoadData()
    {
      this.ReasonCode = (DisconnectReason) this.ReadUInt32();
      this._description = this.ReadBinary();
      this._language = this.ReadBinary();
    }

    protected override void SaveData()
    {
      this.Write((uint) this.ReasonCode);
      this.WriteBinaryString(this._description);
      this.WriteBinaryString(this._language);
    }

    internal override void Process(Session session) => session.OnDisconnectReceived(this);
  }
}
