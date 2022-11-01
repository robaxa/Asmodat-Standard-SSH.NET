// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.XonXoffRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  internal class XonXoffRequestInfo : RequestInfo
  {
    public const string Name = "xon-xoff";

    public override string RequestName => "xon-xoff";

    public bool ClientCanDo { get; set; }

    protected override int BufferCapacity => base.BufferCapacity + 1;

    public XonXoffRequestInfo() => this.WantReply = false;

    public XonXoffRequestInfo(bool clientCanDo)
      : this()
    {
      this.ClientCanDo = clientCanDo;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.ClientCanDo = this.ReadBoolean();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.ClientCanDo);
    }
  }
}
