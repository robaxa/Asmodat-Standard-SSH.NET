// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ShellStream
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Renci.SshNet
{
  public class ShellStream : Stream
  {
    private const string CrLf = "\r\n";
    private readonly ISession _session;
    private readonly Encoding _encoding;
    private readonly int _bufferSize;
    private readonly Queue<byte> _incoming;
    private readonly Queue<byte> _outgoing;
    private IChannelSession _channel;
    private AutoResetEvent _dataReceived = new AutoResetEvent(false);
    private bool _isDisposed;

    public event EventHandler<ShellDataEventArgs> DataReceived;

    public event EventHandler<ExceptionEventArgs> ErrorOccurred;

    public bool DataAvailable
    {
      get
      {
        lock (this._incoming)
          return this._incoming.Count > 0;
      }
    }

    internal int BufferSize => this._bufferSize;

    internal ShellStream(
      ISession session,
      string terminalName,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModeValues,
      int bufferSize)
    {
      this._encoding = session.ConnectionInfo.Encoding;
      this._session = session;
      this._bufferSize = bufferSize;
      this._incoming = new Queue<byte>();
      this._outgoing = new Queue<byte>();
      this._channel = this._session.CreateChannelSession();
      this._channel.DataReceived += new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
      this._channel.Closed += new EventHandler<ChannelEventArgs>(this.Channel_Closed);
      this._session.Disconnected += new EventHandler<EventArgs>(this.Session_Disconnected);
      this._session.ErrorOccured += new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
      try
      {
        this._channel.Open();
        if (!this._channel.SendPseudoTerminalRequest(terminalName, columns, rows, width, height, terminalModeValues))
          throw new SshException("The pseudo-terminal request was not accepted by the server. Consult the server log for more information.");
        if (!this._channel.SendShellRequest())
          throw new SshException("The request to start a shell was not accepted by the server. Consult the server log for more information.");
      }
      catch
      {
        this.UnsubscribeFromSessionEvents(session);
        this._channel.Dispose();
        throw;
      }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override void Flush()
    {
      if (this._channel == null)
        throw new ObjectDisposedException(nameof (ShellStream));
      if (this._outgoing.Count <= 0)
        return;
      this._channel.SendData(this._outgoing.ToArray());
      this._outgoing.Clear();
    }

    public override long Length
    {
      get
      {
        lock (this._incoming)
          return (long) this._incoming.Count;
      }
    }

    public override long Position
    {
      get => 0;
      set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      int num = 0;
      lock (this._incoming)
      {
        for (; num < count && this._incoming.Count > 0; ++num)
          buffer[offset + num] = this._incoming.Dequeue();
      }
      return num;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
      foreach (byte num in buffer.Take(offset, count))
      {
        if (this._outgoing.Count == this._bufferSize)
          this.Flush();
        this._outgoing.Enqueue(num);
      }
    }

    public void Expect(params ExpectAction[] expectActions) => this.Expect(TimeSpan.Zero, expectActions);

    public void Expect(TimeSpan timeout, params ExpectAction[] expectActions)
    {
      bool flag = false;
      string empty = string.Empty;
      do
      {
        lock (this._incoming)
        {
          if (this._incoming.Count > 0)
            empty = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
          if (empty.Length > 0)
          {
            foreach (ExpectAction expectAction in expectActions)
            {
              Match match = expectAction.Expect.Match(empty);
              if (match.Success)
              {
                string str = empty.Substring(0, match.Index + match.Length);
                for (int index = 0; index < match.Index + match.Length && this._incoming.Count > 0; ++index)
                {
                  int num = (int) this._incoming.Dequeue();
                }
                expectAction.Action(str);
                flag = true;
              }
            }
          }
        }
        if (!flag)
        {
          if (timeout.Ticks > 0L)
          {
            if (!this._dataReceived.WaitOne(timeout))
              goto label_19;
          }
          else
            this._dataReceived.WaitOne();
        }
      }
      while (!flag);
      goto label_20;
label_19:
      return;
label_20:;
    }

    public IAsyncResult BeginExpect(params ExpectAction[] expectActions) => this.BeginExpect(TimeSpan.Zero, (AsyncCallback) null, (object) null, expectActions);

    public IAsyncResult BeginExpect(
      AsyncCallback callback,
      params ExpectAction[] expectActions)
    {
      return this.BeginExpect(TimeSpan.Zero, callback, (object) null, expectActions);
    }

    public IAsyncResult BeginExpect(
      AsyncCallback callback,
      object state,
      params ExpectAction[] expectActions)
    {
      return this.BeginExpect(TimeSpan.Zero, callback, state, expectActions);
    }

    public IAsyncResult BeginExpect(
      TimeSpan timeout,
      AsyncCallback callback,
      object state,
      params ExpectAction[] expectActions)
    {
      string text = string.Empty;
      ExpectAsyncResult asyncResult = new ExpectAsyncResult(callback, state);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        string result = (string) null;
        try
        {
          while (true)
          {
            lock (this._incoming)
            {
              if (this._incoming.Count > 0)
                text = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
              if (text.Length > 0)
              {
                foreach (ExpectAction expectAction in expectActions)
                {
                  Match match = expectAction.Expect.Match(text);
                  if (match.Success)
                  {
                    string str = text.Substring(0, match.Index + match.Length);
                    for (int index = 0; index < match.Index + match.Length && this._incoming.Count > 0; ++index)
                    {
                      int num = (int) this._incoming.Dequeue();
                    }
                    expectAction.Action(str);
                    if (callback != null)
                      callback((IAsyncResult) asyncResult);
                    result = str;
                  }
                }
              }
            }
            if (result == null)
            {
              if (timeout.Ticks > 0L)
              {
                if (!this._dataReceived.WaitOne(timeout))
                  break;
              }
              else
                this._dataReceived.WaitOne();
            }
            else
              goto label_26;
          }
          if (callback != null)
            callback((IAsyncResult) asyncResult);
label_26:
          asyncResult.SetAsCompleted(result, true);
        }
        catch (Exception ex)
        {
          asyncResult.SetAsCompleted(ex, true);
        }
      }));
      return (IAsyncResult) asyncResult;
    }

    public string EndExpect(IAsyncResult asyncResult)
    {
      if (!(asyncResult is ExpectAsyncResult expectAsyncResult) || expectAsyncResult.EndInvokeCalled)
        throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");
      return expectAsyncResult.EndInvoke();
    }

    public string Expect(string text) => this.Expect(new Regex(Regex.Escape(text)), Session.InfiniteTimeSpan);

    public string Expect(string text, TimeSpan timeout) => this.Expect(new Regex(Regex.Escape(text)), timeout);

    public string Expect(Regex regex) => this.Expect(regex, TimeSpan.Zero);

    public string Expect(Regex regex, TimeSpan timeout)
    {
      string empty = string.Empty;
      while (true)
      {
        lock (this._incoming)
        {
          if (this._incoming.Count > 0)
            empty = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
          Match match = regex.Match(empty);
          if (match.Success)
          {
            for (int index = 0; index < match.Index + match.Length && this._incoming.Count > 0; ++index)
            {
              int num = (int) this._incoming.Dequeue();
            }
            goto label_16;
          }
        }
        if (timeout.Ticks > 0L)
        {
          if (!this._dataReceived.WaitOne(timeout))
            break;
        }
        else
          this._dataReceived.WaitOne();
      }
      return (string) null;
label_16:
      return empty;
    }

    public string ReadLine() => this.ReadLine(TimeSpan.Zero);

    public string ReadLine(TimeSpan timeout)
    {
      string str = string.Empty;
      while (true)
      {
        lock (this._incoming)
        {
          if (this._incoming.Count > 0)
            str = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
          int length = str.IndexOf("\r\n", StringComparison.Ordinal);
          if (length >= 0)
          {
            str = str.Substring(0, length);
            int byteCount = this._encoding.GetByteCount(str + "\r\n");
            for (int index = 0; index < byteCount; ++index)
            {
              int num = (int) this._incoming.Dequeue();
            }
            goto label_16;
          }
        }
        if (timeout.Ticks > 0L)
        {
          if (!this._dataReceived.WaitOne(timeout))
            break;
        }
        else
          this._dataReceived.WaitOne();
      }
      return (string) null;
label_16:
      return str;
    }

    public string Read()
    {
      string str;
      lock (this._incoming)
      {
        str = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
        this._incoming.Clear();
      }
      return str;
    }

    public void Write(string text)
    {
      if (text == null)
        return;
      if (this._channel == null)
        throw new ObjectDisposedException(nameof (ShellStream));
      this._channel.SendData(this._encoding.GetBytes(text));
    }

    public void WriteLine(string line) => this.Write(line + "\r");

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this._isDisposed)
        return;
      if (disposing)
      {
        this.UnsubscribeFromSessionEvents(this._session);
        if (this._channel != null)
        {
          this._channel.DataReceived -= new EventHandler<ChannelDataEventArgs>(this.Channel_DataReceived);
          this._channel.Closed -= new EventHandler<ChannelEventArgs>(this.Channel_Closed);
          this._channel.Dispose();
          this._channel = (IChannelSession) null;
        }
        if (this._dataReceived != null)
        {
          this._dataReceived.Dispose();
          this._dataReceived = (AutoResetEvent) null;
        }
        this._isDisposed = true;
      }
      else
        this.UnsubscribeFromSessionEvents(this._session);
    }

    private void UnsubscribeFromSessionEvents(ISession session)
    {
      if (session == null)
        return;
      session.Disconnected -= new EventHandler<EventArgs>(this.Session_Disconnected);
      session.ErrorOccured -= new EventHandler<ExceptionEventArgs>(this.Session_ErrorOccured);
    }

    private void Session_ErrorOccured(object sender, ExceptionEventArgs e) => this.OnRaiseError(e);

    private void Session_Disconnected(object sender, EventArgs e)
    {
      if (this._channel == null)
        return;
      this._channel.Dispose();
    }

    private void Channel_Closed(object sender, ChannelEventArgs e) => this.Dispose();

    private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
    {
      lock (this._incoming)
      {
        foreach (byte num in e.Data)
          this._incoming.Enqueue(num);
      }
      if (this._dataReceived != null)
        this._dataReceived.Set();
      this.OnDataReceived(e.Data);
    }

    private void OnRaiseError(ExceptionEventArgs e)
    {
      EventHandler<ExceptionEventArgs> errorOccurred = this.ErrorOccurred;
      if (errorOccurred == null)
        return;
      errorOccurred((object) this, e);
    }

    private void OnDataReceived(byte[] data)
    {
      EventHandler<ShellDataEventArgs> dataReceived = this.DataReceived;
      if (dataReceived == null)
        return;
      dataReceived((object) this, new ShellDataEventArgs(data));
    }
  }
}
