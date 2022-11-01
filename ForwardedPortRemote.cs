// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ForwardedPortRemote
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System;
using System.Globalization;
using System.Net;
using System.Threading;

namespace Renci.SshNet
{
  public class ForwardedPortRemote : ForwardedPort, IDisposable
  {
    private ForwardedPortStatus _status;
    private bool _requestStatus;
    private EventWaitHandle _globalRequestResponse = (EventWaitHandle) new AutoResetEvent(false);
    private CountdownEvent _pendingChannelCountdown;
    private bool _isDisposed;

    public override bool IsStarted => this._status == ForwardedPortStatus.Started;

    public IPAddress BoundHostAddress { get; private set; }

    public string BoundHost => this.BoundHostAddress.ToString();

    public uint BoundPort { get; private set; }

    public IPAddress HostAddress { get; private set; }

    public string Host => this.HostAddress.ToString();

    public uint Port { get; private set; }

    public ForwardedPortRemote(
      IPAddress boundHostAddress,
      uint boundPort,
      IPAddress hostAddress,
      uint port)
    {
      if (boundHostAddress == null)
        throw new ArgumentNullException(nameof (boundHostAddress));
      if (hostAddress == null)
        throw new ArgumentNullException(nameof (hostAddress));
      boundPort.ValidatePort(nameof (boundPort));
      port.ValidatePort(nameof (port));
      this.BoundHostAddress = boundHostAddress;
      this.BoundPort = boundPort;
      this.HostAddress = hostAddress;
      this.Port = port;
      this._status = ForwardedPortStatus.Stopped;
    }

    public ForwardedPortRemote(uint boundPort, string host, uint port)
      : this(string.Empty, boundPort, host, port)
    {
    }

    public ForwardedPortRemote(string boundHost, uint boundPort, string host, uint port)
      : this(DnsAbstraction.GetHostAddresses(boundHost)[0], boundPort, DnsAbstraction.GetHostAddresses(host)[0], port)
    {
    }

    protected override void StartPort()
    {
      if (!ForwardedPortStatus.ToStarting(ref this._status))
        return;
      this.InitializePendingChannelCountdown();
      try
      {
        this.Session.RegisterMessage("SSH_MSG_REQUEST_FAILURE");
        this.Session.RegisterMessage("SSH_MSG_REQUEST_SUCCESS");
        this.Session.RegisterMessage("SSH_MSG_CHANNEL_OPEN");
        this.Session.RequestSuccessReceived += new EventHandler<MessageEventArgs<RequestSuccessMessage>>(this.Session_RequestSuccess);
        this.Session.RequestFailureReceived += new EventHandler<MessageEventArgs<RequestFailureMessage>>(this.Session_RequestFailure);
        this.Session.ChannelOpenReceived += new EventHandler<MessageEventArgs<ChannelOpenMessage>>(this.Session_ChannelOpening);
        this.Session.SendMessage((Message) new TcpIpForwardGlobalRequestMessage(this.BoundHost, this.BoundPort));
        this.Session.WaitOnHandle((WaitHandle) this._globalRequestResponse);
        if (!this._requestStatus)
          throw new SshException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Port forwarding for '{0}' port '{1}' failed to start.", (object) this.Host, (object) this.Port));
      }
      catch (Exception ex)
      {
        this._status = ForwardedPortStatus.Stopped;
        this.Session.RequestSuccessReceived -= new EventHandler<MessageEventArgs<RequestSuccessMessage>>(this.Session_RequestSuccess);
        this.Session.RequestFailureReceived -= new EventHandler<MessageEventArgs<RequestFailureMessage>>(this.Session_RequestFailure);
        this.Session.ChannelOpenReceived -= new EventHandler<MessageEventArgs<ChannelOpenMessage>>(this.Session_ChannelOpening);
        throw;
      }
      this._status = ForwardedPortStatus.Started;
    }

