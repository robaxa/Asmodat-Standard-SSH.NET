// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.GlobalRequestMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_GLOBAL_REQUEST", 80)]
  public class GlobalRequestMessage : Message
  {
    private byte[] _requestName;

    public string RequestName => SshData.Ascii.GetString(this._requestName, 0, this._requestName.Length);

    public bool WantReply { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._requestName.Length + 1;

    public GlobalRequestMessage()
    {
    }

    internal GlobalRequestMessage(byte[] requestName, bool wantReply)
    {
      this._requestName = requestName;
      this.WantReply = wantReply;
    }

    protected override void LoadData()
    {
      this._requestName = this.ReadBinary();
      this.WantReply = this.ReadBoolean();
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this._requestName);
      this.Write(this.WantReply);
    }

    internal override void Process(Session session) => session.OnGlobalRequestReceived(this);
  }
}
