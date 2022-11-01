// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.DirectTcpipChannelInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class DirectTcpipChannelInfo : ChannelOpenInfo
  {
    private byte[] _hostToConnect;
    private byte[] _originatorAddress;
    public const string NAME = "direct-tcpip";

    public override string ChannelType => "direct-tcpip";

    public string HostToConnect
    {
      get => SshData.Utf8.GetString(this._hostToConnect, 0, this._hostToConnect.Length);
      private set => this._hostToConnect = SshData.Utf8.GetBytes(value);
    }

    public uint PortToConnect { get; private set; }

    public string OriginatorAddress
    {
      get => SshData.Utf8.GetString(this._originatorAddress, 0, this._originatorAddress.Length);
      private set => this._originatorAddress = SshData.Utf8.GetBytes(value);
    }

    public uint OriginatorPort { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._hostToConnect.Length + 4 + 4 + this._originatorAddress.Length + 4;

    public DirectTcpipChannelInfo(byte[] data) => this.Load(data);

    public DirectTcpipChannelInfo(
      string hostToConnect,
      uint portToConnect,
      string originatorAddress,
      uint originatorPort)
    {
      this.HostToConnect = hostToConnect;
      this.PortToConnect = portToConnect;
      this.OriginatorAddress = originatorAddress;
      this.OriginatorPort = originatorPort;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._hostToConnect = this.ReadBinary();
      this.PortToConnect = this.ReadUInt32();
      this._originatorAddress = this.ReadBinary();
      this.OriginatorPort = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._hostToConnect);
      this.Write(this.PortToConnect);
      this.WriteBinaryString(this._originatorAddress);
      this.Write(this.OriginatorPort);
    }
  }
}
