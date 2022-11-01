// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.RemotePathDoubleQuoteTransformation
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Text;

namespace Renci.SshNet
{
  internal class RemotePathDoubleQuoteTransformation : IRemotePathTransformation
  {
    public string Transform(string path)
    {
      StringBuilder stringBuilder = path != null ? new StringBuilder(path.Length) : throw new ArgumentNullException(nameof (path));
      stringBuilder.Append('"');
      foreach (char ch in path)
      {
        if (ch == '"')
          stringBuilder.Append('\\');
        stringBuilder.Append(ch);
      }
      stringBuilder.Append('"');
      return stringBuilder.ToString();
    }
  }
}
