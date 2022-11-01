// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.IServiceFactory
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.NetConf;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet
{
  internal interface IServiceFactory
  {
    IClientAuthentication CreateClientAuthentication();

    ISession CreateSession(ConnectionInfo connectionInfo);

    ISftpSession CreateSftpSession(
      ISession session,
      int operationTimeout,
      Encoding encoding,
      ISftpResponseFactory sftpMessageFactory);

    PipeStream CreatePipeStream();

    IKeyExchange CreateKeyExchange(
      IDictionary<string, Type> clientAlgorithms,
      string[] serverAlgorithms);

    ISftpFileReader CreateSftpFileReader(
      string fileName,
      ISftpSession sftpSession,
      uint bufferSize);

    ISftpResponseFactory CreateSftpResponseFactory();

    ShellStream CreateShellStream(
      ISession session,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModeValues,
      int bufferSize);

    IRemotePathTransformation CreateRemotePathDoubleQuoteTransformation();

    INetConfSession CreateNetConfSession(ISession session, int operationTimeout);
  }
}
