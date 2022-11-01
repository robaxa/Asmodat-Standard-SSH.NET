// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.SshCommand
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Renci.SshNet
{
  public class SshCommand : IDisposable
  {
    private ISession _session;
    private readonly Encoding _encoding;
    private IChannelSession _channel;
    private CommandAsyncResult _asyncResult;
    private AsyncCallback _callback;
    private EventWaitHandle _sessionErrorOccuredWaitHandle;
    private Exception _exception;
    private bool _hasError;
    private readonly object _endExecuteLock = new object();
    private StringBuilder _result;
    private StringBuilder _error;
    private bool _isDisposed;

    public string CommandText { get; private set; }

    public TimeSpan CommandTimeout { get; set; }

    public int ExitStatus { get; private set; }

    public Stream OutputStream { get; private set; }

    public Stream ExtendedOutputStream { get; private set; }

    public string Result
    {
      get
      {
        if (this._result == null)
          this._result = new StringBuilder();
        if (this.OutputStream != null && this.OutputStream.Length > 0L)
          this._result.Append(new StreamReader(this.OutputStream, this._encoding).ReadToEnd());
        return this._result.ToString();
      }
    }

    public string Error
    {
      get
      {
        if (!this._hasError)
          return string.Empty;
        if (this._error == null)
          this._error = new StringBuilder();
        if (this.ExtendedOutputStream != null && this.ExtendedOutputStream.Length > 0L)
          this._error.Append(new StreamReader(this.ExtendedOutputStream, this._encoding).ReadToEnd());
        return this._error.ToString();
      }
    }

    internal SshCommand(ISession session, string commandText, Encoding encoding)
    {
      if (session == null)
        throw new ArgumentNullException(nameof (session));
      if (commandText == null)
        throw new ArgumentNullException(nameof (commandText));
      if (encoding == null)
        throw new ArgumentNullException(nameof (encoding));
      this._session = session;
      this.CommandText = commandText;
      this._encoding = encoding;
      this.CommandTimeout = Session.InfiniteTimeSpan;
      this._sessionErrorOccuredWaitHandle = (EventWaitHandle) new AutoResetEvent(false);
      this._session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
      this._session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
    }

    public IAsyncResult BeginExecute() => this.BeginExecute((AsyncCallback) null, (object) null);

    public IAsyncResult BeginExecute(AsyncCallback callback) => this.BeginExecute(callback, (object) null);

    public IAsyncResult BeginExecute(AsyncCallback callback, object state)
    {
      this._asyncResult = this._asyncResult == null || this._asyncResult.EndCalled ? new CommandAsyncResult()
      {
        AsyncWaitHandle = (WaitHandle) new ManualResetEvent(false),
        IsCompleted = false,
        AsyncState = state
      } : throw new InvalidOperationException("Asynchronous operation is already in progress.");
      if (this._channel != null)
        throw new SshException("Invalid operation.");
      if (string.IsNullOrEmpty(this.CommandText))
        throw new ArgumentException("CommandText property is empty.");
      Stream outputStream = this.OutputStream;
      if (outputStream != null)
      {
        outputStream.Dispose();
        this.OutputStream = (Stream) null;
      }
      Stream extendedOutputStream = this.ExtendedOutputStream;
      if (extendedOutputStream != null)
      {
        extendedOutputStream.Dispose();
        this.ExtendedOutputStream = (Stream) null;
      }
      this.OutputStream = (Stream) new PipeStream();
      this.ExtendedOutputStream = (Stream) new PipeStream();
      this._result = (StringBuilder) null;
      this._error = (StringBuilder) null;
      this._callback = callback;
      this._channel = this.CreateChannel();
      this._channel.Open();
      this._channel.SendExecRequest(this.CommandText);
      return (IAsyncResult) this._asyncResult;
    }

    public IAsyncResult BeginExecute(
      string commandText,
      AsyncCallback callback,
      object state)
    {
      this.CommandText = commandText;
      return this.BeginExecute(callback, state);
    }

    public string EndExecute(IAsyncResult asyncResult)
    {
      if (asyncResult == null)
        throw new ArgumentNullException(nameof (asyncResult));
      if (!(asyncResult is CommandAsyncResult commandAsyncResult) || this._asyncResult != commandAsyncResult)
        throw new ArgumentException(string.Format("The {0} object was not returned from the corresponding asynchronous method on this class.", (object) typeof (IAsyncResult).Name));
      lock (this._endExecuteLock)
      {
        if (commandAsyncResult.EndCalled)
          throw new ArgumentException("EndExecute can only be called once for each asynchronous operation.");
        this.WaitOnHandle(this._asyncResult.AsyncWaitHandle);
        this.UnsubscribeFromEventsAndDisposeChannel((IChannel) this._channel);
        this._channel = (IChannelSession) null;
        commandAsyncResult.EndCalled = true;
        return this.Result;
      }
    }

    public string Execute() => this.EndExecute(this.BeginExecute((AsyncCallback) null, (object) null));

    public void CancelAsync()
    {
      if (this._channel == null || !this._channel.IsOpen || this._asyncResult == null)
        return;
      this._channel.Dispose();
    }

    public string Execute(string commandText)
    {
      this.CommandText = commandText;
      return this.Execute();
    }

    private IChannelSession CreateChannel()
    {
      IChannelSession channelSession = this._session.CreateChannelSession();
      channelSession.DataReceived += new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      channelSession.ExtendedDataReceived += new EventHandler<ChannelExtendedDataEventArgs>(this.Channel_ExtendedDataReceived);
      channelSession.RequestReceived += new EventHandler<ChannelRequestEventArgs>(this.Channel_RequestReceived);
      channelSession.Closed += new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      return channelSession;
    }

    private void Session_Disconnected(object sender, EventArgs e)
    {
      if (this._isDisposed)
        return;
      this._exception = (Exception) new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReason.ConnectionLost);
      this._sessionErrorOccuredWaitHandle.Set();
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
    {
      if (this._isDisposed)
        return;
      this._exception = e.Exception;
      this._sessionErrorOccuredWaitHandle.Set();
    }

    private void Channel_Closed(object sender, ChannelEventArgs e)
    {
      this.OutputStream?.Flush();
      this.ExtendedOutputStream?.Flush();
      this._asyncResult.IsCompleted = true;
      if (this._callback != null)
        ThreadAbstraction.ExecuteThread((Action) (() => this._callback((IAsyncResult) this._asyncResult)));
      ((EventWaitHandle) this._asyncResult.AsyncWaitHandle).Set();
    }

    private void Channel_RequestReceived(object sender, ChannelRequestEventArgs e)
    {
      if (e.Info is ExitStatusRequestInfo info)
      {
        this.ExitStatus = (int) info.ExitStatus;
        if (!info.WantReply)
          return;
        this._session.SendMessage((Message) new ChannelSuccessMessage(this._channel.LocalChannelNumber));
      }
      else if (e.Info.WantReply)
        this._session.SendMessage((Message) new ChannelFailureMessage(this._channel.LocalChannelNumber));
    }

    private void Channel_ExtendedDataReceived(object sender, ChannelExtendedDataEventArgs e)
    {
      if (this.ExtendedOutputStream != null)
      {
        this.ExtendedOutputStream.Write(e.Data, 0, e.Data.Length);
        this.ExtendedOutputStream.Flush();
      }
      if (e.DataTypeCode != 1U)
        return;
      this._hasError = true;
    }

    private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
    {
      if (this.OutputStream != null)
      {
        this.OutputStream.Write(e.Data, 0, e.Data.Length);
        this.OutputStream.Flush();
      }
      if (this._asyncResult == null)
        return;
      lock (this._asyncResult)
        this._asyncResult.BytesReceived += e.Data.Length;
    }

    private void WaitOnHandle(WaitHandle waitHandle)
    {
      switch (WaitHandle.WaitAny(new WaitHandle[2]
      {
        (WaitHandle) this._sessionErrorOccuredWaitHandle,
        waitHandle
      }, this.CommandTimeout))
      {
        case 0:
          throw this._exception;
        case 258:
          throw new SshOperationTimeoutException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Command '{0}' has timed out.", (object) this.CommandText));
      }
    }

    private void UnsubscribeFromEventsAndDisposeChannel(IChannel channel)
    {
      if (channel == null)
        return;
      channel.DataReceived -= new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      channel.ExtendedDataReceived -= new EventHandler<ChannelExtendedDataEventArgs>(this.Channel_ExtendedDataReceived);
      channel.RequestReceived -= new EventHandler<ChannelRequestEventArgs>(this.Channel_RequestReceived);
      channel.Closed -= new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      channel.Dispose();
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
      ISession session = this._session;
      if (session != null)
      {
        session.Disconnected -= new EventHandler<EventArgs>(this.Session_Disconnected);
        session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
        this._session = (ISession) null;
      }
      IChannelSession channel = this._channel;
      if (channel != null)
      {
        this.UnsubscribeFromEventsAndDisposeChannel((IChannel) channel);
        this._channel = (IChannelSession) null;
      }
      Stream outputStream = this.OutputStream;
      if (outputStream != null)
      {
        outputStream.Dispose();
        this.OutputStream = (Stream) null;
      }
      Stream extendedOutputStream = this.ExtendedOutputStream;
      if (extendedOutputStream != null)
      {
        extendedOutputStream.Dispose();
        this.ExtendedOutputStream = (Stream) null;
      }
      EventWaitHandle occuredWaitHandle = this._sessionErrorOccuredWaitHandle;
      if (occuredWaitHandle != null)
      {
        occuredWaitHandle.Dispose();
        this._sessionErrorOccuredWaitHandle = (EventWaitHandle) null;
      }
      this._isDisposed = true;
    }

    ~SshCommand() => this.Dispose(false);
  }
}
