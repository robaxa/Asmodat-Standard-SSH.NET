// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.BreakRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  internal class BreakRequestInfo : RequestInfo
  {
    public const string Name = "break";

    public override string RequestName => "break";

    public uint BreakLength { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4;

    public BreakRequestInfo() => this.WantReply = true;

    public BreakRequestInfo(uint breakLength)
      : this()
    {
      this.BreakLength = breakLength;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.BreakLength = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.BreakLength);
    }
  }
}
