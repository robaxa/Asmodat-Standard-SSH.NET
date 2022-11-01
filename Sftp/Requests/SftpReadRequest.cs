// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpReadRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpReadRequest : SftpRequest
  {
    private readonly Action<SftpDataResponse> _dataAction;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Read;

    public byte[] Handle { get; private set; }

    public ulong Offset { get; private set; }

    public uint Length { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Handle.Length + 8 + 4;

    public SftpReadRequest(
      uint protocolVersion,
      uint requestId,
      byte[] handle,
      ulong offset,
      uint length,
      Action<SftpDataResponse> dataAction,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Handle = handle;
      this.Offset = offset;
      this.Length = length;
      this._dataAction = dataAction;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Handle = this.ReadBinary();
      this.Offset = this.ReadUInt64();
      this.Length = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this.Handle);
      this.Write(this.Offset);
      this.Write(this.Length);
    }

    public override void Complete(SftpResponse response)
    {
      if (response is SftpDataResponse sftpDataResponse)
        this._dataAction(sftpDataResponse);
      else
        base.Complete(response);
    }
  }
}
