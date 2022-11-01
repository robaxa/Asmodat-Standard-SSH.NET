// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ForwardedPortLocal
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet
{
  public class ForwardedPortLocal : ForwardedPort, IDisposable
  {
    private ForwardedPortStatus _status;
    private bool _isDisposed;
    private Socket _listener;
    private CountdownEvent _pendingChannelCountdown;

    public string BoundHost { get; private set; }

    public uint BoundPort { get; private set; }

    public string Host { get; private set; }

    public uint Port { get; private set; }

    public override bool IsStarted => this._status == ForwardedPortStatus.Started;

    public ForwardedPortLocal(uint boundPort, string host, uint port)
      : this(string.Empty, boundPort, host, port)
    {
    }

    public ForwardedPortLocal(string boundHost, string host, uint port)
      : this(boundHost, 0U, host, port)
    {
    }

    public ForwardedPortLocal(string boundHost, uint boundPort, string host, uint port)
    {
      if (boundHost == null)
        throw new ArgumentNullException(nameof (boundHost));
      if (host == null)
        throw new ArgumentNullException(nameof (host));
      boundPort.ValidatePort(nameof (boundPort));
      port.ValidatePort(nameof (port));
      this.BoundHost = boundHost;
      this.BoundPort = boundPort;
      this.Host = host;
      this.Port = port;
      this._status = ForwardedPortStatus.Stopped;
    }

    protected override void StartPort()
    {
      if (!ForwardedPortStatus.ToStarting(ref this._status))
        return;
      try
      {
        this.InternalStart();
      }
      catch (Exception ex)
      {
        this._status = ForwardedPortStatus.Stopped;
        throw;
      }
    }

    protected override void StopPort(TimeSpan timeout)
    {
      if (!ForwardedPortStatus.ToStopping(ref this._status))
        return;
      base.StopPort(timeout);
      this.StopListener();
      this.InternalStop(timeout);
      this._status = ForwardedPortStatus.Stopped;
    }

    protected override void CheckDisposed()
    {
      if (this._isDisposed)
        throw new ObjectDisposedException(this.GetType().FullName);
    }

    private void InternalStart()
    {
      IPEndPoint localEP = new IPEndPoint(DnsAbstraction.GetHostAddresses(this.BoundHost)[0], (int) this.BoundPort);
      this._listener = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
      {
        NoDelay = true
      };
      this._listener.Bind((EndPoint) localEP);
      this._listener.Listen(5);
      this.BoundPort = (uint) ((IPEndPoint) this._listener.LocalEndPoint).Port;
      this.Session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      this.Session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
      this.InitializePendingChannelCountdown();
      this._status = ForwardedPortStatus.Started;
      this.StartAccept((SocketAsyncEventArgs) null);
    }

    private void StopListener()
    {
      this._listener?.Dispose();
      ISession session = this.Session;
      if (session == null)
        return;
      session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      session.Disconnected -= new EventHandler<EventArgs>(this.Session_Disconnected);
    }

    private void InternalStop(TimeSpan timeout)
    {
      this._pendingChannelCountdown.Signal();
      this._pendingChannelCountdown.Wait(timeout);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void InternalDispose(bool disposing)
    {
      if (!disposing)
        return;
      Socket listener = this._listener;
      if (listener != null)
      {
        this._listener = (Socket) null;
        listener.Dispose();
      }
      CountdownEvent channelCountdown = this._pendingChannelCountdown;
      if (channelCountdown != null)
      {
        this._pendingChannelCountdown = (CountdownEvent) null;
        channelCountdown.Dispose();
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (this._isDisposed)
        return;
      base.Dispose(disposing);
      this.InternalDispose(disposing);
      this._isDisposed = true;
    }

    ~ForwardedPortLocal() => this.Dispose(false);

    private void StartAccept(SocketAsyncEventArgs e)
    {
      if (e == null)
      {
        e = new SocketAsyncEventArgs();
        e.Completed += new EventHandler<SocketAsyncEventArgs>(this.AcceptCompleted);
      }
      else
        e.AcceptSocket = (Socket) null;
      if (!this.IsStarted)
        return;
      try
      {
        if (!this._listener.AcceptAsync(e))
          this.AcceptCompleted((object) null, e);
      }
      catch (ObjectDisposedException ex)
      {
        if (this._status == ForwardedPortStatus.Stopped || this._status == ForwardedPortStatus.Stopped)
          return;
        throw;
      }
    }

    private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
      if (e.SocketError == SocketError.OperationAborted || e.SocketError == SocketError.NotSocket)
        return;
      Socket acceptSocket = e.AcceptSocket;
      if (e.SocketError != 0)
      {
        this.StartAccept(e);
        ForwardedPortLocal.CloseClientSocket(acceptSocket);
      }
      else
      {
        this.StartAccept(e);
        this.ProcessAccept(acceptSocket);
      }
    }

    private void ProcessAccept(Socket clientSocket)
    {
      if (!this.IsStarted)
      {
        ForwardedPortLocal.CloseClientSocket(clientSocket);
      }
      else
      {
        CountdownEvent channelCountdown = this._pendingChannelCountdown;
        channelCountdown.AddCount();
        try
        {
          IPEndPoint remoteEndPoint = (IPEndPoint) clientSocket.RemoteEndPoint;
          this.RaiseRequestReceived(remoteEndPoint.Address.ToString(), (uint) remoteEndPoint.Port);
          using (IChannelDirectTcpip channelDirectTcpip = this.Session.CreateChannelDirectTcpip())
          {
            channelDirectTcpip.Exception += new EventHandler<ExceptionEventArgs>(this.Channel_Exception);
            channelDirectTcpip.Open(this.Host, this.Port, (IForwardedPort) this, clientSocket);
            channelDirectTcpip.Bind();
          }
        }
        catch (Exception ex)
        {
          this.RaiseExceptionEvent(ex);
          ForwardedPortLocal.CloseClientSocket(clientSocket);
        }
        finally
        {
          try
          {
            channelCountdown.Signal();
          }
          catch (ObjectDisposedException ex)
          {
          }
        }
      }
    }

    private void InitializePendingChannelCountdown() => Interlocked.Exchange<CountdownEvent>(ref this._pendingChannelCountdown, new CountdownEvent(1))?.Dispose();

    private static void CloseClientSocket(Socket clientSocket)
    {
      if (clientSocket.Connected)
      {
        try
        {
          clientSocket.Shutdown(SocketShutdown.Send);
        }
        catch (Exception ex)
        {
        }
      }
      clientSocket.Dispose();
    }

    private void Session_Disconnected(object sender, EventArgs e)
    {
      ISession session = this.Session;
      if (session == null)
        return;
      this.StopPort(session.ConnectionInfo.Timeout);
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
    {
      ISession session = this.Session;
      if (session == null)
        return;
      this.StopPort(session.ConnectionInfo.Timeout);
    }

    private void Channel_Exception(object sender, ExceptionEventArgs e) => this.RaiseExceptionEvent(e.Exception);
  }
}
