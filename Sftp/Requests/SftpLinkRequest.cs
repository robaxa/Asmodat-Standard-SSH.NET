// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpLinkRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpLinkRequest : SftpRequest
  {
    private byte[] _newLinkPath;
    private byte[] _existingPath;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Link;

    public string NewLinkPath
    {
      get => SshData.Utf8.GetString(this._newLinkPath, 0, this._newLinkPath.Length);
      private set => this._newLinkPath = SshData.Utf8.GetBytes(value);
    }

    public string ExistingPath
    {
      get => SshData.Utf8.GetString(this._existingPath, 0, this._existingPath.Length);
      private set => this._existingPath = SshData.Utf8.GetBytes(value);
    }

    public bool IsSymLink { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.NewLinkPath.Length + 4 + this.ExistingPath.Length + 1;

    public SftpLinkRequest(
      uint protocolVersion,
      uint requestId,
      string newLinkPath,
      string existingPath,
      bool isSymLink,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.NewLinkPath = newLinkPath;
      this.ExistingPath = existingPath;
      this.IsSymLink = isSymLink;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._newLinkPath = this.ReadBinary();
      this._existingPath = this.ReadBinary();
      this.IsSymLink = this.ReadBoolean();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._newLinkPath);
      this.WriteBinaryString(this._existingPath);
      this.Write(this.IsSymLink);
    }
  }
}
