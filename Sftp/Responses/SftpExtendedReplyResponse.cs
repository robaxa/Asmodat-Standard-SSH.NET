// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.Responses.SftpExtendedReplyResponse
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Sftp.Responses
{
  internal class SftpExtendedReplyResponse : SftpResponse
  {
    public override SftpMessageTypes SftpMessageType => SftpMessageTypes.ExtendedReply;

    public SftpExtendedReplyResponse(uint protocolVersion)
      : base(protocolVersion)
    {
    }

    public T GetReply<T>() where T : ExtendedReplyInfo, new()
    {
      T reply = new T();
      reply.LoadData(this.DataStream);
      return reply;
    }
  }
}
