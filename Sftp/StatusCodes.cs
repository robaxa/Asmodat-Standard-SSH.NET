// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.StatusCodes
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Sftp
{
  internal enum StatusCodes : uint
  {
    Ok,
    Eof,
    NoSuchFile,
    PermissionDenied,
    Failure,
    BadMessage,
    NoConnection,
    ConnectionLost,
    OperationUnsupported,
    InvalidHandle,
    NoSuchPath,
    FileAlreadyExists,
    WriteProtect,
    NoMedia,
    NoSpaceOnFilesystem,
    QuotaExceeded,
    UnknownPrincipal,
    LockConflict,
    DirNotEmpty,
    NotDirectory,
    InvalidFilename,
    LinkLoop,
    CannotDelete,
    InvalidParameter,
    FileIsADirectory,
    ByteRangeLockConflict,
    ByteRangeLockRefused,
    DeletePending,
    FileCorrupt,
    OwnerInvalid,
    GroupInvalid,
    NoMatchingByteRangeLock,
  }
}
