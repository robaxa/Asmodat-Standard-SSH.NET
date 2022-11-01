// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ShellRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  internal class ShellRequestInfo : RequestInfo
  {
    public const string Name = "shell";

    public override string RequestName => "shell";

    public ShellRequestInfo() => this.WantReply = true;
  }
}
