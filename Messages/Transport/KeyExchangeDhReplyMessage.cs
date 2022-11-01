// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.KeyExchangeDhReplyMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_KEXDH_REPLY", 31)]
  public class KeyExchangeDhReplyMessage : Message
  {
    private byte[] _fBytes;

    public byte[] HostKey { get; private set; }

    public BigInteger F => this._fBytes.ToBigInteger();

    public byte[] Signature { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.HostKey.Length + 4 + this._fBytes.Length + 4 + this.Signature.Length;

    protected override void LoadData()
    {
      this.HostKey = this.ReadBinary();
      this._fBytes = this.ReadBinary();
      this.Signature = this.ReadBinary();
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this.HostKey);
      this.WriteBinaryString(this._fBytes);
      this.WriteBinaryString(this.Signature);
    }

    internal override void Process(Session session) => session.OnKeyExchangeDhReplyMessageReceived(this);
  }
}
