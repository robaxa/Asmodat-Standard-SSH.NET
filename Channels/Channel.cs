// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.Channel
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System;
using System.Globalization;
using System.Threading;

namespace Renci.SshNet.Channels
{
  internal abstract class Channel : IChannel, IDisposable
  {
    private EventWaitHandle _channelClosedWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private EventWaitHandle _channelServerWindowAdjustWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private readonly object _serverWindowSizeLock = new object();
    private readonly uint _initialWindowSize;
    private uint? _remoteWindowSize;
    private uint? _remoteChannelNumber;
    private uint? _remotePacketSize;
    private ISession _session;
    private bool _closeMessageSent;
    private bool _closeMessageReceived;
    private bool _eofMessageReceived;
    private bool _eofMessageSent;
    private bool _isDisposed;

    public event EventHandler<ExceptionEventArgs> Exception;

    protected Channel(
      ISession session,
      uint localChannelNumber,
      uint localWindowSize,
      uint localPacketSize)
    {
      this._session = session;
      this._initialWindowSize = localWindowSize;
      this.LocalChannelNumber = localChannelNumber;
      this.LocalPacketSize = localPacketSize;
      this.LocalWindowSize = localWindowSize;
      session.ChannelWindowAdjustReceived += new EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>>(this.OnChannelWindowAdjust);
      session.ChannelDataReceived += new EventHandler<MessageEventArgs<ChannelDataMessage>>(this.OnChannelData);
      session.ChannelExtendedDataReceived += new EventHandler<MessageEventArgs<ChannelExtendedDataMessage>>(this.OnChannelExtendedData);
      session.ChannelEofReceived += new EventHandler<MessageEventArgs<ChannelEofMessage>>(this.OnChannelEof);
      session.ChannelCloseReceived += new EventHandler<MessageEventArgs<ChannelCloseMessage>>(this.OnChannelClose);
      session.ChannelRequestReceived += new EventHandler<MessageEventArgs<ChannelRequestMessage>>(this.OnChannelRequest);
      session.ChannelSuccessReceived += new EventHandler<MessageEventArgs<ChannelSuccessMessage>>(this.OnChannelSuccess);
      session.ChannelFailureReceived += new EventHandler<MessageEventArgs<ChannelFailureMessage>>(this.OnChannelFailure);
      session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
    }

    protected ISession Session => this._session;

    public abstract ChannelTypes ChannelType { get; }

    public uint LocalChannelNumber { get; private set; }

    public uint LocalPacketSize { get; private set; }

    public uint LocalWindowSize { get; private set; }

    public uint RemoteChannelNumber
    {
      get => this._remoteChannelNumber.HasValue ? this._remoteChannelNumber.Value : throw Channel.CreateRemoteChannelInfoNotAvailableException();
      private set => this._remoteChannelNumber = new uint?(value);
    }

    public uint RemotePacketSize
    {
      get => this._remotePacketSize.HasValue ? this._remotePacketSize.Value : throw Channel.CreateRemoteChannelInfoNotAvailableException();
      private set => this._remotePacketSize = new uint?(value);
    }

    public uint RemoteWindowSize
    {
      get => this._remoteWindowSize.HasValue ? this._remoteWindowSize.Value : throw Channel.CreateRemoteChannelInfoNotAvailableException();
      private set => this._remoteWindowSize = new uint?(value);
    }

    public bool IsOpen { get; protected set; }

    public event EventHandler<ChannelDataEventArgs> DataReceived;

    public event EventHandler<ChannelExtendedDataEventArgs> ExtendedDataReceived;

    public event EventHandler<ChannelEventArgs> EndOfData;

    public event EventHandler<ChannelEventArgs> Closed;

    public event EventHandler<ChannelRequestEventArgs> RequestReceived;

    public event EventHandler<ChannelEventArgs> RequestSucceeded;

    public event EventHandler<ChannelEventArgs> RequestFailed;

    protected bool IsConnected => this._session.IsConnected;

    protected IConnectionInfo ConnectionInfo => this._session.ConnectionInfo;

    protected SemaphoreLight SessionSemaphore => this._session.SessionSemaphore;

    protected void InitializeRemoteInfo(
      uint remoteChannelNumber,
      uint remoteWindowSize,
      uint remotePacketSize)
    {
      this.RemoteChannelNumber = remoteChannelNumber;
      this.RemoteWindowSize = remoteWindowSize;
      this.RemotePacketSize = remotePacketSize;
    }

    public void SendData(byte[] data) => this.SendData(data, 0, data.Length);

    public void SendData(byte[] data, int offset, int size)
    {
      if (!this.IsOpen)
        return;
      int messageLength = size;
      while (messageLength > 0)
      {
        int canBeSentInMessage = this.GetDataLengthThatCanBeSentInMessage(messageLength);
        this._session.SendMessage((Message) new ChannelDataMessage(this.RemoteChannelNumber, data, offset, canBeSentInMessage));
        messageLength -= canBeSentInMessage;
        offset += canBeSentInMessage;
      }
    }

