// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.ChannelDirectTcpip
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Channels
{
  internal class ChannelDirectTcpip : ClientChannel, IChannelDirectTcpip, IDisposable
  {
    private readonly object _socketLock = new object();
    private EventWaitHandle _channelOpen = (EventWaitHandle) new AutoResetEvent(false);
    private EventWaitHandle _channelData = (EventWaitHandle) new AutoResetEvent(false);
    private IForwardedPort _forwardedPort;
    private Socket _socket;

    public ChannelDirectTcpip(
      ISession session,
      uint localChannelNumber,
      uint localWindowSize,
      uint localPacketSize)
      : base(session, localChannelNumber, localWindowSize, localPacketSize)
    {
    }

    public override ChannelTypes ChannelType => ChannelTypes.DirectTcpip;

    public void Open(string remoteHost, uint port, IForwardedPort forwardedPort, Socket socket)
    {
      if (this.IsOpen)
        throw new SshException("Channel is already open.");
      if (!this.IsConnected)
        throw new SshException("Session is not connected.");
      this._socket = socket;
      this._forwardedPort = forwardedPort;
      this._forwardedPort.Closing += new EventHandler(this.ForwardedPort_Closing);
      IPEndPoint remoteEndPoint = (IPEndPoint) socket.RemoteEndPoint;
      this.SendMessage(new ChannelOpenMessage(this.LocalChannelNumber, this.LocalWindowSize, this.LocalPacketSize, (ChannelOpenInfo) new DirectTcpipChannelInfo(remoteHost, port, remoteEndPoint.Address.ToString(), (uint) remoteEndPoint.Port)));
      this.WaitOnHandle((WaitHandle) this._channelOpen);
    }

    private void ForwardedPort_Closing(object sender, EventArgs eventArgs)
    {
      this.ShutdownSocket(SocketShutdown.Send);
      this.CloseSocket();
    }

    public void Bind()
    {
      if (!this.IsOpen)
        return;
      byte[] buffer = new byte[(int) this.RemotePacketSize];
      SocketAbstraction.ReadContinuous(this._socket, buffer, 0, buffer.Length, new Action<byte[], int, int>(((Channel) this).SendData));
    }

    private void CloseSocket()
    {
      if (this._socket == null)
        return;
      lock (this._socketLock)
      {
        if (this._socket == null)
          return;
        this._socket.Dispose();
        this._socket = (Socket) null;
      }
    }

    private void ShutdownSocket(SocketShutdown how)
    {
      if (this._socket == null)
        return;
      lock (this._socketLock)
      {
        if (!this._socket.IsConnected())
          return;
        try
        {
          this._socket.Shutdown(how);
        }
        catch (SocketException ex)
        {
          DiagnosticAbstraction.Log("Failure shutting down socket: " + ex?.ToString());
        }
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
      if (this._socket == null)
        return;
      lock (this._socketLock)
      {
        if (this._socket.IsConnected())
          SocketAbstraction.Send(this._socket, data, 0, data.Length);
      }
    }

    protected override void OnOpenConfirmation(
      uint remoteChannelNumber,
      uint initialWindowSize,
      uint maximumPacketSize)
    {
      base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);
      this._channelOpen.Set();
    }

    protected override void OnOpenFailure(uint reasonCode, string description, string language)
    {
      base.OnOpenFailure(reasonCode, description, language);
      this._channelOpen.Set();
    }

    protected override void OnEof()
    {
      base.OnEof();
      this.ShutdownSocket(SocketShutdown.Send);
    }

    protected override void OnErrorOccured(Exception exp)
    {
      base.OnErrorOccured(exp);
      this.ShutdownSocket(SocketShutdown.Send);
    }

    protected override void OnDisconnected()
    {
      base.OnDisconnected();
      this.ShutdownSocket(SocketShutdown.Both);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      if (this._socket != null)
      {
        lock (this._socketLock)
        {
          Socket socket = this._socket;
          if (socket != null)
          {
            this._socket = (Socket) null;
            socket.Dispose();
          }
        }
      }
      EventWaitHandle channelOpen = this._channelOpen;
      if (channelOpen != null)
      {
        this._channelOpen = (EventWaitHandle) null;
        channelOpen.Dispose();
      }
      EventWaitHandle channelData = this._channelData;
      if (channelData != null)
      {
        this._channelData = (EventWaitHandle) null;
        channelData.Dispose();
      }
    }
  }
}
