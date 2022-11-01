// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpSymLinkRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpSymLinkRequest : SftpRequest
  {
    private byte[] _newLinkPath;
    private byte[] _existingPath;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.SymLink;

    public string NewLinkPath
    {
      get => this.Encoding.GetString(this._newLinkPath, 0, this._newLinkPath.Length);
      private set => this._newLinkPath = this.Encoding.GetBytes(value);
    }

    public string ExistingPath
    {
      get => this.Encoding.GetString(this._existingPath, 0, this._existingPath.Length);
      private set => this._existingPath = this.Encoding.GetBytes(value);
    }

    public Encoding Encoding { get; set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._newLinkPath.Length + 4 + this._existingPath.Length;

    public SftpSymLinkRequest(
      uint protocolVersion,
      uint requestId,
      string newLinkPath,
      string existingPath,
      Encoding encoding,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Encoding = encoding;
      this.NewLinkPath = newLinkPath;
      this.ExistingPath = existingPath;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._newLinkPath = this.ReadBinary();
      this._existingPath = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._newLinkPath);
      this.WriteBinaryString(this._existingPath);
    }
  }
}
