// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ExitStatusRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  internal class ExitStatusRequestInfo : RequestInfo
  {
    public const string Name = "exit-status";

    public override string RequestName => "exit-status";

    public uint ExitStatus { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4;

    public ExitStatusRequestInfo() => this.WantReply = false;

    public ExitStatusRequestInfo(uint exitStatus)
      : this()
    {
      this.ExitStatus = exitStatus;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.ExitStatus = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.ExitStatus);
    }
  }
}
