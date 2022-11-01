// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Abstractions.FileSystemAbstraction
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace Renci.SshNet.Abstractions
{
  internal class FileSystemAbstraction
  {
    public static IEnumerable<FileInfo> EnumerateFiles(
      DirectoryInfo directoryInfo,
      string searchPattern)
    {
      return directoryInfo != null ? (IEnumerable<FileInfo>) directoryInfo.GetFiles(searchPattern) : throw new ArgumentNullException(nameof (directoryInfo));
    }
  }
}
