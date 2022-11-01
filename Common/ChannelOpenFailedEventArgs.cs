// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.ChannelOpenFailedEventArgs
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Common
{
  internal class ChannelOpenFailedEventArgs : ChannelEventArgs
  {
    public uint ReasonCode { get; private set; }

    public string Description { get; private set; }

    public string Language { get; private set; }

    public ChannelOpenFailedEventArgs(
      uint channelNumber,
      uint reasonCode,
      string description,
      string language)
      : base(channelNumber)
    {
      this.ReasonCode = reasonCode;
      this.Description = description;
      this.Language = language;
    }
  }
}
