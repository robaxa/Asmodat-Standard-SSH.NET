// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.RemotePathShellQuoteTransformation
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Text;

namespace Renci.SshNet
{
  internal class RemotePathShellQuoteTransformation : IRemotePathTransformation
  {
    public string Transform(string path)
    {
      StringBuilder stringBuilder = path != null ? new StringBuilder(path.Length + 2) : throw new ArgumentNullException(nameof (path));
      RemotePathShellQuoteTransformation.ShellQuoteState shellQuoteState = RemotePathShellQuoteTransformation.ShellQuoteState.Unquoted;
      foreach (char ch in path)
      {
        switch (ch)
        {
          case '!':
            switch (shellQuoteState)
            {
              case RemotePathShellQuoteTransformation.ShellQuoteState.Unquoted:
                stringBuilder.Append('\\');
                break;
              case RemotePathShellQuoteTransformation.ShellQuoteState.SingleQuoted:
                stringBuilder.Append('\'');
                stringBuilder.Append('\\');
                break;
              case RemotePathShellQuoteTransformation.ShellQuoteState.Quoted:
                stringBuilder.Append('"');
                stringBuilder.Append('\\');
                break;
            }
            shellQuoteState = RemotePathShellQuoteTransformation.ShellQuoteState.Unquoted;
            break;
          case '\'':
            switch (shellQuoteState)
            {
              case RemotePathShellQuoteTransformation.ShellQuoteState.Unquoted:
                stringBuilder.Append('"');
                break;
              case RemotePathShellQuoteTransformation.ShellQuoteState.SingleQuoted:
                stringBuilder.Append('\'');
                stringBuilder.Append('"');
                break;
            }
            shellQuoteState = RemotePathShellQuoteTransformation.ShellQuoteState.Quoted;
            break;
          default:
            switch (shellQuoteState)
            {
              case RemotePathShellQuoteTransformation.ShellQuoteState.Unquoted:
                stringBuilder.Append('\'');
                break;
              case RemotePathShellQuoteTransformation.ShellQuoteState.Quoted:
                stringBuilder.Append('"');
                stringBuilder.Append('\'');
                break;
            }
            shellQuoteState = RemotePathShellQuoteTransformation.ShellQuoteState.SingleQuoted;
            break;
        }
        stringBuilder.Append(ch);
      }
      switch (shellQuoteState)
      {
        case RemotePathShellQuoteTransformation.ShellQuoteState.SingleQuoted:
          stringBuilder.Append('\'');
          break;
        case RemotePathShellQuoteTransformation.ShellQuoteState.Quoted:
          stringBuilder.Append('"');
          break;
      }
      if (stringBuilder.Length == 0)
        stringBuilder.Append("''");
      return stringBuilder.ToString();
    }

    private enum ShellQuoteState
    {
      Unquoted = 1,
      SingleQuoted = 2,
      Quoted = 3,
    }
  }
}
