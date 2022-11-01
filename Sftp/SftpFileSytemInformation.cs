// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpFileSytemInformation
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
  public class SftpFileSytemInformation
  {
    internal const ulong SSH_FXE_STATVFS_ST_RDONLY = 1;
    internal const ulong SSH_FXE_STATVFS_ST_NOSUID = 2;
    private readonly ulong _flag;

    public ulong FileSystemBlockSize { get; private set; }

    public ulong BlockSize { get; private set; }

    public ulong TotalBlocks { get; private set; }

    public ulong FreeBlocks { get; private set; }

    public ulong AvailableBlocks { get; private set; }

    public ulong TotalNodes { get; private set; }

    public ulong FreeNodes { get; private set; }

    public ulong AvailableNodes { get; private set; }

    public ulong Sid { get; private set; }

    public bool IsReadOnly => ((long) this._flag & 1L) == 1L;

    public bool SupportsSetUid => ((long) this._flag & 2L) == 0L;

    public ulong MaxNameLenght { get; private set; }

    internal SftpFileSytemInformation(
      ulong bsize,
      ulong frsize,
      ulong blocks,
      ulong bfree,
      ulong bavail,
      ulong files,
      ulong ffree,
      ulong favail,
      ulong sid,
      ulong flag,
      ulong namemax)
    {
      this.FileSystemBlockSize = bsize;
      this.BlockSize = frsize;
      this.TotalBlocks = blocks;
      this.FreeBlocks = bfree;
      this.AvailableBlocks = bavail;
      this.TotalNodes = files;
      this.FreeNodes = ffree;
      this.AvailableNodes = favail;
      this.Sid = sid;
      this._flag = flag;
      this.MaxNameLenght = namemax;
    }

    internal void SaveData(SshDataStream stream)
    {
      stream.Write(this.FileSystemBlockSize);
      stream.Write(this.BlockSize);
      stream.Write(this.TotalBlocks);
      stream.Write(this.FreeBlocks);
      stream.Write(this.AvailableBlocks);
      stream.Write(this.TotalNodes);
      stream.Write(this.FreeNodes);
      stream.Write(this.AvailableNodes);
      stream.Write(this.Sid);
      stream.Write(this._flag);
      stream.Write(this.MaxNameLenght);
    }
  }
}