    protected override void StopPort(TimeSpan timeout)
    {
      if (!ForwardedPortStatus.ToStopping(ref this._status))
        return;
      base.StopPort(timeout);
      this.Session.SendMessage((Message) new CancelTcpIpForwardGlobalRequestMessage(this.BoundHost, this.BoundPort));
      WaitHandle.WaitAny(new WaitHandle[2]
      {
        (WaitHandle) this._globalRequestResponse,
        this.Session.MessageListenerCompleted
      }, timeout);
      this.Session.RequestSuccessReceived -= new EventHandler<MessageEventArgs<RequestSuccessMessage>>(this.Session_RequestSuccess);
      this.Session.RequestFailureReceived -= new EventHandler<MessageEventArgs<RequestFailureMessage>>(this.Session_RequestFailure);
      this.Session.ChannelOpenReceived -= new EventHandler<MessageEventArgs<ChannelOpenMessage>>(this.Session_ChannelOpening);
      this._pendingChannelCountdown.Signal();
      this._pendingChannelCountdown.Wait(timeout);
      this._status = ForwardedPortStatus.Stopped;
    }

    protected override void CheckDisposed()
    {
      if (this._isDisposed)
        throw new ObjectDisposedException(this.GetType().FullName);
    }

    private void Session_ChannelOpening(object sender, MessageEventArgs<ChannelOpenMessage> e)
    {
      ChannelOpenMessage channelOpenMessage = e.Message;
      ForwardedTcpipChannelInfo info = channelOpenMessage.Info as ForwardedTcpipChannelInfo;
      if (info == null || !(info.ConnectedAddress == this.BoundHost) || (int) info.ConnectedPort != (int) this.BoundPort)
        return;
      if (!this.IsStarted)
        this.Session.SendMessage((Message) new ChannelOpenFailureMessage(channelOpenMessage.LocalChannelNumber, "", 1U));
      else
        ThreadAbstraction.ExecuteThread((Action) (() =>
        {
          CountdownEvent channelCountdown = this._pendingChannelCountdown;
          channelCountdown.AddCount();
          try
          {
            this.RaiseRequestReceived(info.OriginatorAddress, info.OriginatorPort);
            using (IChannelForwardedTcpip channelForwardedTcpip = this.Session.CreateChannelForwardedTcpip(channelOpenMessage.LocalChannelNumber, channelOpenMessage.InitialWindowSize, channelOpenMessage.MaximumPacketSize))
            {
              channelForwardedTcpip.Exception += new EventHandler<ExceptionEventArgs>(this.Channel_Exception);
              channelForwardedTcpip.Bind(new IPEndPoint(this.HostAddress, (int) this.Port), (IForwardedPort) this);
            }
          }
          catch (Exception ex)
          {
            this.RaiseExceptionEvent(ex);
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
        }));
    }

    private void InitializePendingChannelCountdown() => Interlocked.Exchange<CountdownEvent>(ref this._pendingChannelCountdown, new CountdownEvent(1))?.Dispose();

    private void Channel_Exception(object sender, ExceptionEventArgs exceptionEventArgs) => this.RaiseExceptionEvent(exceptionEventArgs.Exception);

    private void Session_RequestFailure(object sender, EventArgs e)
    {
      this._requestStatus = false;
      this._globalRequestResponse.Set();
    }

    private void Session_RequestSuccess(object sender, MessageEventArgs<RequestSuccessMessage> e)
    {
      this._requestStatus = true;
      if (this.BoundPort == 0U)
        this.BoundPort = !e.Message.BoundPort.HasValue ? 0U : e.Message.BoundPort.Value;
      this._globalRequestResponse.Set();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected override void Dispose(bool disposing)
    {
      if (this._isDisposed)
        return;
      base.Dispose(disposing);
      if (disposing)
      {
        ISession session = this.Session;
        if (session != null)
        {
          this.Session = (ISession) null;
          session.RequestSuccessReceived -= new EventHandler<MessageEventArgs<RequestSuccessMessage>>(this.Session_RequestSuccess);
          session.RequestFailureReceived -= new EventHandler<MessageEventArgs<RequestFailureMessage>>(this.Session_RequestFailure);
          session.ChannelOpenReceived -= new EventHandler<MessageEventArgs<ChannelOpenMessage>>(this.Session_ChannelOpening);
        }
        EventWaitHandle globalRequestResponse = this._globalRequestResponse;
        if (globalRequestResponse != null)
        {
          this._globalRequestResponse = (EventWaitHandle) null;
          globalRequestResponse.Dispose();
        }
        CountdownEvent channelCountdown = this._pendingChannelCountdown;
        if (channelCountdown != null)
        {
          this._pendingChannelCountdown = (CountdownEvent) null;
          channelCountdown.Dispose();
        }
      }
      this._isDisposed = true;
    }

    ~ForwardedPortRemote() => this.Dispose(false);
  }
}
