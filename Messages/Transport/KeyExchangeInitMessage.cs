// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.KeyExchangeInitMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_KEXINIT", 20)]
  public class KeyExchangeInitMessage : Message, IKeyExchangedAllowed
  {
    public KeyExchangeInitMessage()
    {
      byte[] data = new byte[16];
      CryptoAbstraction.GenerateRandom(data);
      this.Cookie = data;
    }

    public byte[] Cookie { get; private set; }

    public string[] KeyExchangeAlgorithms { get; set; }

    public string[] ServerHostKeyAlgorithms { get; set; }

    public string[] EncryptionAlgorithmsClientToServer { get; set; }

    public string[] EncryptionAlgorithmsServerToClient { get; set; }

    public string[] MacAlgorithmsClientToServer { get; set; }

    public string[] MacAlgorithmsServerToClient { get; set; }

    public string[] CompressionAlgorithmsClientToServer { get; set; }

    public string[] CompressionAlgorithmsServerToClient { get; set; }

    public string[] LanguagesClientToServer { get; set; }

    public string[] LanguagesServerToClient { get; set; }

    public bool FirstKexPacketFollows { get; set; }

    public uint Reserved { get; set; }

    protected override int BufferCapacity => -1;

    protected override void LoadData()
    {
      this.Cookie = this.ReadBytes(16);
      this.KeyExchangeAlgorithms = this.ReadNamesList();
      this.ServerHostKeyAlgorithms = this.ReadNamesList();
      this.EncryptionAlgorithmsClientToServer = this.ReadNamesList();
      this.EncryptionAlgorithmsServerToClient = this.ReadNamesList();
      this.MacAlgorithmsClientToServer = this.ReadNamesList();
      this.MacAlgorithmsServerToClient = this.ReadNamesList();
      this.CompressionAlgorithmsClientToServer = this.ReadNamesList();
      this.CompressionAlgorithmsServerToClient = this.ReadNamesList();
      this.LanguagesClientToServer = this.ReadNamesList();
      this.LanguagesServerToClient = this.ReadNamesList();
      this.FirstKexPacketFollows = this.ReadBoolean();
      this.Reserved = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      this.Write(this.Cookie);
      this.Write(this.KeyExchangeAlgorithms);
      this.Write(this.ServerHostKeyAlgorithms);
      this.Write(this.EncryptionAlgorithmsClientToServer);
      this.Write(this.EncryptionAlgorithmsServerToClient);
      this.Write(this.MacAlgorithmsClientToServer);
      this.Write(this.MacAlgorithmsServerToClient);
      this.Write(this.CompressionAlgorithmsClientToServer);
      this.Write(this.CompressionAlgorithmsServerToClient);
      this.Write(this.LanguagesClientToServer);
      this.Write(this.LanguagesServerToClient);
      this.Write(this.FirstKexPacketFollows);
      this.Write(this.Reserved);
    }

    internal override void Process(Session session) => session.OnKeyExchangeInitReceived(this);
  }
}
