// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.RemotePathTransformation
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet
{
  public static class RemotePathTransformation
  {
    private static readonly IRemotePathTransformation ShellQuoteTransformation = (IRemotePathTransformation) new RemotePathShellQuoteTransformation();
    private static readonly IRemotePathTransformation NoneTransformation = (IRemotePathTransformation) new RemotePathNoneTransformation();
    private static readonly IRemotePathTransformation DoubleQuoteTransformation = (IRemotePathTransformation) new RemotePathDoubleQuoteTransformation();

    public static IRemotePathTransformation ShellQuote => RemotePathTransformation.ShellQuoteTransformation;

    public static IRemotePathTransformation None => RemotePathTransformation.NoneTransformation;

    public static IRemotePathTransformation DoubleQuote => RemotePathTransformation.DoubleQuoteTransformation;
  }
}
