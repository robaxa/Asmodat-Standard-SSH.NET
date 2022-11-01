// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.AuthenticationPrompt
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Common
{
  public class AuthenticationPrompt
  {
    public int Id { get; private set; }

    public bool IsEchoed { get; private set; }

    public string Request { get; private set; }

    public string Response { get; set; }

    public AuthenticationPrompt(int id, bool isEchoed, string request)
    {
      this.Id = id;
      this.IsEchoed = isEchoed;
      this.Request = request;
    }
  }
}
