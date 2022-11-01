// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Compression.ZlibOpenSsh
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Messages.Authentication;
using System;

namespace Renci.SshNet.Compression
{
  public class ZlibOpenSsh : Compressor
  {
    public override string Name => "zlib@openssh.org";

    public override void Init(Session session)
    {
      base.Init(session);
      session.UserAuthenticationSuccessReceived += new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
    }

    private void Session_UserAuthenticationSuccessReceived(
      object sender,
      MessageEventArgs<SuccessMessage> e)
    {
      this.IsActive = true;
      this.Session.UserAuthenticationSuccessReceived -= new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
    }
  }
}
