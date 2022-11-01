// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.SshConnectionException
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Messages.Transport;
using System;
using System.Runtime.Serialization;

namespace Renci.SshNet.Common
{
  [Serializable]
  public class SshConnectionException : SshException
  {
    public DisconnectReason DisconnectReason { get; private set; }

    public SshConnectionException()
    {
    }

    public SshConnectionException(string message)
      : base(message)
    {
      this.DisconnectReason = DisconnectReason.None;
    }

    public SshConnectionException(string message, DisconnectReason disconnectReasonCode)
      : base(message)
    {
      this.DisconnectReason = disconnectReasonCode;
    }

    public SshConnectionException(
      string message,
      DisconnectReason disconnectReasonCode,
      Exception inner)
      : base(message, inner)
    {
      this.DisconnectReason = disconnectReasonCode;
    }

    protected SshConnectionException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
