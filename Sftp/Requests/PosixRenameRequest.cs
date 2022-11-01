// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.PosixRenameRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class PosixRenameRequest : SftpExtendedRequest
  {
    private byte[] _oldPath;
    private byte[] _newPath;

    public string OldPath
    {
      get => this.Encoding.GetString(this._oldPath, 0, this._oldPath.Length);
      private set => this._oldPath = this.Encoding.GetBytes(value);
    }

    public string NewPath
    {
      get => this.Encoding.GetString(this._newPath, 0, this._newPath.Length);
      private set => this._newPath = this.Encoding.GetBytes(value);
    }

    public Encoding Encoding { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._oldPath.Length + 4 + this._newPath.Length;

    public PosixRenameRequest(
      uint protocolVersion,
      uint requestId,
      string oldPath,
      string newPath,
      Encoding encoding,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction, "posix-rename@openssh.com")
    {
      this.Encoding = encoding;
      this.OldPath = oldPath;
      this.NewPath = newPath;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._oldPath);
      this.WriteBinaryString(this._newPath);
    }
  }
}
