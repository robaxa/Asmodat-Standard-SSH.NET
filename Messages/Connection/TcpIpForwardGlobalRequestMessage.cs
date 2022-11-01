// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.TcpIpForwardGlobalRequestMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Messages.Connection
{
  internal class TcpIpForwardGlobalRequestMessage : GlobalRequestMessage
  {
    private byte[] _addressToBind;

    public TcpIpForwardGlobalRequestMessage(string addressToBind, uint portToBind)
      : base(SshData.Ascii.GetBytes("tcpip-forward"), true)
    {
      this.AddressToBind = addressToBind;
      this.PortToBind = portToBind;
    }

    public string AddressToBind
    {
      get => SshData.Utf8.GetString(this._addressToBind, 0, this._addressToBind.Length);
      private set => this._addressToBind = SshData.Utf8.GetBytes(value);
    }

    public uint PortToBind { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._addressToBind.Length + 4;

    protected override void LoadData() => throw new NotImplementedException();

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._addressToBind);
      this.Write(this.PortToBind);
    }
  }
}
