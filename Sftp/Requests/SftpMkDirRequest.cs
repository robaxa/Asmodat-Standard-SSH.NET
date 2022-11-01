// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpMkDirRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpMkDirRequest : SftpRequest
  {
    private byte[] _path;
    private byte[] _attributesBytes;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.MkDir;

    public string Path
    {
      get => this.Encoding.GetString(this._path, 0, this._path.Length);
      private set => this._path = this.Encoding.GetBytes(value);
    }

    public Encoding Encoding { get; private set; }

    private SftpFileAttributes Attributes { get; set; }

    private byte[] AttributesBytes
    {
      get
      {
        if (this._attributesBytes == null)
          this._attributesBytes = this.Attributes.GetBytes();
        return this._attributesBytes;
      }
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._path.Length + this.AttributesBytes.Length;

    public SftpMkDirRequest(
      uint protocolVersion,
      uint requestId,
      string path,
      Encoding encoding,
      Action<SftpStatusResponse> statusAction)
      : this(protocolVersion, requestId, path, encoding, SftpFileAttributes.Empty, statusAction)
    {
    }

    private SftpMkDirRequest(
      uint protocolVersion,
      uint requestId,
      string path,
      Encoding encoding,
      SftpFileAttributes attributes,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Encoding = encoding;
      this.Path = path;
      this.Attributes = attributes;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._path = this.ReadBinary();
      this.Attributes = this.ReadAttributes();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._path);
      this.Write(this.AttributesBytes);
    }
  }
}
