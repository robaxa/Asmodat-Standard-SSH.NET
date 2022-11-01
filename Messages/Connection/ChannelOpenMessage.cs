// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelOpenMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Globalization;

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_CHANNEL_OPEN", 90)]
  public class ChannelOpenMessage : Message
  {
    internal const byte MessageNumber = 90;
    private byte[] _infoBytes;

    public byte[] ChannelType { get; private set; }

    public uint LocalChannelNumber { get; protected set; }

    public uint InitialWindowSize { get; private set; }

    public uint MaximumPacketSize { get; private set; }

    public ChannelOpenInfo Info { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.ChannelType.Length + 4 + 4 + 4 + this._infoBytes.Length;

    public ChannelOpenMessage()
    {
    }

    public ChannelOpenMessage(
      uint channelNumber,
      uint initialWindowSize,
      uint maximumPacketSize,
      ChannelOpenInfo info)
    {
      if (info == null)
        throw new ArgumentNullException(nameof (info));
      this.ChannelType = SshData.Ascii.GetBytes(info.ChannelType);
      this.LocalChannelNumber = channelNumber;
      this.InitialWindowSize = initialWindowSize;
      this.MaximumPacketSize = maximumPacketSize;
      this.Info = info;
      this._infoBytes = info.GetBytes();
    }

    protected override void LoadData()
    {
      this.ChannelType = this.ReadBinary();
      this.LocalChannelNumber = this.ReadUInt32();
      this.InitialWindowSize = this.ReadUInt32();
      this.MaximumPacketSize = this.ReadUInt32();
      this._infoBytes = this.ReadBytes();
      string str1 = SshData.Ascii.GetString(this.ChannelType, 0, this.ChannelType.Length);
      string str2 = str1;
      if (!(str2 == "session"))
      {
        if (!(str2 == "x11"))
        {
          if (!(str2 == "direct-tcpip"))
          {
            if (!(str2 == "forwarded-tcpip"))
              throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Channel type '{0}' is not supported.", (object) str1));
            this.Info = (ChannelOpenInfo) new ForwardedTcpipChannelInfo(this._infoBytes);
          }
          else
            this.Info = (ChannelOpenInfo) new DirectTcpipChannelInfo(this._infoBytes);
        }
        else
          this.Info = (ChannelOpenInfo) new X11ChannelOpenInfo(this._infoBytes);
      }
      else
        this.Info = (ChannelOpenInfo) new SessionChannelOpenInfo(this._infoBytes);
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this.ChannelType);
      this.Write(this.LocalChannelNumber);
      this.Write(this.InitialWindowSize);
      this.Write(this.MaximumPacketSize);
      this.Write(this._infoBytes);
    }

    internal override void Process(Session session) => session.OnChannelOpenReceived(this);
  }
}
