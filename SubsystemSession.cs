// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.SubsystemSession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System;
using System.Globalization;
using System.Threading;

namespace Renci.SshNet
{
  internal abstract class SubsystemSession : ISubsystemSession, IDisposable
  {
    private const int SystemWaitHandleCount = 3;
    private ISession _session;
    private readonly string _subsystemName;
    private IChannelSession _channel;
    private Exception _exception;
    private EventWaitHandle _errorOccuredWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private EventWaitHandle _sessionDisconnectedWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private EventWaitHandle _channelClosedWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private bool _isDisposed;

    public int OperationTimeout { get; private set; }

    public event EventHandler<ExceptionEventArgs> ErrorOccurred;

    public event EventHandler<EventArgs> Disconnected;

    internal IChannelSession Channel
    {
      get
      {
        this.EnsureNotDisposed();
        return this._channel;
      }
    }

    public bool IsOpen => this._channel != null && this._channel.IsOpen;

    protected SubsystemSession(ISession session, string subsystemName, int operationTimeout)
    {
      if (session == null)
        throw new ArgumentNullException(nameof (session));
      if (subsystemName == null)
        throw new ArgumentNullException(nameof (subsystemName));
      this._session = session;
      this._subsystemName = subsystemName;
      this.OperationTimeout = operationTimeout;
    }

    public void Connect()
    {
      this.EnsureNotDisposed();
      if (this.IsOpen)
        throw new InvalidOperationException("The session is already connected.");
      this._errorOccuredWaitHandle.Reset();
      this._sessionDisconnectedWaitHandle.Reset();
      this._sessionDisconnectedWaitHandle.Reset();
      this._channelClosedWaitHandle.Reset();
      this._session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      this._session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
      this._channel = this._session.CreateChannelSession();
      this._channel.DataReceived += new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      this._channel.Exception += new EventHandler<ExceptionEventArgs>(this.Channel_Exception);
      this._channel.Closed += new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      this._channel.Open();
      if (!this._channel.SendSubsystemRequest(this._subsystemName))
      {
        this.Disconnect();
        throw new SshException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Subsystem '{0}' could not be executed.", (object) this._subsystemName));
      }
      this.OnChannelOpen();
    }

    public void Disconnect()
    {
      this.UnsubscribeFromSessionEvents(this._session);
      IChannelSession channel = this._channel;
      if (channel == null)
        return;
      this._channel = (IChannelSession) null;
      channel.DataReceived -= new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      channel.Exception -= new EventHandler<ExceptionEventArgs>(this.Channel_Exception);
      channel.Closed -= new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      channel.Dispose();
    }

    public void SendData(byte[] data)
    {
      this.EnsureNotDisposed();
      this.EnsureSessionIsOpen();
      this._channel.SendData(data);
    }

    protected abstract void OnChannelOpen();

    protected abstract void OnDataReceived(byte[] data);

    protected void RaiseError(Exception error)
    {
      this._exception = error;
      DiagnosticAbstraction.Log("Raised exception: " + error?.ToString());
      this._errorOccuredWaitHandle?.Set();
      this.SignalErrorOccurred(error);
    }

    private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
    {
      try
      {
        this.OnDataReceived(e.Data);
      }
      catch (Exception ex)
      {
        this.RaiseError(ex);
      }
    }

    private void Channel_Exception(object sender, ExceptionEventArgs e) => this.RaiseError(e.Exception);

    private void Channel_Closed(object sender, ChannelEventArgs e) => this._channelClosedWaitHandle?.Set();

    public void WaitOnHandle(WaitHandle waitHandle, int millisecondsTimeout)
    {
      int num = WaitHandle.WaitAny(new WaitHandle[4]
      {
        (WaitHandle) this._errorOccuredWaitHandle,
        (WaitHandle) this._sessionDisconnectedWaitHandle,
        (WaitHandle) this._channelClosedWaitHandle,
        waitHandle
      }, millisecondsTimeout);
      switch (num)
      {
        case 0:
          throw this._exception;
        case 1:
          throw new SshException("Connection was closed by the server.");
        case 2:
          throw new SshException("Channel was closed.");
        case 3:
          break;
        case 258:
          throw new SshOperationTimeoutException("Operation has timed out.");
        default:
          throw new NotImplementedException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", (object) num));
      }
    }

    public bool WaitOne(WaitHandle waitHandle, int millisecondsTimeout)
    {
      int num = WaitHandle.WaitAny(new WaitHandle[4]
      {
        (WaitHandle) this._errorOccuredWaitHandle,
        (WaitHandle) this._sessionDisconnectedWaitHandle,
        (WaitHandle) this._channelClosedWaitHandle,
        waitHandle
      }, millisecondsTimeout);
      switch (num)
      {
        case 0:
          throw this._exception;
        case 1:
          throw new SshException("Connection was closed by the server.");
        case 2:
          throw new SshException("Channel was closed.");
        case 3:
          return true;
        case 258:
          return false;
        default:
          throw new NotImplementedException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", (object) num));
      }
    }

