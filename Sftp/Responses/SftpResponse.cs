// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.SftpResponse
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Sftp.Responses
{
  internal abstract class SftpResponse : SftpMessage
  {
    public uint ResponseId { get; set; }

    public uint ProtocolVersion { get; private set; }

    protected SftpResponse(uint protocolVersion) => this.ProtocolVersion = protocolVersion;

    protected override void LoadData()
    {
      base.LoadData();
      this.ResponseId = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.ResponseId);
    }
  }
}
