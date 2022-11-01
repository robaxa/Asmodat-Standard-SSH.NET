// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ForwardedPort
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet
{
  public abstract class ForwardedPort : IForwardedPort
  {
    internal ISession Session { get; set; }

    internal event EventHandler Closing;

    event EventHandler IForwardedPort.Closing
    {
      add => this.Closing += value;
      remove => this.Closing -= value;
    }

    public abstract bool IsStarted { get; }

    public event EventHandler<ExceptionEventArgs> Exception;

    public event EventHandler<PortForwardEventArgs> RequestReceived;

    public virtual void Start()
    {
      this.CheckDisposed();
      if (this.IsStarted)
        throw new InvalidOperationException("Forwarded port is already started.");
      if (this.Session == null)
        throw new InvalidOperationException("Forwarded port is not added to a client.");
      if (!this.Session.IsConnected)
        throw new SshConnectionException("Client not connected.");
      this.Session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      this.StartPort();
    }

    public virtual void Stop()
    {
      if (!this.IsStarted)
        return;
      this.StopPort(this.Session.ConnectionInfo.Timeout);
    }

    protected abstract void StartPort();

    protected virtual void StopPort(TimeSpan timeout)
    {
      this.RaiseClosing();
      ISession session = this.Session;
      if (session == null)
        return;
      session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      ISession session = this.Session;
      if (session != null)
      {
        this.StopPort(session.ConnectionInfo.Timeout);
        this.Session = (ISession) null;
      }
    }

    protected abstract void CheckDisposed();

    protected void RaiseExceptionEvent(System.Exception exception)
    {
      EventHandler<ExceptionEventArgs> exception1 = this.Exception;
      if (exception1 == null)
        return;
      exception1((object) this, new ExceptionEventArgs(exception));
    }

    protected void RaiseRequestReceived(string host, uint port)
    {
      EventHandler<PortForwardEventArgs> requestReceived = this.RequestReceived;
      if (requestReceived == null)
        return;
      requestReceived((object) this, new PortForwardEventArgs(host, port));
    }

    private void RaiseClosing()
    {
      EventHandler closing = this.Closing;
      if (closing == null)
        return;
      closing((object) this, EventArgs.Empty);
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e) => this.RaiseExceptionEvent(e.Exception);
  }
}