    public int WaitAny(WaitHandle waitHandle1, WaitHandle waitHandle2, int millisecondsTimeout)
    {
      int num = WaitHandle.WaitAny(new WaitHandle[5]
      {
        (WaitHandle) this._errorOccuredWaitHandle,
        (WaitHandle) this._sessionDisconnectedWaitHandle,
        (WaitHandle) this._channelClosedWaitHandle,
        waitHandle1,
        waitHandle2
      }, millisecondsTimeout);
      switch (num)
      {
        case 0:
          throw this._exception;
        case 1:
          throw new SshException("Connection was closed by the server.");
        case 2:
          throw new SshException("Channel was closed.");
        case 3:
          return 0;
        case 4:
          return 1;
        case 258:
          throw new SshOperationTimeoutException("Operation has timed out.");
        default:
          throw new NotImplementedException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "WaitAny return value '{0}' is not implemented.", (object) num));
      }
    }

    public int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
    {
      int num = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
      switch (num)
      {
        case 0:
          throw this._exception;
        case 1:
          throw new SshException("Connection was closed by the server.");
        case 2:
          throw new SshException("Channel was closed.");
        case 258:
          throw new SshOperationTimeoutException("Operation has timed out.");
        default:
          return num - 3;
      }
    }

    public WaitHandle[] CreateWaitHandleArray(
      WaitHandle waitHandle1,
      WaitHandle waitHandle2)
    {
      return new WaitHandle[5]
      {
        (WaitHandle) this._errorOccuredWaitHandle,
        (WaitHandle) this._sessionDisconnectedWaitHandle,
        (WaitHandle) this._channelClosedWaitHandle,
        waitHandle1,
        waitHandle2
      };
    }

    public WaitHandle[] CreateWaitHandleArray(params WaitHandle[] waitHandles)
    {
      WaitHandle[] waitHandleArray = new WaitHandle[waitHandles.Length + 3];
      waitHandleArray[0] = (WaitHandle) this._errorOccuredWaitHandle;
      waitHandleArray[1] = (WaitHandle) this._sessionDisconnectedWaitHandle;
      waitHandleArray[2] = (WaitHandle) this._channelClosedWaitHandle;
      for (int index = 0; index < waitHandles.Length; ++index)
        waitHandleArray[index + 3] = waitHandles[index];
      return waitHandleArray;
    }

    private void Session_Disconnected(object sender, EventArgs e)
    {
      this._sessionDisconnectedWaitHandle?.Set();
      this.SignalDisconnected();
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e) => this.RaiseError(e.Exception);

    private void SignalErrorOccurred(Exception error)
    {
      EventHandler<ExceptionEventArgs> errorOccurred = this.ErrorOccurred;
      if (errorOccurred == null)
        return;
      errorOccurred((object) this, new ExceptionEventArgs(error));
    }

    private void SignalDisconnected()
    {
      EventHandler<EventArgs> disconnected = this.Disconnected;
      if (disconnected == null)
        return;
      disconnected((object) this, new EventArgs());
    }

    private void EnsureSessionIsOpen()
    {
      if (!this.IsOpen)
        throw new InvalidOperationException("The session is not open.");
    }

    private void UnsubscribeFromSessionEvents(ISession session)
    {
      if (session == null)
        return;
      session.Disconnected -= new EventHandler<EventArgs>(this.Session_Disconnected);
      session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._isDisposed || !disposing)
        return;
      this.Disconnect();
      this._session = (ISession) null;
      EventWaitHandle occuredWaitHandle = this._errorOccuredWaitHandle;
      if (occuredWaitHandle != null)
      {
        this._errorOccuredWaitHandle = (EventWaitHandle) null;
        occuredWaitHandle.Dispose();
      }
      EventWaitHandle disconnectedWaitHandle = this._sessionDisconnectedWaitHandle;
      if (disconnectedWaitHandle != null)
      {
        this._sessionDisconnectedWaitHandle = (EventWaitHandle) null;
        disconnectedWaitHandle.Dispose();
      }
      EventWaitHandle closedWaitHandle = this._channelClosedWaitHandle;
      if (closedWaitHandle != null)
      {
        this._channelClosedWaitHandle = (EventWaitHandle) null;
        closedWaitHandle.Dispose();
      }
      this._isDisposed = true;
    }

    ~SubsystemSession() => this.Dispose(false);

    private void EnsureNotDisposed()
    {
      if (this._isDisposed)
        throw new ObjectDisposedException(this.GetType().FullName);
    }
  }
}
