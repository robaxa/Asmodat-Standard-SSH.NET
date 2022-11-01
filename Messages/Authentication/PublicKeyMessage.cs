// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.PublicKeyMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Authentication
{
  [Message("SSH_MSG_USERAUTH_PK_OK", 60)]
  internal class PublicKeyMessage : Message
  {
    public byte[] PublicKeyAlgorithmName { get; private set; }

    public byte[] PublicKeyData { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.PublicKeyAlgorithmName.Length + 4 + this.PublicKeyData.Length;

    internal override void Process(Session session) => session.OnUserAuthenticationPublicKeyReceived(this);

    protected override void LoadData()
    {
      this.PublicKeyAlgorithmName = this.ReadBinary();
      this.PublicKeyData = this.ReadBinary();
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this.PublicKeyAlgorithmName);
      this.WriteBinaryString(this.PublicKeyData);
    }
  }
}
