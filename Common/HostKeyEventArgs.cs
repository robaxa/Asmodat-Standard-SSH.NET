// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.HostKeyEventArgs
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Security;
using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Common
{
  public class HostKeyEventArgs : EventArgs
  {
    public bool CanTrust { get; set; }

    public byte[] HostKey { get; private set; }

    public string HostKeyName { get; private set; }

    public byte[] FingerPrint { get; private set; }

    public int KeyLength { get; private set; }

    public HostKeyEventArgs(KeyHostAlgorithm host)
    {
      this.CanTrust = true;
      this.HostKey = host.Data;
      this.HostKeyName = host.Name;
      this.KeyLength = host.Key.KeyLength;
      using (MD5 md5 = CryptoAbstraction.CreateMD5())
        this.FingerPrint = md5.ComputeHash(host.Data);
    }
  }
}
