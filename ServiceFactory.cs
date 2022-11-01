// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ServiceFactory
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.NetConf;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet
{
  internal class ServiceFactory : IServiceFactory
  {
    private static int PartialSuccessLimit = 5;

    public IClientAuthentication CreateClientAuthentication() => (IClientAuthentication) new ClientAuthentication(ServiceFactory.PartialSuccessLimit);

    public ISession CreateSession(ConnectionInfo connectionInfo) => (ISession) new Session(connectionInfo, (IServiceFactory) this);

    public ISftpSession CreateSftpSession(
      ISession session,
      int operationTimeout,
      Encoding encoding,
      ISftpResponseFactory sftpMessageFactory)
    {
      return (ISftpSession) new SftpSession(session, operationTimeout, encoding, sftpMessageFactory);
    }

    public PipeStream CreatePipeStream() => new PipeStream();

    public IKeyExchange CreateKeyExchange(
      IDictionary<string, Type> clientAlgorithms,
      string[] serverAlgorithms)
    {
      if (clientAlgorithms == null)
        throw new ArgumentNullException(nameof (clientAlgorithms));
      if (serverAlgorithms == null)
        throw new ArgumentNullException(nameof (serverAlgorithms));
      Type type = clientAlgorithms.SelectMany((Func<KeyValuePair<string, Type>, IEnumerable<string>>) (c => (IEnumerable<string>) serverAlgorithms), (c, s) => new
      {
        c = c,
        s = s
      }).Where(_param1 => _param1.s == _param1.c.Key).Select(_param1 => _param1.c.Value).FirstOrDefault<Type>();
      return !(type == (Type) null) ? type.CreateInstance<IKeyExchange>() : throw new SshConnectionException("Failed to negotiate key exchange algorithm.", DisconnectReason.KeyExchangeFailed);
    }

    public ISftpFileReader CreateSftpFileReader(
      string fileName,
      ISftpSession sftpSession,
      uint bufferSize)
    {
      SftpOpenAsyncResult asyncResult1 = sftpSession.BeginOpen(fileName, Flags.Read, (AsyncCallback) null, (object) null);
      byte[] handle = sftpSession.EndOpen(asyncResult1);
      SFtpStatAsyncResult asyncResult2 = sftpSession.BeginLStat(fileName, (AsyncCallback) null, (object) null);
      uint optimalReadLength = sftpSession.CalculateOptimalReadLength(bufferSize);
      long? fileSize;
      int maxPendingReads;
      try
      {
        SftpFileAttributes sftpFileAttributes = sftpSession.EndLStat(asyncResult2);
        fileSize = new long?(sftpFileAttributes.Size);
        maxPendingReads = Math.Min(10, (int) Math.Ceiling((double) sftpFileAttributes.Size / (double) optimalReadLength) + 1);
      }
      catch (SshException ex)
      {
        fileSize = new long?();
        maxPendingReads = 3;
        DiagnosticAbstraction.Log(string.Format("Failed to obtain size of file. Allowing maximum {0} pending reads: {1}", (object) maxPendingReads, (object) ex));
      }
      return sftpSession.CreateFileReader(handle, sftpSession, optimalReadLength, maxPendingReads, fileSize);
    }

    public ISftpResponseFactory CreateSftpResponseFactory() => (ISftpResponseFactory) new SftpResponseFactory();

    public ShellStream CreateShellStream(
      ISession session,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModeValues,
      int bufferSize)
    {
      return new ShellStream(session, terminalName, columns, rows, width, height, terminalModeValues, bufferSize);
    }

    public IRemotePathTransformation CreateRemotePathDoubleQuoteTransformation() => RemotePathTransformation.DoubleQuote;

    public INetConfSession CreateNetConfSession(
      ISession session,
      int operationTimeout)
    {
      return (INetConfSession) new NetConfSession(session, operationTimeout);
    }
  }
}
