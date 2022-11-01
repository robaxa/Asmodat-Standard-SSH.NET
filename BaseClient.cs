// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.BaseClient
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Transport;
using System;
using System.Threading;

namespace Renci.SshNet
{
  public abstract class BaseClient : IDisposable
  {
    private readonly bool _ownsConnectionInfo;
    private readonly IServiceFactory _serviceFactory;
    private readonly object _keepAliveLock = new object();
    private TimeSpan _keepAliveInterval;
    private Timer _keepAliveTimer;
    private ConnectionInfo _connectionInfo;
    private bool _isDisposed;

    internal ISession Session { get; private set; }

    internal IServiceFactory ServiceFactory => this._serviceFactory;

    public ConnectionInfo ConnectionInfo
    {
      get
      {
        this.CheckDisposed();
        return this._connectionInfo;
      }
      private set => this._connectionInfo = value;
    }

    public bool IsConnected
    {
      get
      {
        this.CheckDisposed();
        return this.IsSessionConnected();
      }
    }

    public TimeSpan KeepAliveInterval
    {
      get
      {
        this.CheckDisposed();
        return this._keepAliveInterval;
      }
      set
      {
        this.CheckDisposed();
        if (value == this._keepAliveInterval)
          return;
        if (value == Renci.SshNet.Session.InfiniteTimeSpan)
          this.StopKeepAliveTimer();
        else if (this._keepAliveTimer != null)
          this._keepAliveTimer.Change(value, value);
        else if (this.IsSessionConnected())
          this._keepAliveTimer = this.CreateKeepAliveTimer(value, value);
        this._keepAliveInterval = value;
      }
    }

    public event EventHandler<ExceptionEventArgs> ErrorOccurred;

    public event EventHandler<HostKeyEventArgs> HostKeyReceived;

    protected BaseClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
      : this(connectionInfo, ownsConnectionInfo, (IServiceFactory) new Renci.SshNet.ServiceFactory())
    {
    }

    internal BaseClient(
      ConnectionInfo connectionInfo,
      bool ownsConnectionInfo,
      IServiceFactory serviceFactory)
    {
      if (connectionInfo == null)
        throw new ArgumentNullException(nameof (connectionInfo));
      if (serviceFactory == null)
        throw new ArgumentNullException(nameof (serviceFactory));
      this.ConnectionInfo = connectionInfo;
      this._ownsConnectionInfo = ownsConnectionInfo;
      this._serviceFactory = serviceFactory;
      this._keepAliveInterval = Renci.SshNet.Session.InfiniteTimeSpan;
    }

    public void Connect()
    {
      this.CheckDisposed();
      if (this.IsSessionConnected())
        throw new InvalidOperationException("The client is already connected.");
      this.OnConnecting();
      this.Session = this.CreateAndConnectSession();
      try
      {
        this.OnConnected();
      }
      catch
      {
        this.DisposeSession();
        throw;
      }
      this.StartKeepAliveTimer();
    }

    public void Disconnect()
    {
      DiagnosticAbstraction.Log("Disconnecting client.");
      this.CheckDisposed();
      this.OnDisconnecting();
      this.StopKeepAliveTimer();
      this.DisposeSession();
      this.OnDisconnected();
    }

    [Obsolete("Use KeepAliveInterval to send a keep-alive message at regular intervals.")]
    public void SendKeepAlive()
    {
      this.CheckDisposed();
      this.SendKeepAliveMessage();
    }

    protected virtual void OnConnecting()
    {
    }

    protected virtual void OnConnected()
    {
    }

    protected virtual void OnDisconnecting() => this.Session?.OnDisconnecting();

    protected virtual void OnDisconnected()
    {
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
    {
      EventHandler<ExceptionEventArgs> errorOccurred = this.ErrorOccurred;
      if (errorOccurred == null)
        return;
      errorOccurred((object) this, e);
    }

    private void Session_HostKeyReceived(object sender, HostKeyEventArgs e)
    {
      EventHandler<HostKeyEventArgs> hostKeyReceived = this.HostKeyReceived;
      if (hostKeyReceived == null)
        return;
      hostKeyReceived((object) this, e);
    }

    public void Dispose()
    {
      DiagnosticAbstraction.Log("Disposing client.");
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._isDisposed || !disposing)
        return;
      this.Disconnect();
      if (this._ownsConnectionInfo && this._connectionInfo != null)
      {
        if (this._connectionInfo is IDisposable connectionInfo)
          connectionInfo.Dispose();
        this._connectionInfo = (ConnectionInfo) null;
      }
      this._isDisposed = true;
    }

    protected void CheckDisposed()
    {
      if (this._isDisposed)
        throw new ObjectDisposedException(this.GetType().FullName);
    }

    ~BaseClient() => this.Dispose(false);

    private void StopKeepAliveTimer()
    {
      if (this._keepAliveTimer == null)
        return;
      this._keepAliveTimer.Dispose();
      this._keepAliveTimer = (Timer) null;
    }

    private void SendKeepAliveMessage()
    {
      ISession session = this.Session;
      if (session == null)
        return;
      if (!Monitor.TryEnter(this._keepAliveLock))
        return;
      try
      {
        session.TrySendMessage((Message) new IgnoreMessage());
      }
      finally
      {
        Monitor.Exit(this._keepAliveLock);
      }
    }

    private void StartKeepAliveTimer()
    {
      if (this._keepAliveInterval == Renci.SshNet.Session.InfiniteTimeSpan || this._keepAliveTimer != null)
        return;
      this._keepAliveTimer = this.CreateKeepAliveTimer(this._keepAliveInterval, this._keepAliveInterval);
    }

    private Timer CreateKeepAliveTimer(TimeSpan dueTime, TimeSpan period) => new Timer((TimerCallback) (state => this.SendKeepAliveMessage()), (object) this.Session, dueTime, period);

    private ISession CreateAndConnectSession()
    {
      ISession session = this._serviceFactory.CreateSession(this.ConnectionInfo);
      session.HostKeyReceived += new EventHandler<HostKeyEventArgs>(this.Session_HostKeyReceived);
      session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      try
      {
        session.Connect();
        return session;
      }
      catch
      {
        this.DisposeSession(session);
        throw;
      }
    }

    private void DisposeSession(ISession session)
    {
      session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      session.HostKeyReceived -= new EventHandler<HostKeyEventArgs>(this.Session_HostKeyReceived);
      session.Dispose();
    }

    private void DisposeSession()
    {
      ISession session = this.Session;
      if (session == null)
        return;
      this.Session = (ISession) null;
      this.DisposeSession(session);
    }

    private bool IsSessionConnected()
    {
      ISession session = this.Session;
      return session != null && session.IsConnected;
    }
  }
}
