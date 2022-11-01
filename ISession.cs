// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ISession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using System;
using System.Threading;

namespace Renci.SshNet
{
  internal interface ISession : IDisposable
  {
    IConnectionInfo ConnectionInfo { get; }

    bool IsConnected { get; }

    SemaphoreLight SessionSemaphore { get; }

    WaitHandle MessageListenerCompleted { get; }

    void Connect();

    IChannelSession CreateChannelSession();

    IChannelDirectTcpip CreateChannelDirectTcpip();

    IChannelForwardedTcpip CreateChannelForwardedTcpip(
      uint remoteChannelNumber,
      uint remoteWindowSize,
      uint remoteChannelDataPacketSize);

    void Disconnect();

    void OnDisconnecting();

    void RegisterMessage(string messageName);

    void SendMessage(Message message);

    bool TrySendMessage(Message message);

    void UnRegisterMessage(string messageName);

    void WaitOnHandle(WaitHandle waitHandle);

    void WaitOnHandle(WaitHandle waitHandle, TimeSpan timeout);

    WaitResult TryWait(WaitHandle waitHandle, TimeSpan timeout, out Exception exception);

    WaitResult TryWait(WaitHandle waitHandle, TimeSpan timeout);

    event EventHandler<MessageEventArgs<ChannelCloseMessage>> ChannelCloseReceived;

    event EventHandler<MessageEventArgs<ChannelDataMessage>> ChannelDataReceived;

    event EventHandler<MessageEventArgs<ChannelEofMessage>> ChannelEofReceived;

    event EventHandler<MessageEventArgs<ChannelExtendedDataMessage>> ChannelExtendedDataReceived;

    event EventHandler<MessageEventArgs<ChannelFailureMessage>> ChannelFailureReceived;

    event EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>> ChannelOpenConfirmationReceived;

    event EventHandler<MessageEventArgs<ChannelOpenFailureMessage>> ChannelOpenFailureReceived;

    event EventHandler<MessageEventArgs<ChannelOpenMessage>> ChannelOpenReceived;

    event EventHandler<MessageEventArgs<ChannelRequestMessage>> ChannelRequestReceived;

    event EventHandler<MessageEventArgs<ChannelSuccessMessage>> ChannelSuccessReceived;

    event EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> ChannelWindowAdjustReceived;

    event EventHandler<EventArgs> Disconnected;

    event EventHandler<ExceptionEventArgs> ErrorOccured;

    event EventHandler<HostKeyEventArgs> HostKeyReceived;

    event EventHandler<MessageEventArgs<RequestSuccessMessage>> RequestSuccessReceived;

    event EventHandler<MessageEventArgs<RequestFailureMessage>> RequestFailureReceived;

    event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;
  }
}
