// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.ChannelSession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Renci.SshNet.Channels
{
  internal sealed class ChannelSession : ClientChannel, IChannelSession, IChannel, IDisposable
  {
    private int _failedOpenAttempts;
    private int _sessionSemaphoreObtained;
    private EventWaitHandle _channelOpenResponseWaitHandle = (EventWaitHandle) new AutoResetEvent(false);
    private EventWaitHandle _channelRequestResponse = (EventWaitHandle) new ManualResetEvent(false);
    private bool _channelRequestSucces;

    public ChannelSession(
      ISession session,
      uint localChannelNumber,
      uint localWindowSize,
      uint localPacketSize)
      : base(session, localChannelNumber, localWindowSize, localPacketSize)
    {
    }

    public override ChannelTypes ChannelType => ChannelTypes.Session;

    public void Open()
    {
      while (!this.IsOpen && this._failedOpenAttempts < this.ConnectionInfo.RetryAttempts)
      {
        this.SendChannelOpenMessage();
        try
        {
          this.WaitOnHandle((WaitHandle) this._channelOpenResponseWaitHandle);
        }
        catch (Exception ex)
        {
          this.ReleaseSemaphore();
          throw;
        }
      }
      if (!this.IsOpen)
        throw new SshException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Failed to open a channel after {0} attempts.", (object) this._failedOpenAttempts));
    }

    protected override void OnOpenConfirmation(
      uint remoteChannelNumber,
      uint initialWindowSize,
      uint maximumPacketSize)
    {
      base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);
      this._channelOpenResponseWaitHandle.Set();
    }

    protected override void OnOpenFailure(uint reasonCode, string description, string language)
    {
      ++this._failedOpenAttempts;
      this.ReleaseSemaphore();
      this._channelOpenResponseWaitHandle.Set();
    }

    protected override void Close()
    {
      base.Close();
      this.ReleaseSemaphore();
    }

    public bool SendPseudoTerminalRequest(
      string environmentVariable,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModeValues)
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new PseudoTerminalRequestInfo(environmentVariable, columns, rows, width, height, terminalModeValues)));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendX11ForwardingRequest(
      bool isSingleConnection,
      string protocol,
      byte[] cookie,
      uint screenNumber)
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new X11ForwardingRequestInfo(isSingleConnection, protocol, cookie, screenNumber)));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendEnvironmentVariableRequest(string variableName, string variableValue)
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new EnvironmentVariableRequestInfo(variableName, variableValue)));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendShellRequest()
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new ShellRequestInfo()));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendExecRequest(string command)
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new ExecRequestInfo(command, this.ConnectionInfo.Encoding)));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendBreakRequest(uint breakLength)
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new BreakRequestInfo(breakLength)));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendSubsystemRequest(string subsystem)
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new SubsystemRequestInfo(subsystem)));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height)
    {
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new WindowChangeRequestInfo(columns, rows, width, height)));
      return true;
    }

    public bool SendLocalFlowRequest(bool clientCanDo)
    {
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new XonXoffRequestInfo(clientCanDo)));
      return true;
    }

    public bool SendSignalRequest(string signalName)
    {
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new SignalRequestInfo(signalName)));
      return true;
    }

    public bool SendExitStatusRequest(uint exitStatus)
    {
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new ExitStatusRequestInfo(exitStatus)));
      return true;
    }

    public bool SendExitSignalRequest(
      string signalName,
      bool coreDumped,
      string errorMessage,
      string language)
    {
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new ExitSignalRequestInfo(signalName, coreDumped, errorMessage, language)));
      return true;
    }

    public bool SendEndOfWriteRequest()
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new EndOfWriteRequestInfo()));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    public bool SendKeepAliveRequest()
    {
      this._channelRequestResponse.Reset();
      this.SendMessage((Message) new ChannelRequestMessage(this.RemoteChannelNumber, (RequestInfo) new KeepAliveRequestInfo()));
      this.WaitOnHandle((WaitHandle) this._channelRequestResponse);
      return this._channelRequestSucces;
    }

    protected override void OnSuccess()
    {
      base.OnSuccess();
      this._channelRequestSucces = true;
      this._channelRequestResponse?.Set();
    }

    protected override void OnFailure()
    {
      base.OnFailure();
      this._channelRequestSucces = false;
      this._channelRequestResponse?.Set();
    }

    private void SendChannelOpenMessage()
    {
      if (Interlocked.CompareExchange(ref this._sessionSemaphoreObtained, 1, 0) != 0)
        return;
      this.SessionSemaphore.Wait();
      this.SendMessage(new ChannelOpenMessage(this.LocalChannelNumber, this.LocalWindowSize, this.LocalPacketSize, (ChannelOpenInfo) new SessionChannelOpenInfo()));
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      EventWaitHandle responseWaitHandle = this._channelOpenResponseWaitHandle;
      if (responseWaitHandle != null)
      {
        this._channelOpenResponseWaitHandle = (EventWaitHandle) null;
        responseWaitHandle.Dispose();
      }
      EventWaitHandle channelRequestResponse = this._channelRequestResponse;
      if (channelRequestResponse != null)
      {
        this._channelRequestResponse = (EventWaitHandle) null;
        channelRequestResponse.Dispose();
      }
    }

    private void ReleaseSemaphore()
    {
      if (Interlocked.CompareExchange(ref this._sessionSemaphoreObtained, 0, 1) != 1)
        return;
      this.SessionSemaphore.Release();
    }
  }
}
