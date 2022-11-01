// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.SftpNameResponse
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
  internal class SftpNameResponse : SftpResponse
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Name;

    public uint Count { get; private set; }

    public Encoding Encoding { get; private set; }

    public KeyValuePair<string, SftpFileAttributes>[] Files { get; set; }

    public SftpNameResponse(uint protocolVersion, Encoding encoding)
      : base(protocolVersion)
    {
      this.Files = Array<KeyValuePair<string, SftpFileAttributes>>.Empty;
      this.Encoding = encoding;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Count = this.ReadUInt32();
      this.Files = new KeyValuePair<string, SftpFileAttributes>[(int) this.Count];
      for (int index = 0; (long) index < (long) this.Count; ++index)
      {
        string key = this.ReadString(this.Encoding);
        if (SftpNameResponse.SupportsLongName(this.ProtocolVersion))
          this.ReadString(this.Encoding);
        this.Files[index] = new KeyValuePair<string, SftpFileAttributes>(key, this.ReadAttributes());
      }
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write((uint) this.Files.Length);
      for (int index = 0; index < this.Files.Length; ++index)
      {
        KeyValuePair<string, SftpFileAttributes> file = this.Files[index];
        this.Write(file.Key, this.Encoding);
        if (SftpNameResponse.SupportsLongName(this.ProtocolVersion))
          this.Write(0U);
        this.Write(file.Value.GetBytes());
      }
    }

    private static bool SupportsLongName(uint protocolVersion) => protocolVersion <= 3U;
  }
}
