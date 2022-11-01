// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.UnimplementedMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_UNIMPLEMENTED", 3)]
  public class UnimplementedMessage : Message
  {
    protected override void LoadData()
    {
    }

    protected override void SaveData() => throw new NotImplementedException();

    internal override void Process(Session session) => session.OnUnimplementedReceived(this);
  }
}
