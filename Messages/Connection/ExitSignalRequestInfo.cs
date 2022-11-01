// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ExitSignalRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class ExitSignalRequestInfo : RequestInfo
  {
    private byte[] _signalName;
    private byte[] _errorMessage;
    private byte[] _language;
    public const string Name = "exit-signal";

    public override string RequestName => "exit-signal";

    public string SignalName
    {
      get => SshData.Ascii.GetString(this._signalName, 0, this._signalName.Length);
      private set => this._signalName = SshData.Ascii.GetBytes(value);
    }

    public bool CoreDumped { get; private set; }

    public string ErrorMessage
    {
      get => SshData.Utf8.GetString(this._errorMessage, 0, this._errorMessage.Length);
      private set => this._errorMessage = SshData.Utf8.GetBytes(value);
    }

    public string Language
    {
      get => SshData.Utf8.GetString(this._language, 0, this._language.Length);
      private set => this._language = SshData.Utf8.GetBytes(value);
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._signalName.Length + 1 + 4 + this._errorMessage.Length + 4 + this._language.Length;

    public ExitSignalRequestInfo() => this.WantReply = false;

    public ExitSignalRequestInfo(
      string signalName,
      bool coreDumped,
      string errorMessage,
      string language)
      : this()
    {
      this.SignalName = signalName;
      this.CoreDumped = coreDumped;
      this.ErrorMessage = errorMessage;
      this.Language = language;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._signalName = this.ReadBinary();
      this.CoreDumped = this.ReadBoolean();
      this._errorMessage = this.ReadBinary();
      this._language = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._signalName);
      this.Write(this.CoreDumped);
      this.Write(this._errorMessage);
      this.Write(this._language);
    }
  }
}
