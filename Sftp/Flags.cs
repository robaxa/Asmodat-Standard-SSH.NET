// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Flags
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Sftp
{
  [Flags]
  internal enum Flags
  {
    None = 0,
    Read = 1,
    Write = 2,
    Append = 4,
    CreateNewOrOpen = 8,
    Truncate = 16, // 0x00000010
    CreateNew = 40, // 0x00000028
  }
}
