// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Requests.SftpExtendedRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Sftp.Responses;
using System;

namespace Renci.SshNet.Sftp.Requests
{
  internal abstract class SftpExtendedRequest : SftpRequest
  {
    private byte[] _nameBytes;
    private string _name;

    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.Extended;

    public string Name
    {
      get => this._name;
      private set
      {
        this._name = value;
        this._nameBytes = SshData.Utf8.GetBytes(value);
      }
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._nameBytes.Length;

    protected SftpExtendedRequest(
      uint protocolVersion,
      uint requestId,
      Action<SftpStatusResponse> statusAction,
      string name)
      : base(protocolVersion, requestId, statusAction)
    {
      this.Name = name;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._nameBytes);
    }
  }
}
