// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.SftpVersionResponse
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System.Collections.Generic;

namespace Renci.SshNet.Sftp.Responses
{
  internal class SftpVersionResponse : SftpMessage
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Version;

    public uint Version { get; set; }

    public IDictionary<string, string> Extentions { get; set; }

    protected override void LoadData()
    {
      base.LoadData();
      this.Version = this.ReadUInt32();
      this.Extentions = this.ReadExtensionPair();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.Version);
      if (this.Extentions == null)
        return;
      this.Write(this.Extentions);
    }
  }
}
