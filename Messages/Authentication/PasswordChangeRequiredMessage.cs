// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.PasswordChangeRequiredMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Authentication
{
  [Renci.SshNet.Messages.Message("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", 60)]
  internal class PasswordChangeRequiredMessage : Renci.SshNet.Messages.Message
  {
    public byte[] Message { get; private set; }

    public byte[] Language { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Message.Length + 4 + this.Language.Length;

    protected override void LoadData()
    {
      this.Message = this.ReadBinary();
      this.Language = this.ReadBinary();
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this.Message);
      this.WriteBinaryString(this.Language);
    }

    internal override void Process(Session session) => session.OnUserAuthenticationPasswordChangeRequiredReceived(this);
  }
}
