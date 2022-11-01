// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ForwardedPortDynamic
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Renci.SshNet
{
  public class ForwardedPortDynamic : ForwardedPort
  {
    private ForwardedPortStatus _status;
    private bool _isDisposed;
    private Socket _listener;
    private CountdownEvent _pendingChannelCountdown;

    public string BoundHost { get; private set; }

    public uint BoundPort { get; private set; }

    public override bool IsStarted => this._status == ForwardedPortStatus.Started;

    public ForwardedPortDynamic(uint port)
      : this(string.Empty, port)
    {
    }

    public ForwardedPortDynamic(string host, uint port)
    {
      this.BoundHost = host;
      this.BoundPort = port;
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
      this.InitializePendingChannelCountdown();
      IPAddress address = IPAddress.Any;
      if (!string.IsNullOrEmpty(this.BoundHost))
        address = DnsAbstraction.GetHostAddresses(this.BoundHost)[0];
      IPEndPoint localEP = new IPEndPoint(address, (int) this.BoundPort);
      this._listener = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
      {
        NoDelay = true
      };
      this._listener.Bind((EndPoint) localEP);
      this._listener.Listen(5);
      this.Session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      this.Session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
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

    ~ForwardedPortDynamic() => this.Dispose(false);

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
        ForwardedPortDynamic.CloseClientSocket(acceptSocket);
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
        ForwardedPortDynamic.CloseClientSocket(clientSocket);
      }
      else
      {
        CountdownEvent channelCountdown = this._pendingChannelCountdown;
        channelCountdown.AddCount();
        try
        {
          using (IChannelDirectTcpip channelDirectTcpip = this.Session.CreateChannelDirectTcpip())
          {
            channelDirectTcpip.Exception += new EventHandler<ExceptionEventArgs>(this.Channel_Exception);
            if (!this.HandleSocks(channelDirectTcpip, clientSocket, this.Session.ConnectionInfo.Timeout))
              ForwardedPortDynamic.CloseClientSocket(clientSocket);
            else
              channelDirectTcpip.Bind();
          }
        }
        catch (Exception ex)
        {
          this.RaiseExceptionEvent(ex);
          ForwardedPortDynamic.CloseClientSocket(clientSocket);
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

    private bool HandleSocks(IChannelDirectTcpip channel, Socket clientSocket, TimeSpan timeout)
    {
      EventHandler eventHandler = (EventHandler) ((_, args) => ForwardedPortDynamic.CloseClientSocket(clientSocket));
      this.Closing += eventHandler;
      try
      {
        int num = SocketAbstraction.ReadByte(clientSocket, timeout);
        switch (num)
        {
          case -1:
            return false;
          case 4:
            return this.HandleSocks4(clientSocket, channel, timeout);
          case 5:
            return this.HandleSocks5(clientSocket, channel, timeout);
          default:
            throw new NotSupportedException(string.Format("SOCKS version {0} is not supported.", (object) num));
        }
      }
      catch (SocketException ex)
      {
        if (ex.SocketErrorCode != SocketError.Interrupted)
          this.RaiseExceptionEvent((Exception) ex);
        return false;
      }
      finally
      {
        this.Closing -= eventHandler;
      }
    }

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

    private bool HandleSocks4(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
    {
      if (SocketAbstraction.ReadByte(socket, timeout) == -1)
        return false;
      byte[] numArray1 = new byte[2];
      if (SocketAbstraction.Read(socket, numArray1, 0, numArray1.Length, timeout) == 0)
        return false;
      ushort uint16 = Pack.BigEndianToUInt16(numArray1);
      byte[] numArray2 = new byte[4];
      if (SocketAbstraction.Read(socket, numArray2, 0, numArray2.Length, timeout) == 0)
        return false;
      IPAddress ipAddress = new IPAddress(numArray2);
      if (ForwardedPortDynamic.ReadString(socket, timeout) == null)
        return false;
      string str = ipAddress.ToString();
      this.RaiseRequestReceived(str, (uint) uint16);
      channel.Open(str, (uint) uint16, (IForwardedPort) this, socket);
      SocketAbstraction.SendByte(socket, (byte) 0);
      if (channel.IsOpen)
      {
        SocketAbstraction.SendByte(socket, (byte) 90);
        SocketAbstraction.Send(socket, numArray1, 0, numArray1.Length);
        SocketAbstraction.Send(socket, numArray2, 0, numArray2.Length);
        return true;
      }
      SocketAbstraction.SendByte(socket, (byte) 91);
      return false;
    }

    private bool HandleSocks5(Socket socket, IChannelDirectTcpip channel, TimeSpan timeout)
    {
      int length = SocketAbstraction.ReadByte(socket, timeout);
      if (length == -1)
        return false;
      byte[] numArray = new byte[length];
      if (SocketAbstraction.Read(socket, numArray, 0, numArray.Length, timeout) == 0)
        return false;
      if (((IEnumerable<byte>) numArray).Min<byte>() == (byte) 0)
        SocketAbstraction.Send(socket, new byte[2]
        {
          (byte) 5,
          (byte) 0
        }, 0, 2);
      else
        SocketAbstraction.Send(socket, new byte[2]
        {
          (byte) 5,
          byte.MaxValue
        }, 0, 2);
      switch (SocketAbstraction.ReadByte(socket, timeout))
      {
        case -1:
          return false;
        case 5:
          if (SocketAbstraction.ReadByte(socket, timeout) == -1)
            return false;
          int num = SocketAbstraction.ReadByte(socket, timeout);
          if (num == -1)
            return false;
          if (num != 0)
            throw new ProxyException("SOCKS5: 0 is expected for reserved byte.");
          int addressType = SocketAbstraction.ReadByte(socket, timeout);
          if (addressType == -1)
            return false;
          string socks5Host = ForwardedPortDynamic.GetSocks5Host(addressType, socket, timeout);
          if (socks5Host == null)
            return false;
          byte[] buffer = new byte[2];
          if (SocketAbstraction.Read(socket, buffer, 0, buffer.Length, timeout) == 0)
            return false;
          ushort uint16 = Pack.BigEndianToUInt16(buffer);
          this.RaiseRequestReceived(socks5Host, (uint) uint16);
          channel.Open(socks5Host, (uint) uint16, (IForwardedPort) this, socket);
          byte[] socks5Reply = ForwardedPortDynamic.CreateSocks5Reply(channel.IsOpen);
          SocketAbstraction.Send(socket, socks5Reply, 0, socks5Reply.Length);
          return true;
        default:
          throw new ProxyException("SOCKS5: Version 5 is expected.");
      }
    }

    private static string GetSocks5Host(int addressType, Socket socket, TimeSpan timeout)
    {
      switch (addressType)
      {
        case 1:
          byte[] numArray1 = new byte[4];
          return SocketAbstraction.Read(socket, numArray1, 0, 4, timeout) == 0 ? (string) null : new IPAddress(numArray1).ToString();
        case 3:
          int length = SocketAbstraction.ReadByte(socket, timeout);
          if (length == -1)
            return (string) null;
          byte[] numArray2 = new byte[length];
          return SocketAbstraction.Read(socket, numArray2, 0, numArray2.Length, timeout) == 0 ? (string) null : SshData.Ascii.GetString(numArray2, 0, numArray2.Length);
        case 4:
          byte[] numArray3 = new byte[16];
          return SocketAbstraction.Read(socket, numArray3, 0, 16, timeout) == 0 ? (string) null : new IPAddress(numArray3).ToString();
        default:
          throw new ProxyException(string.Format("SOCKS5: Address type '{0}' is not supported.", (object) addressType));
      }
    }

    private static byte[] CreateSocks5Reply(bool channelOpen)
    {
      byte[] socks5Reply = new byte[10];
      socks5Reply[0] = (byte) 5;
      socks5Reply[1] = !channelOpen ? (byte) 1 : (byte) 0;
      socks5Reply[2] = (byte) 0;
      socks5Reply[3] = (byte) 1;
      return socks5Reply;
    }

    private static string ReadString(Socket socket, TimeSpan timeout)
    {
      StringBuilder stringBuilder = new StringBuilder();
      byte[] buffer = new byte[1];
      while (true)
      {
        if (SocketAbstraction.Read(socket, buffer, 0, 1, timeout) != 0)
        {
          byte num = buffer[0];
          if (num != (byte) 0)
          {
            char ch = (char) num;
            stringBuilder.Append(ch);
          }
          else
            goto label_5;
        }
        else
          break;
      }
      return (string) null;
label_5:
      return stringBuilder.ToString();
    }
  }
}
