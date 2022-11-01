// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.SignalRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class SignalRequestInfo : RequestInfo
  {
    private byte[] _signalName;
    public const string Name = "signal";

    public override string RequestName => "signal";

    public string SignalName
    {
      get => SshData.Ascii.GetString(this._signalName, 0, this._signalName.Length);
      private set => this._signalName = SshData.Ascii.GetBytes(value);
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._signalName.Length;

    public SignalRequestInfo() => this.WantReply = false;

    public SignalRequestInfo(string signalName)
      : this()
    {
      this.SignalName = signalName;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._signalName = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._signalName);
    }
  }
}
