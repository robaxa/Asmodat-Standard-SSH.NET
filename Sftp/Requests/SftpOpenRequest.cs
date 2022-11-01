// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpOpenRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Text;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpOpenRequest : SftpRequest
  {
    private byte[] _fileName;
    private byte[] _attributes;
    private readonly Action<SftpHandleResponse> _handleAction;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Open;

    public string Filename
    {
      get => this.Encoding.GetString(this._fileName, 0, this._fileName.Length);
      private set => this._fileName = this.Encoding.GetBytes(value);
    }

    public Flags Flags { get; private set; }

    public SftpFileAttributes Attributes
    {
      get => SftpFileAttributes.FromBytes(this._attributes);
      private set => this._attributes = value.GetBytes();
    }

    public Encoding Encoding { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._fileName.Length + 4 + this._attributes.Length;

    public SftpOpenRequest(
      uint protocolVersion,
      uint requestId,
      string fileName,
      Encoding encoding,
      Flags flags,
      Action<SftpHandleResponse> handleAction,
      Action<SftpStatusResponse> statusAction)
      : this(protocolVersion, requestId, fileName, encoding, flags, SftpFileAttributes.Empty, handleAction, statusAction)
    {
    }

    private SftpOpenRequest(
      uint protocolVersion,
      uint requestId,
      string fileName,
      Encoding encoding,
      Flags flags,
      SftpFileAttributes attributes,
      Action<SftpHandleResponse> handleAction,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Encoding = encoding;
      this.Filename = fileName;
      this.Flags = flags;
      this.Attributes = attributes;
      this._handleAction = handleAction;
    }

    protected override void LoadData()
    {
      base.LoadData();
      throw new NotSupportedException();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._fileName);
      this.Write((uint) this.Flags);
      this.Write(this._attributes);
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
