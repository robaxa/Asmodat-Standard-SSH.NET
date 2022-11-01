// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.RequestSuccessMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Connection
{
  [Message("SSH_MSG_REQUEST_SUCCESS", 81)]
  public class RequestSuccessMessage : Message
  {
    public uint? BoundPort { get; private set; }

    protected override int BufferCapacity
    {
      get
      {
        int bufferCapacity = base.BufferCapacity;
        if (this.BoundPort.HasValue)
          bufferCapacity += 4;
        return bufferCapacity;
      }
    }

    public RequestSuccessMessage()
    {
    }

    public RequestSuccessMessage(uint boundPort) => this.BoundPort = new uint?(boundPort);

    protected override void LoadData()
    {
      if (this.IsEndOfData)
        return;
      this.BoundPort = new uint?(this.ReadUInt32());
    }

    protected override void SaveData()
    {
      if (!this.BoundPort.HasValue)
        return;
      this.Write(this.BoundPort.Value);
    }

    internal override void Process(Session session) => session.OnRequestSuccessReceived(this);
  }
}
