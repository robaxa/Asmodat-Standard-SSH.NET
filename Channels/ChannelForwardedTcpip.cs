// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.ChannelForwardedTcpip
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Channels
{
  internal class ChannelForwardedTcpip : ServerChannel, IChannelForwardedTcpip, IDisposable
  {
    private readonly object _socketShutdownAndCloseLock = new object();
    private Socket _socket;
    private IForwardedPort _forwardedPort;

    internal ChannelForwardedTcpip(
      ISession session,
      uint localChannelNumber,
      uint localWindowSize,
      uint localPacketSize,
      uint remoteChannelNumber,
      uint remoteWindowSize,
      uint remotePacketSize)
      : base(session, localChannelNumber, localWindowSize, localPacketSize, remoteChannelNumber, remoteWindowSize, remotePacketSize)
    {
    }

    public override ChannelTypes ChannelType => ChannelTypes.ForwardedTcpip;

    public void Bind(IPEndPoint remoteEndpoint, IForwardedPort forwardedPort)
    {
      if (!this.IsConnected)
        throw new SshException("Session is not connected.");
      this._forwardedPort = forwardedPort;
      this._forwardedPort.Closing += new EventHandler(this.ForwardedPort_Closing);
      try
      {
        this._socket = SocketAbstraction.Connect(remoteEndpoint, this.ConnectionInfo.Timeout);
        this.SendMessage(new ChannelOpenConfirmationMessage(this.RemoteChannelNumber, this.LocalWindowSize, this.LocalPacketSize, this.LocalChannelNumber));
      }
      catch (Exception ex)
      {
        this.SendMessage((Message) new ChannelOpenFailureMessage(this.RemoteChannelNumber, ex.ToString(), 2U, "en"));
        throw;
      }
      byte[] buffer = new byte[(int) this.RemotePacketSize];
      SocketAbstraction.ReadContinuous(this._socket, buffer, 0, buffer.Length, new Action<byte[], int, int>(((Channel) this).SendData));
    }

    protected override void OnErrorOccured(Exception exp)
    {
      base.OnErrorOccured(exp);
      this.ShutdownSocket(SocketShutdown.Send);
    }

    private void ForwardedPort_Closing(object sender, EventArgs eventArgs) => this.ShutdownSocket(SocketShutdown.Send);

    private void ShutdownSocket(SocketShutdown how)
    {
      if (this._socket == null)
        return;
      lock (this._socketShutdownAndCloseLock)
      {
        Socket socket = this._socket;
        if (!socket.IsConnected())
          return;
        try
        {
          socket.Shutdown(how);
        }
        catch (SocketException ex)
        {
          DiagnosticAbstraction.Log("Failure shutting down socket: " + ex?.ToString());
        }
      }
    }

    private void CloseSocket()
    {
      if (this._socket == null)
        return;
      lock (this._socketShutdownAndCloseLock)
      {
        Socket socket = this._socket;
        if (socket == null)
          return;
        this._socket = (Socket) null;
        socket.Dispose();
      }
    }

    protected override void Close()
    {
      IForwardedPort forwardedPort = this._forwardedPort;
      if (forwardedPort != null)
      {
        forwardedPort.Closing -= new EventHandler(this.ForwardedPort_Closing);
        this._forwardedPort = (IForwardedPort) null;
      }
      this.ShutdownSocket(SocketShutdown.Send);
      base.Close();
      this.CloseSocket();
    }

    protected override void OnData(byte[] data)
    {
      base.OnData(data);
      Socket socket = this._socket;
      if (!socket.IsConnected())
        return;
      SocketAbstraction.Send(socket, data, 0, data.Length);
    }
  }
}
