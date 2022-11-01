// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.SshClient
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Renci.SshNet
{
  public class SshClient : BaseClient
  {
    private readonly List<ForwardedPort> _forwardedPorts;
    private bool _isDisposed;
    private Stream _inputStream;

    public IEnumerable<ForwardedPort> ForwardedPorts => (IEnumerable<ForwardedPort>) this._forwardedPorts.AsReadOnly();

    public SshClient(ConnectionInfo connectionInfo)
      : this(connectionInfo, false)
    {
    }

    public SshClient(string host, int port, string username, string password)
      : this((ConnectionInfo) new PasswordConnectionInfo(host, port, username, password), true)
    {
    }

    public SshClient(string host, string username, string password)
      : this(host, ConnectionInfo.DefaultPort, username, password)
    {
    }

    public SshClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
      : this((ConnectionInfo) new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
    {
    }

    public SshClient(string host, string username, params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
    {
    }

    private SshClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
      : this(connectionInfo, ownsConnectionInfo, (IServiceFactory) new ServiceFactory())
    {
    }

    internal SshClient(
      ConnectionInfo connectionInfo,
      bool ownsConnectionInfo,
      IServiceFactory serviceFactory)
      : base(connectionInfo, ownsConnectionInfo, serviceFactory)
    {
      this._forwardedPorts = new List<ForwardedPort>();
    }

    protected override void OnDisconnecting()
    {
      base.OnDisconnecting();
      foreach (ForwardedPort forwardedPort in this._forwardedPorts)
        forwardedPort.Stop();
    }

    public void AddForwardedPort(ForwardedPort port)
    {
      if (port == null)
        throw new ArgumentNullException(nameof (port));
      this.EnsureSessionIsOpen();
      this.AttachForwardedPort(port);
      this._forwardedPorts.Add(port);
    }

    public void RemoveForwardedPort(ForwardedPort port)
    {
      if (port == null)
        throw new ArgumentNullException(nameof (port));
      port.Stop();
      SshClient.DetachForwardedPort(port);
      this._forwardedPorts.Remove(port);
    }

    private void AttachForwardedPort(ForwardedPort port)
    {
      if (port.Session != null && port.Session != this.Session)
        throw new InvalidOperationException("Forwarded port is already added to a different client.");
      port.Session = this.Session;
    }

    private static void DetachForwardedPort(ForwardedPort port) => port.Session = (ISession) null;

    public SshCommand CreateCommand(string commandText) => this.CreateCommand(commandText, this.ConnectionInfo.Encoding);

    public SshCommand CreateCommand(string commandText, Encoding encoding)
    {
      this.EnsureSessionIsOpen();
      this.ConnectionInfo.Encoding = encoding;
      return new SshCommand(this.Session, commandText, encoding);
    }

    public SshCommand RunCommand(string commandText)
    {
      SshCommand command = this.CreateCommand(commandText);
      command.Execute();
      return command;
    }

    public Shell CreateShell(
      Stream input,
      Stream output,
      Stream extendedOutput,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModes,
      int bufferSize)
    {
      this.EnsureSessionIsOpen();
      return new Shell(this.Session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
    }

    public Shell CreateShell(
      Stream input,
      Stream output,
      Stream extendedOutput,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModes)
    {
      return this.CreateShell(input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, 1024);
    }

    public Shell CreateShell(Stream input, Stream output, Stream extendedOutput) => this.CreateShell(input, output, extendedOutput, string.Empty, 0U, 0U, 0U, 0U, (IDictionary<TerminalModes, uint>) null, 1024);

    public Shell CreateShell(
      Encoding encoding,
      string input,
      Stream output,
      Stream extendedOutput,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModes,
      int bufferSize)
    {
      this._inputStream = (Stream) new MemoryStream();
      StreamWriter streamWriter = new StreamWriter(this._inputStream, encoding);
      streamWriter.Write(input);
      streamWriter.Flush();
      this._inputStream.Seek(0L, SeekOrigin.Begin);
      return this.CreateShell(this._inputStream, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
    }

    public Shell CreateShell(
      Encoding encoding,
      string input,
      Stream output,
      Stream extendedOutput,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModes)
    {
      return this.CreateShell(encoding, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, 1024);
    }

    public Shell CreateShell(
      Encoding encoding,
      string input,
      Stream output,
      Stream extendedOutput)
    {
      return this.CreateShell(encoding, input, output, extendedOutput, string.Empty, 0U, 0U, 0U, 0U, (IDictionary<TerminalModes, uint>) null, 1024);
    }

    public ShellStream CreateShellStream(
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      int bufferSize)
    {
      return this.CreateShellStream(terminalName, columns, rows, width, height, bufferSize, (IDictionary<TerminalModes, uint>) null);
    }

    public ShellStream CreateShellStream(
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      int bufferSize,
      IDictionary<TerminalModes, uint> terminalModeValues)
    {
      this.EnsureSessionIsOpen();
      return this.ServiceFactory.CreateShellStream(this.Session, terminalName, columns, rows, width, height, terminalModeValues, bufferSize);
    }

    protected override void OnDisconnected()
    {
      base.OnDisconnected();
      for (int index = this._forwardedPorts.Count - 1; index >= 0; --index)
      {
        SshClient.DetachForwardedPort(this._forwardedPorts[index]);
        this._forwardedPorts.RemoveAt(index);
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this._isDisposed || !disposing)
        return;
      if (this._inputStream != null)
      {
        this._inputStream.Dispose();
        this._inputStream = (Stream) null;
      }
      this._isDisposed = true;
    }

    private void EnsureSessionIsOpen()
    {
      if (this.Session == null)
        throw new SshConnectionException("Client not connected.");
    }
  }
}
