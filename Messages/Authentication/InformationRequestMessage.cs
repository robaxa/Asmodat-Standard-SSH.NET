// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.InformationRequestMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Messages.Authentication
{
  [Message("SSH_MSG_USERAUTH_INFO_REQUEST", 60)]
  internal class InformationRequestMessage : Message
  {
    public string Name { get; private set; }

    public string Instruction { get; private set; }

    public string Language { get; private set; }

    public IEnumerable<AuthenticationPrompt> Prompts { get; private set; }

    protected override void LoadData()
    {
      this.Name = this.ReadString(Encoding.UTF8);
      this.Instruction = this.ReadString(Encoding.UTF8);
      this.Language = this.ReadString(SshData.Ascii);
      uint num = this.ReadUInt32();
      List<AuthenticationPrompt> authenticationPromptList = new List<AuthenticationPrompt>();
      for (int id = 0; (long) id < (long) num; ++id)
      {
        string request = this.ReadString(Encoding.UTF8);
        bool isEchoed = this.ReadBoolean();
        authenticationPromptList.Add(new AuthenticationPrompt(id, isEchoed, request));
      }
      this.Prompts = (IEnumerable<AuthenticationPrompt>) authenticationPromptList;
    }

    protected override void SaveData() => throw new NotImplementedException();

    internal override void Process(Session session) => session.OnUserAuthenticationInformationRequestReceived(this);
  }
}
