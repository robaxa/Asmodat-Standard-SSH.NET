// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.ServiceAcceptMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_SERVICE_ACCEPT", 6)]
  public class ServiceAcceptMessage : Message
  {
    internal const byte MessageNumber = 6;

    public ServiceName ServiceName { get; private set; }

    protected override void LoadData() => this.ServiceName = this.ReadBinary().ToServiceName();

    protected override void SaveData() => throw new NotImplementedException();

    internal override void Process(Session session) => session.OnServiceAcceptReceived(this);
  }
}
