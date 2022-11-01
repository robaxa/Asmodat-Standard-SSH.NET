// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.KeyExchangeDhGroupExchangeInit
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_KEX_DH_GEX_INIT", 32)]
  internal class KeyExchangeDhGroupExchangeInit : Message, IKeyExchangedAllowed
  {
    private byte[] _eBytes;

    public BigInteger E => this._eBytes.ToBigInteger();

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._eBytes.Length;

    public KeyExchangeDhGroupExchangeInit(BigInteger clientExchangeValue) => this._eBytes = clientExchangeValue.ToByteArray().Reverse<byte>();

    protected override void LoadData() => this._eBytes = this.ReadBinary();

    protected override void SaveData() => this.WriteBinaryString(this._eBytes);

    internal override void Process(Session session) => throw new NotImplementedException();
  }
}
