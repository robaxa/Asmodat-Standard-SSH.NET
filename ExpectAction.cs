// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ExpectAction
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
  public class ExpectAction
  {
    public Regex Expect { get; private set; }

    public System.Action<string> Action { get; private set; }

    public ExpectAction(Regex expect, System.Action<string> action)
    {
      if (expect == null)
        throw new ArgumentNullException(nameof (expect));
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      this.Expect = expect;
      this.Action = action;
    }

    public ExpectAction(string expect, System.Action<string> action)
    {
      if (expect == null)
        throw new ArgumentNullException(nameof (expect));
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      this.Expect = new Regex(Regex.Escape(expect));
      this.Action = action;
    }
  }
}
