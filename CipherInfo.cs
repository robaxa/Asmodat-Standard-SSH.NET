// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.CipherInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet
{
  public class CipherInfo
  {
    public int KeySize { get; private set; }

    public Func<byte[], byte[], Renci.SshNet.Security.Cryptography.Cipher> Cipher { get; private set; }

    public CipherInfo(int keySize, Func<byte[], byte[], Renci.SshNet.Security.Cryptography.Cipher> cipher)
    {
      CipherInfo cipherInfo = this;
      this.KeySize = keySize;
      this.Cipher = (Func<byte[], byte[], Renci.SshNet.Security.Cryptography.Cipher>) ((key, iv) => cipher(key.Take(cipherInfo.KeySize / 8), iv));
    }
  }
}
