// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Channels.IChannelSession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;

namespace Renci.SshNet.Channels
{
  internal interface IChannelSession : IChannel, IDisposable
  {
    void Open();

    bool SendPseudoTerminalRequest(
      string environmentVariable,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModeValues);

    bool SendX11ForwardingRequest(
      bool isSingleConnection,
      string protocol,
      byte[] cookie,
      uint screenNumber);

    bool SendEnvironmentVariableRequest(string variableName, string variableValue);

    bool SendShellRequest();

    bool SendExecRequest(string command);

    bool SendBreakRequest(uint breakLength);

    bool SendSubsystemRequest(string subsystem);

    bool SendWindowChangeRequest(uint columns, uint rows, uint width, uint height);

    bool SendLocalFlowRequest(bool clientCanDo);

    bool SendSignalRequest(string signalName);

    bool SendExitStatusRequest(uint exitStatus);

    bool SendExitSignalRequest(
      string signalName,
      bool coreDumped,
      string errorMessage,
      string language);

    bool SendEndOfWriteRequest();

    bool SendKeepAliveRequest();
  }
}
