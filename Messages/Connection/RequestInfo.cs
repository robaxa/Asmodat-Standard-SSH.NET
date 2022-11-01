// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.RequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  public abstract class RequestInfo : SshData
  {
    public abstract string RequestName { get; }

    public bool WantReply { get; protected set; }

    protected override int BufferCapacity => base.BufferCapacity + 1;

    protected override void LoadData() => this.WantReply = this.ReadBoolean();

    protected override void SaveData() => this.Write(this.WantReply);
  }
}
