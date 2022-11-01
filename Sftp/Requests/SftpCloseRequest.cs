// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpCloseRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpCloseRequest : SftpRequest
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Close;

    public byte[] Handle { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Handle.Length;

    public SftpCloseRequest(
      uint protocolVersion,
      uint requestId,
      byte[] handle,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Handle = handle;
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
  }
}
