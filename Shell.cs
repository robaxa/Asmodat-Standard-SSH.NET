// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Shell
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Renci.SshNet
{
  public class Shell : IDisposable
  {
    private readonly ISession _session;
    private IChannelSession _channel;
    private EventWaitHandle _channelClosedWaitHandle;
    private Stream _input;
    private readonly string _terminalName;
    private readonly uint _columns;
    private readonly uint _rows;
    private readonly uint _width;
    private readonly uint _height;
    private readonly IDictionary<TerminalModes, uint> _terminalModes;
    private EventWaitHandle _dataReaderTaskCompleted;
    private readonly Stream _outputStream;
    private readonly Stream _extendedOutputStream;
    private readonly int _bufferSize;
    private bool _disposed;

    public bool IsStarted { get; private set; }

    public event EventHandler<EventArgs> Starting;

    public event EventHandler<EventArgs> Started;

    public event EventHandler<EventArgs> Stopping;

    public event EventHandler<EventArgs> Stopped;

    public event EventHandler<ExceptionEventArgs> ErrorOccurred;

    internal Shell(
      ISession session,
      Stream input,
      Stream output,
      Stream extendedOutput,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModes,
      int bufferSize)
    {
      this._session = session;
      this._input = input;
      this._outputStream = output;
      this._extendedOutputStream = extendedOutput;
      this._terminalName = terminalName;
      this._columns = columns;
      this._rows = rows;
      this._width = width;
      this._height = height;
      this._terminalModes = terminalModes;
      this._bufferSize = bufferSize;
    }

    public void Start()
    {
      if (this.IsStarted)
        throw new SshException("Shell is started.");
      if (this.Starting != null)
        this.Starting((object) this, new EventArgs());
      this._channel = this._session.CreateChannelSession();
      this._channel.DataReceived += new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      this._channel.ExtendedDataReceived += new EventHandler<ChannelExtendedDataEventArgs>(this.Channel_ExtendedDataReceived);
      this._channel.Closed += new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      this._session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
      this._session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      this._channel.Open();
      this._channel.SendPseudoTerminalRequest(this._terminalName, this._columns, this._rows, this._width, this._height, this._terminalModes);
      this._channel.SendShellRequest();
      this._channelClosedWaitHandle = (EventWaitHandle) new AutoResetEvent(false);
      this._dataReaderTaskCompleted = (EventWaitHandle) new ManualResetEvent(false);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          byte[] buffer = new byte[this._bufferSize];
          while (this._channel.IsOpen)
          {
            IAsyncResult asyncResult = this._input.BeginRead(buffer, 0, buffer.Length, (AsyncCallback) (result =>
            {
              if (this._input == null)
                return;
              this._channel.SendData(buffer, 0, this._input.EndRead(result));
            }), (object) null);
            WaitHandle.WaitAny(new WaitHandle[2]
            {
              asyncResult.AsyncWaitHandle,
              (WaitHandle) this._channelClosedWaitHandle
            });
            if (!asyncResult.IsCompleted)
              break;
          }
        }
        catch (Exception ex)
        {
          this.RaiseError(new ExceptionEventArgs(ex));
        }
        finally
        {
          this._dataReaderTaskCompleted.Set();
        }
      }));
      this.IsStarted = true;
      if (this.Started == null)
        return;
      this.Started((object) this, new EventArgs());
    }

    public void Stop()
    {
      if (!this.IsStarted)
        throw new SshException("Shell is not started.");
      if (this._channel == null)
        return;
      this._channel.Dispose();
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e) => this.RaiseError(e);

    private void RaiseError(ExceptionEventArgs e)
    {
      EventHandler<ExceptionEventArgs> errorOccurred = this.ErrorOccurred;
      if (errorOccurred == null)
        return;
      errorOccurred((object) this, e);
    }

    private void Session_Disconnected(object sender, EventArgs e) => this.Stop();

    private void Channel_ExtendedDataReceived(object sender, ChannelExtendedDataEventArgs e)
    {
      if (this._extendedOutputStream == null)
        return;
      this._extendedOutputStream.Write(e.Data, 0, e.Data.Length);
    }

    private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
    {
      if (this._outputStream == null)
        return;
      this._outputStream.Write(e.Data, 0, e.Data.Length);
    }

    private void Channel_Closed(object sender, ChannelEventArgs e)
    {
      if (this.Stopping != null)
        ThreadAbstraction.ExecuteThread((Action) (() => this.Stopping((object) this, new EventArgs())));
      this._channel.Dispose();
      this._channelClosedWaitHandle.Set();
      this._input.Dispose();
      this._input = (Stream) null;
      this._dataReaderTaskCompleted.WaitOne(this._session.ConnectionInfo.Timeout);
      this._dataReaderTaskCompleted.Dispose();
      this._dataReaderTaskCompleted = (EventWaitHandle) null;
      this._channel.DataReceived -= new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      this._channel.ExtendedDataReceived -= new EventHandler<ChannelExtendedDataEventArgs>(this.Channel_ExtendedDataReceived);
      this._channel.Closed -= new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      this.UnsubscribeFromSessionEvents(this._session);
      if (this.Stopped != null)
        ThreadAbstraction.ExecuteThread((Action) (() => this.Stopped((object) this, new EventArgs())));
      this._channel = (IChannelSession) null;
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
      if (this._disposed || !disposing)
        return;
      this.UnsubscribeFromSessionEvents(this._session);
      EventWaitHandle closedWaitHandle = this._channelClosedWaitHandle;
      if (closedWaitHandle != null)
      {
        closedWaitHandle.Dispose();
        this._channelClosedWaitHandle = (EventWaitHandle) null;
      }
      IChannelSession channel = this._channel;
      if (channel != null)
      {
        channel.Dispose();
        this._channel = (IChannelSession) null;
      }
      EventWaitHandle readerTaskCompleted = this._dataReaderTaskCompleted;
      if (readerTaskCompleted != null)
      {
        readerTaskCompleted.Dispose();
        this._dataReaderTaskCompleted = (EventWaitHandle) null;
      }
      this._disposed = true;
    }

    ~Shell() => this.Dispose(false);
  }
}
