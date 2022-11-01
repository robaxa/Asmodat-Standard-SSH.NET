// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpMessageTypes
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Sftp
{
  internal enum SftpMessageTypes : byte
  {
    Init = 1,
    Version = 2,
    Open = 3,
    Close = 4,
    Read = 5,
    Write = 6,
    LStat = 7,
    FStat = 8,
    SetStat = 9,
    FSetStat = 10, // 0x0A
    OpenDir = 11, // 0x0B
    ReadDir = 12, // 0x0C
    Remove = 13, // 0x0D
    MkDir = 14, // 0x0E
    RmDir = 15, // 0x0F
    RealPath = 16, // 0x10
    Stat = 17, // 0x11
    Rename = 18, // 0x12
    ReadLink = 19, // 0x13
    SymLink = 20, // 0x14
    Link = 21, // 0x15
    Block = 22, // 0x16
    Unblock = 23, // 0x17
    Status = 101, // 0x65
    Handle = 102, // 0x66
    Data = 103, // 0x67
    Name = 104, // 0x68
    Attrs = 105, // 0x69
    Extended = 200, // 0xC8
    ExtendedReply = 201, // 0xC9
  }
}
