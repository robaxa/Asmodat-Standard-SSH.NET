// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.SubsystemRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class SubsystemRequestInfo : RequestInfo
  {
    private byte[] _subsystemName;
    public const string Name = "subsystem";

    public override string RequestName => "subsystem";

    public string SubsystemName
    {
      get => SshData.Ascii.GetString(this._subsystemName, 0, this._subsystemName.Length);
      private set => this._subsystemName = SshData.Ascii.GetBytes(value);
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._subsystemName.Length;

    public SubsystemRequestInfo() => this.WantReply = true;

    public SubsystemRequestInfo(string subsystem)
      : this()
    {
      this.SubsystemName = subsystem;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._subsystemName = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._subsystemName);
    }
  }
}
