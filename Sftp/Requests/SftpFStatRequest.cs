// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpFStatRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpFStatRequest : SftpRequest
  {
    private readonly Action<SftpAttrsResponse> _attrsAction;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.FStat;

    public byte[] Handle { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Handle.Length;

    public SftpFStatRequest(
      uint protocolVersion,
      uint requestId,
      byte[] handle,
      Action<SftpAttrsResponse> attrsAction,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Handle = handle;
      this._attrsAction = attrsAction;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Handle = this.ReadBinary();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this.Handle);
    }

    public override void Complete(SftpResponse response)
    {
      if (response is SftpAttrsResponse sftpAttrsResponse)
        this._attrsAction(sftpAttrsResponse);
      else
        base.Complete(response);
    }
  }
}
