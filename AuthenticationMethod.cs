// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.AuthenticationMethod
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet
{
  public abstract class AuthenticationMethod : IAuthenticationMethod
  {
    public abstract string Name { get; }

    public string Username { get; private set; }

    public string[] AllowedAuthentications { get; protected set; }

    protected AuthenticationMethod(string username) => this.Username = !username.IsNullOrWhiteSpace() ? username : throw new ArgumentException(nameof (username));

    public abstract AuthenticationResult Authenticate(Session session);

    AuthenticationResult IAuthenticationMethod.Authenticate(
      ISession session)
    {
      return this.Authenticate((Session) session);
    }
  }
}
