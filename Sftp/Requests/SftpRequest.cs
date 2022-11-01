// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal abstract class SftpRequest : SftpMessage
  {
    private readonly Action<SftpStatusResponse> _statusAction;

    public uint RequestId { get; private set; }

    public uint ProtocolVersion { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4;

    protected SftpRequest(
      uint protocolVersion,
      uint requestId,
      Action<SftpStatusResponse> statusAction)
    {
      this.RequestId = requestId;
      this.ProtocolVersion = protocolVersion;
      this._statusAction = statusAction;
    }

    public virtual void Complete(SftpResponse response)
    {
      if (!(response is SftpStatusResponse sftpStatusResponse))
        throw new InvalidOperationException(string.Format("Response of type '{0}' is not expected.", (object) response.GetType().Name));
      this._statusAction(sftpStatusResponse);
    }

    protected override void LoadData() => throw new InvalidOperationException("Request cannot be saved.");

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.RequestId);
    }
  }
}
