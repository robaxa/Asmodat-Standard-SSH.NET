﻿// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.SshException
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Runtime.Serialization;

namespace Renci.SshNet.Common
{
  [Serializable]
  public class SshException : Exception
  {
    public SshException()
    {
    }

    public SshException(string message)
      : base(message)
    {
    }

    public SshException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected SshException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
