// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpRealPathRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpRealPathRequest : SftpRequest
  {
    private byte[] _path;
    private readonly Action<SftpNameResponse> _nameAction;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.RealPath;

    public string Path
    {
      get => this.Encoding.GetString(this._path, 0, this._path.Length);
      private set => this._path = this.Encoding.GetBytes(value);
    }

    public Encoding Encoding { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._path.Length;

    public SftpRealPathRequest(
      uint protocolVersion,
      uint requestId,
      string path,
      Encoding encoding,
      Action<SftpNameResponse> nameAction,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      if (nameAction == null)
        throw new ArgumentNullException(nameof (nameAction));
      this.Encoding = encoding;
      this.Path = path;
      this._nameAction = nameAction;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._path);
    }

    public override void Complete(SftpResponse response)
    {
      if (response is SftpNameResponse sftpNameResponse)
        this._nameAction(sftpNameResponse);
      else
        base.Complete(response);
    }
  }
}
