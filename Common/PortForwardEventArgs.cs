// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.PortForwardEventArgs
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Common
{
  public class PortForwardEventArgs : EventArgs
  {
    public string OriginatorHost { get; private set; }

    public uint OriginatorPort { get; private set; }

    internal PortForwardEventArgs(string host, uint port)
    {
      if (host == null)
        throw new ArgumentNullException(nameof (host));
      port.ValidatePort(nameof (port));
      this.OriginatorHost = host;
      this.OriginatorPort = port;
    }
  }
}
