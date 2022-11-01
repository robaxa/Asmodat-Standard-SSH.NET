// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.KeyExchangeDhGroupExchangeGroup
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_KEX_DH_GEX_GROUP", 31)]
  public class KeyExchangeDhGroupExchangeGroup : Message
  {
    internal const byte MessageNumber = 31;
    private byte[] _safePrime;
    private byte[] _subGroup;

    public BigInteger SafePrime => this._safePrime.ToBigInteger();

    public BigInteger SubGroup => this._subGroup.ToBigInteger();

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._safePrime.Length + 4 + this._subGroup.Length;

    protected override void LoadData()
    {
      this._safePrime = this.ReadBinary();
      this._subGroup = this.ReadBinary();
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this._safePrime);
      this.WriteBinaryString(this._subGroup);
    }

    internal override void Process(Session session) => session.OnKeyExchangeDhGroupExchangeGroupReceived(this);
  }
}
