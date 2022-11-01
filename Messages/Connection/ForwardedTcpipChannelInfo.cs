// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ForwardedTcpipChannelInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class ForwardedTcpipChannelInfo : ChannelOpenInfo
  {
    private byte[] _connectedAddress;
    private byte[] _originatorAddress;
    public const string NAME = "forwarded-tcpip";

    public ForwardedTcpipChannelInfo(byte[] data) => this.Load(data);

    public ForwardedTcpipChannelInfo(
      string connectedAddress,
      uint connectedPort,
      string originatorAddress,
      uint originatorPort)
    {
      this.ConnectedAddress = connectedAddress;
      this.ConnectedPort = connectedPort;
      this.OriginatorAddress = originatorAddress;
      this.OriginatorPort = originatorPort;
    }

    public override string ChannelType => "forwarded-tcpip";

    public string ConnectedAddress
    {
      get => SshData.Utf8.GetString(this._connectedAddress, 0, this._connectedAddress.Length);
      private set => this._connectedAddress = SshData.Utf8.GetBytes(value);
    }

    public uint ConnectedPort { get; private set; }

    public string OriginatorAddress
    {
      get => SshData.Utf8.GetString(this._originatorAddress, 0, this._originatorAddress.Length);
      private set => this._originatorAddress = SshData.Utf8.GetBytes(value);
    }

    public uint OriginatorPort { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._connectedAddress.Length + 4 + 4 + this._originatorAddress.Length + 4;

    protected override void LoadData()
    {
      base.LoadData();
      this._connectedAddress = this.ReadBinary();
      this.ConnectedPort = this.ReadUInt32();
      this._originatorAddress = this.ReadBinary();
      this.OriginatorPort = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._connectedAddress);
      this.Write(this.ConnectedPort);
      this.WriteBinaryString(this._originatorAddress);
      this.Write(this.OriginatorPort);
    }
  }
}
