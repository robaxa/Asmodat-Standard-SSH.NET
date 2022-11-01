// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.TerminalModes
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Common
{
  public enum TerminalModes : byte
  {
    TTY_OP_END = 0,
    VINTR = 1,
    VQUIT = 2,
    VERASE = 3,
    VKILL = 4,
    VEOF = 5,
    VEOL = 6,
    VEOL2 = 7,
    VSTART = 8,
    VSTOP = 9,
    VSUSP = 10, // 0x0A
    VDSUSP = 11, // 0x0B
    VREPRINT = 12, // 0x0C
    VWERASE = 13, // 0x0D
    VLNEXT = 14, // 0x0E
    VFLUSH = 15, // 0x0F
    VSWTCH = 16, // 0x10
    VSTATUS = 17, // 0x11
    VDISCARD = 18, // 0x12
    IGNPAR = 30, // 0x1E
    PARMRK = 31, // 0x1F
    INPCK = 32, // 0x20
    ISTRIP = 33, // 0x21
    INLCR = 34, // 0x22
    IGNCR = 35, // 0x23
    ICRNL = 36, // 0x24
    IUCLC = 37, // 0x25
    IXON = 38, // 0x26
    IXANY = 39, // 0x27
    IXOFF = 40, // 0x28
    IMAXBEL = 41, // 0x29
    IUTF8 = 42, // 0x2A
    ISIG = 50, // 0x32
    ICANON = 51, // 0x33
    XCASE = 52, // 0x34
    ECHO = 53, // 0x35
    ECHOE = 54, // 0x36
    ECHOK = 55, // 0x37
    ECHONL = 56, // 0x38
    NOFLSH = 57, // 0x39
    TOSTOP = 58, // 0x3A
    IEXTEN = 59, // 0x3B
    ECHOCTL = 60, // 0x3C
    ECHOKE = 61, // 0x3D
    PENDIN = 62, // 0x3E
    OPOST = 70, // 0x46
    OLCUC = 71, // 0x47
    ONLCR = 72, // 0x48
    OCRNL = 73, // 0x49
    ONOCR = 74, // 0x4A
    ONLRET = 75, // 0x4B
    CS7 = 90, // 0x5A
    CS8 = 91, // 0x5B
    PARENB = 92, // 0x5C
    PARODD = 93, // 0x5D
    TTY_OP_ISPEED = 128, // 0x80
    TTY_OP_OSPEED = 129, // 0x81
  }
}
