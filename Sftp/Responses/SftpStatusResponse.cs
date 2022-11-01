// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.SftpStatusResponse
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp.Responses
{
  internal class SftpStatusResponse : SftpResponse
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Status;

    public SftpStatusResponse(uint protocolVersion)
      : base(protocolVersion)
    {
    }

    public StatusCodes StatusCode { get; private set; }

    public string ErrorMessage { get; private set; }

    public string Language { get; private set; }

    protected override void LoadData()
    {
      base.LoadData();
      this.StatusCode = (StatusCodes) this.ReadUInt32();
      if (this.ProtocolVersion < 3U || this.IsEndOfData)
        return;
      this.ErrorMessage = this.ReadString(SshData.Utf8);
      this.Language = this.ReadString(SshData.Ascii);
    }
  }
}
