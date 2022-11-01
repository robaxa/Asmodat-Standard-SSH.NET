// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpOpenDirRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpOpenDirRequest : SftpRequest
  {
    private byte[] _path;
    private readonly Action<SftpHandleResponse> _handleAction;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.OpenDir;

    public string Path
    {
      get => this.Encoding.GetString(this._path, 0, this._path.Length);
      private set => this._path = this.Encoding.GetBytes(value);
    }

    public Encoding Encoding { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._path.Length;

    public SftpOpenDirRequest(
      uint protocolVersion,
      uint requestId,
      string path,
      Encoding encoding,
      Action<SftpHandleResponse> handleAction,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Encoding = encoding;
      this.Path = path;
      this._handleAction = handleAction;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._path = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._path);
    }

    public override void Complete(SftpResponse response)
    {
      if (response is SftpHandleResponse sftpHandleResponse)
        this._handleAction(sftpHandleResponse);
      else
        base.Complete(response);
    }
  }
}
