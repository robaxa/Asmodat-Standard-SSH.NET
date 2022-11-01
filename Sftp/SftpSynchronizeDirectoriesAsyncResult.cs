// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpSynchronizeDirectoriesAsyncResult
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Renci.SshNet.Sftp
{
  public class SftpSynchronizeDirectoriesAsyncResult : AsyncResult<IEnumerable<FileInfo>>
  {
    public int FilesRead { get; private set; }

    public SftpSynchronizeDirectoriesAsyncResult(AsyncCallback asyncCallback, object state)
      : base(asyncCallback, state)
    {
    }

    internal void Update(int filesRead) => this.FilesRead = filesRead;
  }
}
