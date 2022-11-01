// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpUploadAsyncResult
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Sftp
{
  public class SftpUploadAsyncResult : AsyncResult
  {
    public bool IsUploadCanceled { get; set; }

    public ulong UploadedBytes { get; private set; }

    public SftpUploadAsyncResult(AsyncCallback asyncCallback, object state)
      : base(asyncCallback, state)
    {
    }

    internal void Update(ulong uploadedBytes) => this.UploadedBytes = uploadedBytes;
  }
}
