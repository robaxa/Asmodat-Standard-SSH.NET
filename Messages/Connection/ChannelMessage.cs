// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ChannelMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Globalization;

namespace Renci.SshNet.Messages.Connection
{
  public abstract class ChannelMessage : Message
  {
    public uint LocalChannelNumber { get; protected set; }

    protected override int BufferCapacity => base.BufferCapacity + 4;

    protected ChannelMessage()
    {
    }

    protected ChannelMessage(uint localChannelNumber) => this.LocalChannelNumber = localChannelNumber;

    protected override void LoadData() => this.LocalChannelNumber = this.ReadUInt32();

    protected override void SaveData() => this.Write(this.LocalChannelNumber);

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{0} : #{1}", (object) base.ToString(), (object) this.LocalChannelNumber);
  }
}
