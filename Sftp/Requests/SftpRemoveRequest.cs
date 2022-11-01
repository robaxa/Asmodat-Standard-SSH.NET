// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpRemoveRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpRemoveRequest : SftpRequest
  {
    private byte[] _fileName;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Remove;

    public string Filename
    {
      get => this.Encoding.GetString(this._fileName, 0, this._fileName.Length);
      private set => this._fileName = this.Encoding.GetBytes(value);
    }

    public Encoding Encoding { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._fileName.Length;

    public SftpRemoveRequest(
      uint protocolVersion,
      uint requestId,
      string filename,
      Encoding encoding,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Encoding = encoding;
      this.Filename = filename;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._fileName = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._fileName);
    }
  }
}
