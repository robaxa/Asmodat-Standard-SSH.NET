// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpInitRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpInitRequest : SftpMessage
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Init;

    public uint Version { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4;

    public SftpInitRequest(uint version) => this.Version = version;

    protected override void LoadData()
    {
      base.LoadData();
      this.Version = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.Version);
    }
  }
}
