// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.RequestMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Messages.Authentication
{
  [Message("SSH_MSG_USERAUTH_REQUEST", 50)]
  public abstract class RequestMessage : Message
  {
    internal const int AuthenticationMessageCode = 50;
    private readonly byte[] _serviceName;
    private readonly byte[] _userName;
    private readonly byte[] _methodNameBytes;
    private readonly string _methodName;

    public byte[] Username => this._userName;

    public byte[] ServiceName => this._serviceName;

    public virtual string MethodName => this._methodName;

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Username.Length + 4 + this.ServiceName.Length + 4 + this._methodNameBytes.Length;

    protected RequestMessage(Renci.SshNet.Messages.ServiceName serviceName, string username, string methodName)
    {
      this._serviceName = serviceName.ToArray();
      this._userName = SshData.Utf8.GetBytes(username);
      this._methodNameBytes = SshData.Ascii.GetBytes(methodName);
      this._methodName = methodName;
    }

    protected override void LoadData() => throw new InvalidOperationException("Load data is not supported.");

    protected override void SaveData()
    {
      this.WriteBinaryString(this._userName);
      this.WriteBinaryString(this._serviceName);
      this.WriteBinaryString(this._methodNameBytes);
    }

    internal override void Process(Session session) => throw new NotImplementedException();
  }
}
