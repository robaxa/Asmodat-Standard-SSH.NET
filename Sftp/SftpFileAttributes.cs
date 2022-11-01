// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpFileAttributes
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Renci.SshNet.Sftp
{
  public class SftpFileAttributes
  {
    private const uint S_IFMT = 61440;
    private const uint S_IFSOCK = 49152;
    private const uint S_IFLNK = 40960;
    private const uint S_IFREG = 32768;
    private const uint S_IFBLK = 24576;
    private const uint S_IFDIR = 16384;
    private const uint S_IFCHR = 8192;
    private const uint S_IFIFO = 4096;
    private const uint S_ISUID = 2048;
    private const uint S_ISGID = 1024;
    private const uint S_ISVTX = 512;
    private const uint S_IRUSR = 256;
    private const uint S_IWUSR = 128;
    private const uint S_IXUSR = 64;
    private const uint S_IRGRP = 32;
    private const uint S_IWGRP = 16;
    private const uint S_IXGRP = 8;
    private const uint S_IROTH = 4;
    private const uint S_IWOTH = 2;
    private const uint S_IXOTH = 1;
    private bool _isBitFiledsBitSet;
    private bool _isUIDBitSet;
    private bool _isGroupIDBitSet;
    private bool _isStickyBitSet;
    private readonly DateTime _originalLastAccessTime;
    private readonly DateTime _originalLastWriteTime;
    private readonly long _originalSize;
    private readonly int _originalUserId;
    private readonly int _originalGroupId;
    private readonly uint _originalPermissions;
    private readonly IDictionary<string, string> _originalExtensions;
    internal static readonly SftpFileAttributes Empty = new SftpFileAttributes();

    internal bool IsLastAccessTimeChanged => this._originalLastAccessTime != this.LastAccessTime;

    internal bool IsLastWriteTimeChanged => this._originalLastWriteTime != this.LastWriteTime;

    internal bool IsSizeChanged => this._originalSize != this.Size;

    internal bool IsUserIdChanged => this._originalUserId != this.UserId;

    internal bool IsGroupIdChanged => this._originalGroupId != this.GroupId;

    internal bool IsPermissionsChanged => (int) this._originalPermissions != (int) this.Permissions;

    internal bool IsExtensionsChanged => this._originalExtensions != null && this.Extensions != null && !this._originalExtensions.SequenceEqual<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>) this.Extensions);

    public DateTime LastAccessTime { get; set; }

    public DateTime LastWriteTime { get; set; }

    public long Size { get; set; }

    public int UserId { get; set; }

    public int GroupId { get; set; }

    public bool IsSocket { get; private set; }

    public bool IsSymbolicLink { get; private set; }

    public bool IsRegularFile { get; private set; }

    public bool IsBlockDevice { get; private set; }

    public bool IsDirectory { get; private set; }

    public bool IsCharacterDevice { get; private set; }

    public bool IsNamedPipe { get; private set; }

    public bool OwnerCanRead { get; set; }

    public bool OwnerCanWrite { get; set; }

    public bool OwnerCanExecute { get; set; }

    public bool GroupCanRead { get; set; }

    public bool GroupCanWrite { get; set; }

    public bool GroupCanExecute { get; set; }

    public bool OthersCanRead { get; set; }

    public bool OthersCanWrite { get; set; }

    public bool OthersCanExecute { get; set; }

    public IDictionary<string, string> Extensions { get; private set; }

    internal uint Permissions
    {
      get
      {
        uint permissions = 0;
        if (this._isBitFiledsBitSet)
          permissions |= 61440U;
        if (this.IsSocket)
          permissions |= 49152U;
        if (this.IsSymbolicLink)
          permissions |= 40960U;
        if (this.IsRegularFile)
          permissions |= 32768U;
        if (this.IsBlockDevice)
          permissions |= 24576U;
        if (this.IsDirectory)
          permissions |= 16384U;
        if (this.IsCharacterDevice)
          permissions |= 8192U;
        if (this.IsNamedPipe)
          permissions |= 4096U;
        if (this._isUIDBitSet)
          permissions |= 2048U;
        if (this._isGroupIDBitSet)
          permissions |= 1024U;
        if (this._isStickyBitSet)
          permissions |= 512U;
        if (this.OwnerCanRead)
          permissions |= 256U;
        if (this.OwnerCanWrite)
          permissions |= 128U;
        if (this.OwnerCanExecute)
          permissions |= 64U;
        if (this.GroupCanRead)
          permissions |= 32U;
        if (this.GroupCanWrite)
          permissions |= 16U;
        if (this.GroupCanExecute)
          permissions |= 8U;
        if (this.OthersCanRead)
          permissions |= 4U;
        if (this.OthersCanWrite)
          permissions |= 2U;
        if (this.OthersCanExecute)
          permissions |= 1U;
        return permissions;
      }
      private set
      {
        this._isBitFiledsBitSet = ((int) value & 61440) == 61440;
        this.IsSocket = ((int) value & 49152) == 49152;
        this.IsSymbolicLink = ((int) value & 40960) == 40960;
        this.IsRegularFile = ((int) value & 32768) == 32768;
        this.IsBlockDevice = ((int) value & 24576) == 24576;
        this.IsDirectory = ((int) value & 16384) == 16384;
        this.IsCharacterDevice = ((int) value & 8192) == 8192;
        this.IsNamedPipe = ((int) value & 4096) == 4096;
        this._isUIDBitSet = ((int) value & 2048) == 2048;
        this._isGroupIDBitSet = ((int) value & 1024) == 1024;
        this._isStickyBitSet = ((int) value & 512) == 512;
        this.OwnerCanRead = ((int) value & 256) == 256;
        this.OwnerCanWrite = ((int) value & 128) == 128;
        this.OwnerCanExecute = ((int) value & 64) == 64;
        this.GroupCanRead = ((int) value & 32) == 32;
        this.GroupCanWrite = ((int) value & 16) == 16;
        this.GroupCanExecute = ((int) value & 8) == 8;
        this.OthersCanRead = ((int) value & 4) == 4;
        this.OthersCanWrite = ((int) value & 2) == 2;
        this.OthersCanExecute = ((int) value & 1) == 1;
      }
    }

    private SftpFileAttributes()
    {
    }

    internal SftpFileAttributes(
      DateTime lastAccessTime,
      DateTime lastWriteTime,
      long size,
      int userId,
      int groupId,
      uint permissions,
      IDictionary<string, string> extensions)
    {
      this.LastAccessTime = this._originalLastAccessTime = lastAccessTime;
      this.LastWriteTime = this._originalLastWriteTime = lastWriteTime;
      this.Size = this._originalSize = size;
      this.UserId = this._originalUserId = userId;
      this.GroupId = this._originalGroupId = groupId;
      this.Permissions = this._originalPermissions = permissions;
      this.Extensions = this._originalExtensions = extensions;
    }

    public void SetPermissions(short mode)
    {
      char[] chArray = mode >= (short) 0 && mode <= (short) 999 ? mode.ToString((IFormatProvider) CultureInfo.InvariantCulture).PadLeft(3, '0').ToCharArray() : throw new ArgumentOutOfRangeException(nameof (mode));
      int num = ((int) chArray[0] & 15) * 8 * 8 + ((int) chArray[1] & 15) * 8 + ((int) chArray[2] & 15);
      this.OwnerCanRead = ((long) num & 256L) == 256L;
      this.OwnerCanWrite = ((long) num & 128L) == 128L;
      this.OwnerCanExecute = ((long) num & 64L) == 64L;
      this.GroupCanRead = ((long) num & 32L) == 32L;
      this.GroupCanWrite = ((long) num & 16L) == 16L;
      this.GroupCanExecute = ((long) num & 8L) == 8L;
      this.OthersCanRead = ((long) num & 4L) == 4L;
      this.OthersCanWrite = ((long) num & 2L) == 2L;
      this.OthersCanExecute = ((long) num & 1L) == 1L;
    }

    public byte[] GetBytes()
    {
      SshDataStream sshDataStream = new SshDataStream(4);
      uint num1 = 0;
      if (this.IsSizeChanged && this.IsRegularFile)
        num1 |= 1U;
      if (this.IsUserIdChanged || this.IsGroupIdChanged)
        num1 |= 2U;
      if (this.IsPermissionsChanged)
        num1 |= 4U;
      if (this.IsLastAccessTimeChanged || this.IsLastWriteTimeChanged)
        num1 |= 8U;
      if (this.IsExtensionsChanged)
        num1 |= 2147483648U;
      sshDataStream.Write(num1);
      if (this.IsSizeChanged && this.IsRegularFile)
        sshDataStream.Write((ulong) this.Size);
      if (this.IsUserIdChanged || this.IsGroupIdChanged)
      {
        sshDataStream.Write((uint) this.UserId);
        sshDataStream.Write((uint) this.GroupId);
      }
      if (this.IsPermissionsChanged)
        sshDataStream.Write(this.Permissions);
      if (this.IsLastAccessTimeChanged || this.IsLastWriteTimeChanged)
      {
        uint num2 = (uint) (this.LastAccessTime.ToFileTime() / 10000000L - 11644473600L);
        sshDataStream.Write(num2);
        uint num3 = (uint) (this.LastWriteTime.ToFileTime() / 10000000L - 11644473600L);
        sshDataStream.Write(num3);
      }
      if (this.IsExtensionsChanged)
      {
        foreach (KeyValuePair<string, string> extension in (IEnumerable<KeyValuePair<string, string>>) this.Extensions)
        {
          sshDataStream.Write(extension.Key, SshData.Ascii);
          sshDataStream.Write(extension.Value, SshData.Ascii);
        }
      }
      return sshDataStream.ToArray();
    }

    internal static SftpFileAttributes FromBytes(SshDataStream stream)
    {
      uint num = stream.ReadUInt32();
      long size = -1;
      int userId = -1;
      int groupId = -1;
      uint permissions = 0;
      DateTime lastAccessTime = DateTime.MinValue;
      DateTime lastWriteTime = DateTime.MinValue;
      IDictionary<string, string> extensions = (IDictionary<string, string>) null;
      if (((int) num & 1) == 1)
        size = (long) stream.ReadUInt64();
      if (((int) num & 2) == 2)
      {
        userId = (int) stream.ReadUInt32();
        groupId = (int) stream.ReadUInt32();
      }
      if (((int) num & 4) == 4)
        permissions = stream.ReadUInt32();
      if (((int) num & 8) == 8)
      {
        lastAccessTime = DateTime.FromFileTime(((long) stream.ReadUInt32() + 11644473600L) * 10000000L);
        lastWriteTime = DateTime.FromFileTime(((long) stream.ReadUInt32() + 11644473600L) * 10000000L);
      }
      if (((int) num & int.MinValue) == int.MinValue)
      {
        int capacity = (int) stream.ReadUInt32();
        extensions = (IDictionary<string, string>) new Dictionary<string, string>(capacity);
        for (int index = 0; index < capacity; ++index)
        {
          string key = stream.ReadString(SshData.Utf8);
          string str = stream.ReadString(SshData.Utf8);
          extensions.Add(key, str);
        }
      }
      return new SftpFileAttributes(lastAccessTime, lastWriteTime, size, userId, groupId, permissions, extensions);
    }

    internal static SftpFileAttributes FromBytes(byte[] buffer)
    {
      using (SshDataStream stream = new SshDataStream(buffer))
        return SftpFileAttributes.FromBytes(stream);
    }
  }
}
