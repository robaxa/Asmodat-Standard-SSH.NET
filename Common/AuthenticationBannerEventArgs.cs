// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.AuthenticationBannerEventArgs
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Common
{
  public class AuthenticationBannerEventArgs : AuthenticationEventArgs
  {
    public string BannerMessage { get; private set; }

    public string Language { get; private set; }

    public AuthenticationBannerEventArgs(string username, string message, string language)
      : base(username)
    {
      this.BannerMessage = message;
      this.Language = language;
    }
  }
}
