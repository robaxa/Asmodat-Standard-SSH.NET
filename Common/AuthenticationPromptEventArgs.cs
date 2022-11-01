// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.AuthenticationPromptEventArgs
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System.Collections.Generic;

namespace Renci.SshNet.Common
{
  public class AuthenticationPromptEventArgs : AuthenticationEventArgs
  {
    public string Language { get; private set; }

    public string Instruction { get; private set; }

    public IEnumerable<AuthenticationPrompt> Prompts { get; private set; }

    public AuthenticationPromptEventArgs(
      string username,
      string instruction,
      string language,
      IEnumerable<AuthenticationPrompt> prompts)
      : base(username)
    {
      this.Instruction = instruction;
      this.Language = language;
      this.Prompts = prompts;
    }
  }
}
