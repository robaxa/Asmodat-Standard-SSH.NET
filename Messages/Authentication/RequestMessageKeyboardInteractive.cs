// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.RequestMessageKeyboardInteractive
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
  internal class RequestMessageKeyboardInteractive : RequestMessage
  {
    public byte[] Language { get; private set; }

    public byte[] SubMethods { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Language.Length + 4 + this.SubMethods.Length;

    public RequestMessageKeyboardInteractive(ServiceName serviceName, string username)
      : base(serviceName, username, "keyboard-interactive")
    {
      this.Language = Array<byte>.Empty;
      this.SubMethods = Array<byte>.Empty;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this.Language);
      this.WriteBinaryString(this.SubMethods);
    }
  }
}
