// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.InformationResponseMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Collections.Generic;

namespace Renci.SshNet.Messages.Authentication
{
  [Message("SSH_MSG_USERAUTH_INFO_RESPONSE", 61)]
  internal class InformationResponseMessage : Message
  {
    public IList<string> Responses { get; private set; }

    protected override int BufferCapacity => -1;

    public InformationResponseMessage() => this.Responses = (IList<string>) new List<string>();

    protected override void LoadData() => throw new NotImplementedException();

    protected override void SaveData()
    {
      this.Write((uint) this.Responses.Count);
      foreach (string response in (IEnumerable<string>) this.Responses)
        this.Write(response);
    }

    internal override void Process(Session session) => throw new NotImplementedException();
  }
}
