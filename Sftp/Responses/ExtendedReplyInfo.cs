﻿// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.ExtendedReplyInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp.Responses
{
  internal abstract class ExtendedReplyInfo
  {
    public abstract void LoadData(SshDataStream stream);
  }
}