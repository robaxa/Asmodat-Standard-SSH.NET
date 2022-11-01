// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpResponseFactory
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Sftp.Responses;
using System;
using System.Globalization;
using System.Text;

namespace Renci.SshNet.Sftp
{
  internal sealed class SftpResponseFactory : ISftpResponseFactory
  {
    public SftpMessage Create(uint protocolVersion, byte messageType, Encoding encoding)
    {
      SftpMessageTypes sftpMessageTypes = (SftpMessageTypes) messageType;
      SftpMessage sftpMessage;
      switch (sftpMessageTypes)
      {
        case SftpMessageTypes.Version:
          sftpMessage = (SftpMessage) new SftpVersionResponse();
          break;
        case SftpMessageTypes.Status:
          sftpMessage = (SftpMessage) new SftpStatusResponse(protocolVersion);
          break;
        case SftpMessageTypes.Handle:
          sftpMessage = (SftpMessage) new SftpHandleResponse(protocolVersion);
          break;
        case SftpMessageTypes.Data:
          sftpMessage = (SftpMessage) new SftpDataResponse(protocolVersion);
          break;
        case SftpMessageTypes.Name:
          sftpMessage = (SftpMessage) new SftpNameResponse(protocolVersion, encoding);
          break;
        case SftpMessageTypes.Attrs:
          sftpMessage = (SftpMessage) new SftpAttrsResponse(protocolVersion);
          break;
        case SftpMessageTypes.ExtendedReply:
          sftpMessage = (SftpMessage) new SftpExtendedReplyResponse(protocolVersion);
          break;
        default:
          throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Message type '{0}' is not supported.", (object) sftpMessageTypes));
      }
      return sftpMessage;
    }
  }
}
