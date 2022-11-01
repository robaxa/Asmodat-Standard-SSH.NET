// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpWriteRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal class SftpWriteRequest : SftpRequest
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Write;

    public byte[] Handle { get; private set; }

    public ulong ServerFileOffset { get; private set; }

    public byte[] Data { get; private set; }

    public int Offset { get; private set; }

    public int Length { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Handle.Length + 8 + 4 + this.Length;

    public SftpWriteRequest(
      uint protocolVersion,
      uint requestId,
      byte[] handle,
      ulong serverFileOffset,
      byte[] data,
      int offset,
      int length,
      Action<SftpStatusResponse> statusAction)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Handle = handle;
      this.ServerFileOffset = serverFileOffset;
      this.Data = data;
      this.Offset = offset;
      this.Length = length;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.Handle = this.ReadBinary();
      this.ServerFileOffset = this.ReadUInt64();
      this.Data = this.ReadBinary();
      this.Offset = 0;
      this.Length = this.Data.Length;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this.Handle);
      this.Write(this.ServerFileOffset);
      this.WriteBinary(this.Data, this.Offset, this.Length);
    }
  }
}
