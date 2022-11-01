// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.FailureMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Messages.Authentication
{
  [Renci.SshNet.Messages.Message("SSH_MSG_USERAUTH_FAILURE", 51)]
  public class FailureMessage : Renci.SshNet.Messages.Message
  {
    public string[] AllowedAuthentications { get; set; }

    public string Message { get; private set; }

    public bool PartialSuccess { get; private set; }

    protected override void LoadData()
    {
      this.AllowedAuthentications = this.ReadNamesList();
      this.PartialSuccess = this.ReadBoolean();
      if (!this.PartialSuccess)
        return;
      this.Message = string.Join(",", this.AllowedAuthentications);
    }

    protected override void SaveData() => throw new NotImplementedException();

    internal override void Process(Session session) => session.OnUserAuthenticationFailureReceived(this);
  }
}