    protected virtual void OnWindowAdjust(uint bytesToAdd)
    {
      lock (this._serverWindowSizeLock)
        this.RemoteWindowSize += bytesToAdd;
      this._channelServerWindowAdjustWaitHandle.Set();
    }

    protected virtual void OnData(byte[] data)
    {
      this.AdjustDataWindow(data);
      EventHandler<ChannelDataEventArgs> dataReceived = this.DataReceived;
      if (dataReceived == null)
        return;
      dataReceived((object) this, new ChannelDataEventArgs(this.LocalChannelNumber, data));
    }

    protected virtual void OnExtendedData(byte[] data, uint dataTypeCode)
    {
      this.AdjustDataWindow(data);
      EventHandler<ChannelExtendedDataEventArgs> extendedDataReceived = this.ExtendedDataReceived;
      if (extendedDataReceived == null)
        return;
      extendedDataReceived((object) this, new ChannelExtendedDataEventArgs(this.LocalChannelNumber, data, dataTypeCode));
    }

    protected virtual void OnEof()
    {
      this._eofMessageReceived = true;
      EventHandler<ChannelEventArgs> endOfData = this.EndOfData;
      if (endOfData == null)
        return;
      endOfData((object) this, new ChannelEventArgs(this.LocalChannelNumber));
    }

    protected virtual void OnClose()
    {
      this._closeMessageReceived = true;
      this._channelClosedWaitHandle?.Set();
      this.Close();
    }

    protected virtual void OnRequest(RequestInfo info)
    {
      EventHandler<ChannelRequestEventArgs> requestReceived = this.RequestReceived;
      if (requestReceived == null)
        return;
      requestReceived((object) this, new ChannelRequestEventArgs(info));
    }

    protected virtual void OnSuccess()
    {
      EventHandler<ChannelEventArgs> requestSucceeded = this.RequestSucceeded;
      if (requestSucceeded == null)
        return;
      requestSucceeded((object) this, new ChannelEventArgs(this.LocalChannelNumber));
    }

    protected virtual void OnFailure()
    {
      EventHandler<ChannelEventArgs> requestFailed = this.RequestFailed;
      if (requestFailed == null)
        return;
      requestFailed((object) this, new ChannelEventArgs(this.LocalChannelNumber));
    }

    private void RaiseExceptionEvent(System.Exception exception)
    {
      EventHandler<ExceptionEventArgs> exception1 = this.Exception;
      if (exception1 == null)
        return;
      exception1((object) this, new ExceptionEventArgs(exception));
    }

    private bool TrySendMessage(Message message) => this._session.TrySendMessage(message);

    protected void SendMessage(Message message)
    {
      if (!this.IsOpen)
        return;
      this._session.SendMessage(message);
    }

    public void SendEof()
    {
      if (!this.IsOpen)
        throw Channel.CreateChannelClosedException();
      lock (this)
      {
        this._session.SendMessage((Message) new ChannelEofMessage(this.RemoteChannelNumber));
        this._eofMessageSent = true;
      }
    }

    protected void WaitOnHandle(WaitHandle waitHandle) => this._session.WaitOnHandle(waitHandle);

    protected virtual void Close()
    {
      lock (this)
      {
        if (!this._eofMessageSent && !this._closeMessageReceived && !this._eofMessageReceived && this.IsOpen && this.IsConnected && this.TrySendMessage((Message) new ChannelEofMessage(this.RemoteChannelNumber)))
          this._eofMessageSent = true;
        if (!this._closeMessageSent && this.IsOpen && this.IsConnected && this.TrySendMessage((Message) new ChannelCloseMessage(this.RemoteChannelNumber)))
        {
          this._closeMessageSent = true;
          WaitResult waitResult = this._session.TryWait((WaitHandle) this._channelClosedWaitHandle, this.ConnectionInfo.ChannelCloseTimeout);
          if (waitResult != WaitResult.Success)
            DiagnosticAbstraction.Log(string.Format("Wait for channel close not successful: {0:G}.", (object) waitResult));
        }
        if (!this.IsOpen)
          return;
        this.IsOpen = false;
        if (this._closeMessageReceived)
        {
          EventHandler<ChannelEventArgs> closed = this.Closed;
          if (closed != null)
            closed((object) this, new ChannelEventArgs(this.LocalChannelNumber));
        }
      }
    }

    protected virtual void OnDisconnected()
    {
    }

    protected virtual void OnErrorOccured(System.Exception exp)
    {
    }

