// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpFSetStatRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpFSetStatRequest : SftpRequest
  {
    private byte[] _attributesBytes;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.FSetStat;

    public byte[] Handle { get; private set; }

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

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Handle.Length + this.AttributesBytes.Length;

    public SftpFSetStatRequest(
      uint protocolVersion,
      uint requestId,
      byte[] handle,
      SftpFileAttributes attributes,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Handle = handle;
      this.Attributes = attributes;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Handle = this.ReadBinary();
      this.Attributes = this.ReadAttributes();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this.Handle);
      this.Write(this.AttributesBytes);
    }
  }
}
