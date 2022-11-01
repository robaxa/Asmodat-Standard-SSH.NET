// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpFile
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
  public class SftpFile
  {
    private readonly ISftpSession _sftpSession;

    public SftpFileAttributes Attributes { get; private set; }

    internal SftpFile(ISftpSession sftpSession, string fullName, SftpFileAttributes attributes)
    {
      if (sftpSession == null)
        throw new SshConnectionException("Client not connected.");
      if (attributes == null)
        throw new ArgumentNullException(nameof (attributes));
      if (fullName == null)
        throw new ArgumentNullException(nameof (fullName));
      this._sftpSession = sftpSession;
      this.Attributes = attributes;
      this.Name = fullName.Substring(fullName.LastIndexOf('/') + 1);
      this.FullName = fullName;
    }

    public string FullName { get; private set; }

    public string Name { get; private set; }

    public DateTime LastAccessTime
    {
      get => this.Attributes.LastAccessTime;
      set => this.Attributes.LastAccessTime = value;
    }

    public DateTime LastWriteTime
    {
      get => this.Attributes.LastWriteTime;
      set => this.Attributes.LastWriteTime = value;
    }

    public DateTime LastAccessTimeUtc
    {
      get => this.Attributes.LastAccessTime.ToUniversalTime();
      set => this.Attributes.LastAccessTime = value.ToLocalTime();
    }

    public DateTime LastWriteTimeUtc
    {
      get => this.Attributes.LastWriteTime.ToUniversalTime();
      set => this.Attributes.LastWriteTime = value.ToLocalTime();
    }

    public long Length => this.Attributes.Size;

    public int UserId
    {
      get => this.Attributes.UserId;
      set => this.Attributes.UserId = value;
    }

    public int GroupId
    {
      get => this.Attributes.GroupId;
      set => this.Attributes.GroupId = value;
    }

    public bool IsSocket => this.Attributes.IsSocket;

    public bool IsSymbolicLink => this.Attributes.IsSymbolicLink;

    public bool IsRegularFile => this.Attributes.IsRegularFile;

    public bool IsBlockDevice => this.Attributes.IsBlockDevice;

    public bool IsDirectory => this.Attributes.IsDirectory;

    public bool IsCharacterDevice => this.Attributes.IsCharacterDevice;

    public bool IsNamedPipe => this.Attributes.IsNamedPipe;

    public bool OwnerCanRead
    {
      get => this.Attributes.OwnerCanRead;
      set => this.Attributes.OwnerCanRead = value;
    }

    public bool OwnerCanWrite
    {
      get => this.Attributes.OwnerCanWrite;
      set => this.Attributes.OwnerCanWrite = value;
    }

    public bool OwnerCanExecute
    {
      get => this.Attributes.OwnerCanExecute;
      set => this.Attributes.OwnerCanExecute = value;
    }

    public bool GroupCanRead
    {
      get => this.Attributes.GroupCanRead;
      set => this.Attributes.GroupCanRead = value;
    }

    public bool GroupCanWrite
    {
      get => this.Attributes.GroupCanWrite;
      set => this.Attributes.GroupCanWrite = value;
    }

    public bool GroupCanExecute
    {
      get => this.Attributes.GroupCanExecute;
      set => this.Attributes.GroupCanExecute = value;
    }

    public bool OthersCanRead
    {
      get => this.Attributes.OthersCanRead;
      set => this.Attributes.OthersCanRead = value;
    }

    public bool OthersCanWrite
    {
      get => this.Attributes.OthersCanWrite;
      set => this.Attributes.OthersCanWrite = value;
    }

    public bool OthersCanExecute
    {
      get => this.Attributes.OthersCanExecute;
      set => this.Attributes.OthersCanExecute = value;
    }

    public void SetPermissions(short mode)
    {
      this.Attributes.SetPermissions(mode);
      this.UpdateStatus();
    }

    public void Delete()
    {
      if (this.IsDirectory)
        this._sftpSession.RequestRmDir(this.FullName);
      else
        this._sftpSession.RequestRemove(this.FullName);
    }

    public void MoveTo(string destFileName)
    {
      if (destFileName == null)
        throw new ArgumentNullException(nameof (destFileName));
      this._sftpSession.RequestRename(this.FullName, destFileName);
      string canonicalPath = this._sftpSession.GetCanonicalPath(destFileName);
      this.Name = canonicalPath.Substring(canonicalPath.LastIndexOf('/') + 1);
      this.FullName = canonicalPath;
    }

    public void UpdateStatus() => this._sftpSession.RequestSetStat(this.FullName, this.Attributes);

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Name {0}, Length {1}, User ID {2}, Group ID {3}, Accessed {4}, Modified {5}", (object) this.Name, (object) this.Length, (object) this.UserId, (object) this.GroupId, (object) this.LastAccessTime, (object) this.LastWriteTime);
  }
}
