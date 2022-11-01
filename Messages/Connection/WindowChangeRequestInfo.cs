// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.WindowChangeRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  internal class WindowChangeRequestInfo : RequestInfo
  {
    public const string Name = "window-change";

    public override string RequestName => "window-change";

    public uint Columns { get; private set; }

    public uint Rows { get; private set; }

    public uint Width { get; private set; }

    public uint Height { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + 4 + 4 + 4;

    public WindowChangeRequestInfo() => this.WantReply = false;

    public WindowChangeRequestInfo(uint columns, uint rows, uint width, uint height)
      : this()
    {
      this.Columns = columns;
      this.Rows = rows;
      this.Width = width;
      this.Height = height;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Columns = this.ReadUInt32();
      this.Rows = this.ReadUInt32();
      this.Width = this.ReadUInt32();
      this.Height = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.Columns);
      this.Write(this.Rows);
      this.Write(this.Width);
      this.Write(this.Height);
    }
  }
}
