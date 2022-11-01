// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.SftpHandleResponse
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Sftp.Responses
{
  internal class SftpHandleResponse : SftpResponse
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Handle;

    public byte[] Handle { get; set; }

    public SftpHandleResponse(uint protocolVersion)
      : base(protocolVersion)
    {
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Handle = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinary(this.Handle, 0, this.Handle.Length);
    }
  }
}