    private void Session_Disconnected(object sender, EventArgs e)
    {
      this.IsOpen = false;
      try
      {
        this.OnDisconnected();
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    protected void OnChannelException(System.Exception ex)
    {
      this.OnErrorOccured(ex);
      this.RaiseExceptionEvent(ex);
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
    {
      try
      {
        this.OnErrorOccured(e.Exception);
      }
      catch (System.Exception ex)
      {
        this.RaiseExceptionEvent(ex);
      }
    }

    private void OnChannelWindowAdjust(
      object sender,
      MessageEventArgs<ChannelWindowAdjustMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnWindowAdjust(e.Message.BytesToAdd);
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelData(object sender, MessageEventArgs<ChannelDataMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnData(e.Message.Data);
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelExtendedData(
      object sender,
      MessageEventArgs<ChannelExtendedDataMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnExtendedData(e.Message.Data, e.Message.DataTypeCode);
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelEof(object sender, MessageEventArgs<ChannelEofMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnEof();
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelClose(object sender, MessageEventArgs<ChannelCloseMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnClose();
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelRequest(object sender, MessageEventArgs<ChannelRequestMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        RequestInfo info;
        if (!this._session.ConnectionInfo.ChannelRequests.TryGetValue(e.Message.RequestName, out info))
          throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Request '{0}' is not supported.", (object) e.Message.RequestName));
        info.Load(e.Message.RequestData);
        this.OnRequest(info);
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelSuccess(object sender, MessageEventArgs<ChannelSuccessMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnSuccess();
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void OnChannelFailure(object sender, MessageEventArgs<ChannelFailureMessage> e)
    {
      if ((int) e.Message.LocalChannelNumber != (int) this.LocalChannelNumber)
        return;
      try
      {
        this.OnFailure();
      }
      catch (System.Exception ex)
      {
        this.OnChannelException(ex);
      }
    }

    private void AdjustDataWindow(byte[] messageData)
    {
      this.LocalWindowSize -= (uint) messageData.Length;
      if (this.LocalWindowSize >= this.LocalPacketSize)
        return;
      this.SendMessage((Message) new ChannelWindowAdjustMessage(this.RemoteChannelNumber, this._initialWindowSize - this.LocalWindowSize));
      this.LocalWindowSize = this._initialWindowSize;
    }

    private int GetDataLengthThatCanBeSentInMessage(int messageLength)
    {
      while (true)
      {
        lock (this._serverWindowSizeLock)
        {
          uint remoteWindowSize = this.RemoteWindowSize;
          if (remoteWindowSize == 0U)
          {
            this._channelServerWindowAdjustWaitHandle.Reset();
          }
          else
          {
            uint canBeSentInMessage = Math.Min(Math.Min(this.RemotePacketSize, (uint) messageLength), remoteWindowSize);
            this.RemoteWindowSize -= canBeSentInMessage;
            return (int) canBeSentInMessage;
          }
        }
        this.WaitOnHandle((WaitHandle) this._channelServerWindowAdjustWaitHandle);
      }
    }

    private static InvalidOperationException CreateRemoteChannelInfoNotAvailableException() => throw new InvalidOperationException("The channel has not been opened, or the open has not yet been confirmed.");

    private static InvalidOperationException CreateChannelClosedException() => throw new InvalidOperationException("The channel is closed.");

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._isDisposed || !disposing)
        return;
      this.Close();
      ISession session = this._session;
      if (session != null)
      {
        this._session = (ISession) null;
        session.ChannelWindowAdjustReceived -= new EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>>(this.OnChannelWindowAdjust);
        session.ChannelDataReceived -= new EventHandler<MessageEventArgs<ChannelDataMessage>>(this.OnChannelData);
        session.ChannelExtendedDataReceived -= new EventHandler<MessageEventArgs<ChannelExtendedDataMessage>>(this.OnChannelExtendedData);
        session.ChannelEofReceived -= new EventHandler<MessageEventArgs<ChannelEofMessage>>(this.OnChannelEof);
        session.ChannelCloseReceived -= new EventHandler<MessageEventArgs<ChannelCloseMessage>>(this.OnChannelClose);
        session.ChannelRequestReceived -= new EventHandler<MessageEventArgs<ChannelRequestMessage>>(this.OnChannelRequest);
        session.ChannelSuccessReceived -= new EventHandler<MessageEventArgs<ChannelSuccessMessage>>(this.OnChannelSuccess);
        session.ChannelFailureReceived -= new EventHandler<MessageEventArgs<ChannelFailureMessage>>(this.OnChannelFailure);
        session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
        session.Disconnected -= new EventHandler<EventArgs>(this.Session_Disconnected);
      }
      EventWaitHandle closedWaitHandle = this._channelClosedWaitHandle;
      if (closedWaitHandle != null)
      {
        this._channelClosedWaitHandle = (EventWaitHandle) null;
        closedWaitHandle.Dispose();
      }
      EventWaitHandle adjustWaitHandle = this._channelServerWindowAdjustWaitHandle;
      if (adjustWaitHandle != null)
      {
        this._channelServerWindowAdjustWaitHandle = (EventWaitHandle) null;
        adjustWaitHandle.Dispose();
      }
      this._isDisposed = true;
    }

    ~Channel() => this.Dispose(false);
  }
}
