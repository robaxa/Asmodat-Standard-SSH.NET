// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.ChannelOpenConfirmedEventArgs
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Common
{
  internal class ChannelOpenConfirmedEventArgs : ChannelEventArgs
  {
    public ChannelOpenConfirmedEventArgs(
      uint remoteChannelNumber,
      uint initialWindowSize,
      uint maximumPacketSize)
      : base(remoteChannelNumber)
    {
      this.InitialWindowSize = initialWindowSize;
      this.MaximumPacketSize = maximumPacketSize;
    }

    public uint InitialWindowSize { get; private set; }

    public uint MaximumPacketSize { get; private set; }
  }
}
