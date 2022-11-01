// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.KeyExchangeDhGroupExchangeRequest
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_KEX_DH_GEX_REQUEST", 34)]
  internal class KeyExchangeDhGroupExchangeRequest : Message, IKeyExchangedAllowed
  {
    internal const byte MessageNumber = 34;

    public uint Minimum { get; private set; }

    public uint Preferred { get; private set; }

    public uint Maximum { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + 4 + 4;

    public KeyExchangeDhGroupExchangeRequest(uint minimum, uint preferred, uint maximum)
    {
      this.Minimum = minimum;
      this.Preferred = preferred;
      this.Maximum = maximum;
    }

    protected override void LoadData()
    {
      this.Minimum = this.ReadUInt32();
      this.Preferred = this.ReadUInt32();
      this.Maximum = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      this.Write(this.Minimum);
      this.Write(this.Preferred);
      this.Write(this.Maximum);
    }

    internal override void Process(Session session) => throw new NotImplementedException();
  }
}
