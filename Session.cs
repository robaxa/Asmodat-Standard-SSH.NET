// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Session
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Renci.SshNet
{
  public class Session : ISession, IDisposable
  {
    private const byte Null = 0;
    private const byte CarriageReturn = 13;
    internal const byte LineFeed = 10;
    internal static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);
    internal static readonly int Infinite = -1;
    private const int MaximumSshPacketSize = 68536;
    private const int InitialLocalWindowSize = 2147483647;
    private const int LocalChannelDataPacketSize = 65536;
    private static readonly Regex ServerVersionRe = new Regex("^SSH-(?<protoversion>[^-]+)-(?<softwareversion>.+)( SP.+)?$", RegexOptions.Compiled);
    private static readonly SemaphoreLight AuthenticationConnection = new SemaphoreLight(3);
    private SshMessageFactory _sshMessageFactory;
    private EventWaitHandle _messageListenerCompleted;
    private volatile uint _outboundPacketSequence;
    private uint _inboundPacketSequence;
    private EventWaitHandle _serviceAccepted = (EventWaitHandle) new AutoResetEvent(false);
    private EventWaitHandle _exceptionWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private EventWaitHandle _keyExchangeCompletedWaitHandle = (EventWaitHandle) new ManualResetEvent(false);
    private bool _keyExchangeInProgress;
    private Exception _exception;
    private bool _isAuthenticated;
    private bool _isDisconnecting;
    private IKeyExchange _keyExchange;
    private HashAlgorithm _serverMac;
    private HashAlgorithm _clientMac;
    private Cipher _clientCipher;
    private Cipher _serverCipher;
    private Compressor _serverDecompression;
    private Compressor _clientCompression;
    private SemaphoreLight _sessionSemaphore;
    private readonly IServiceFactory _serviceFactory;
    private Socket _socket;
    private readonly object _socketReadLock = new object();
    private readonly object _socketWriteLock = new object();
    private readonly object _socketDisposeLock = new object();
    private bool _isDisconnectMessageSent;
    private uint _nextChannelNumber;
    private Message _clientInitMessage;
    private bool _disposed;

    public SemaphoreLight SessionSemaphore
    {
      get
      {
        if (this._sessionSemaphore == null)
        {
          lock (this)
          {
            if (this._sessionSemaphore == null)
              this._sessionSemaphore = new SemaphoreLight(this.ConnectionInfo.MaxSessions);
          }
        }
        return this._sessionSemaphore;
      }
    }

    private uint NextChannelNumber
    {
      get
      {
        uint nextChannelNumber;
        lock (this)
          nextChannelNumber = this._nextChannelNumber++;
        return nextChannelNumber;
      }
    }

    public bool IsConnected => !this._disposed && !this._isDisconnectMessageSent && this._isAuthenticated && this._messageListenerCompleted != null && !this._messageListenerCompleted.WaitOne(0) && this.IsSocketConnected();

    public byte[] SessionId { get; private set; }

    public Message ClientInitMessage
    {
      get
      {
        if (this._clientInitMessage == null)
          this._clientInitMessage = (Message) new KeyExchangeInitMessage()
          {
            KeyExchangeAlgorithms = this.ConnectionInfo.KeyExchangeAlgorithms.Keys.ToArray<string>(),
            ServerHostKeyAlgorithms = this.ConnectionInfo.HostKeyAlgorithms.Keys.ToArray<string>(),
            EncryptionAlgorithmsClientToServer = this.ConnectionInfo.Encryptions.Keys.ToArray<string>(),
            EncryptionAlgorithmsServerToClient = this.ConnectionInfo.Encryptions.Keys.ToArray<string>(),
            MacAlgorithmsClientToServer = this.ConnectionInfo.HmacAlgorithms.Keys.ToArray<string>(),
            MacAlgorithmsServerToClient = this.ConnectionInfo.HmacAlgorithms.Keys.ToArray<string>(),
            CompressionAlgorithmsClientToServer = this.ConnectionInfo.CompressionAlgorithms.Keys.ToArray<string>(),
            CompressionAlgorithmsServerToClient = this.ConnectionInfo.CompressionAlgorithms.Keys.ToArray<string>(),
            LanguagesClientToServer = new string[1]
            {
              string.Empty
            },
            LanguagesServerToClient = new string[1]
            {
              string.Empty
            },
            FirstKexPacketFollows = false,
            Reserved = 0U
          };
        return this._clientInitMessage;
      }
    }

    public string ServerVersion { get; private set; }

    public string ClientVersion { get; private set; }

    public ConnectionInfo ConnectionInfo { get; private set; }

    public event EventHandler<ExceptionEventArgs> ErrorOccured;

    public event EventHandler<EventArgs> Disconnected;

    public event EventHandler<HostKeyEventArgs> HostKeyReceived;

    public event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;

    internal event EventHandler<MessageEventArgs<InformationRequestMessage>> UserAuthenticationInformationRequestReceived;

    internal event EventHandler<MessageEventArgs<PasswordChangeRequiredMessage>> UserAuthenticationPasswordChangeRequiredReceived;

    internal event EventHandler<MessageEventArgs<PublicKeyMessage>> UserAuthenticationPublicKeyReceived;

    internal event EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeGroup>> KeyExchangeDhGroupExchangeGroupReceived;

    internal event EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeReply>> KeyExchangeDhGroupExchangeReplyReceived;

    internal event EventHandler<MessageEventArgs<DisconnectMessage>> DisconnectReceived;

    internal event EventHandler<MessageEventArgs<IgnoreMessage>> IgnoreReceived;

    internal event EventHandler<MessageEventArgs<UnimplementedMessage>> UnimplementedReceived;

    internal event EventHandler<MessageEventArgs<DebugMessage>> DebugReceived;

    internal event EventHandler<MessageEventArgs<ServiceRequestMessage>> ServiceRequestReceived;

    internal event EventHandler<MessageEventArgs<ServiceAcceptMessage>> ServiceAcceptReceived;

    internal event EventHandler<MessageEventArgs<KeyExchangeInitMessage>> KeyExchangeInitReceived;

    internal event EventHandler<MessageEventArgs<KeyExchangeDhReplyMessage>> KeyExchangeDhReplyMessageReceived;

    internal event EventHandler<MessageEventArgs<NewKeysMessage>> NewKeysReceived;

    internal event EventHandler<MessageEventArgs<RequestMessage>> UserAuthenticationRequestReceived;

    internal event EventHandler<MessageEventArgs<FailureMessage>> UserAuthenticationFailureReceived;

    internal event EventHandler<MessageEventArgs<SuccessMessage>> UserAuthenticationSuccessReceived;

    internal event EventHandler<MessageEventArgs<GlobalRequestMessage>> GlobalRequestReceived;

    public event EventHandler<MessageEventArgs<RequestSuccessMessage>> RequestSuccessReceived;

    public event EventHandler<MessageEventArgs<RequestFailureMessage>> RequestFailureReceived;

    public event EventHandler<MessageEventArgs<ChannelOpenMessage>> ChannelOpenReceived;

    public event EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>> ChannelOpenConfirmationReceived;

    public event EventHandler<MessageEventArgs<ChannelOpenFailureMessage>> ChannelOpenFailureReceived;

    public event EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> ChannelWindowAdjustReceived;

    public event EventHandler<MessageEventArgs<ChannelDataMessage>> ChannelDataReceived;

    public event EventHandler<MessageEventArgs<ChannelExtendedDataMessage>> ChannelExtendedDataReceived;

    public event EventHandler<MessageEventArgs<ChannelEofMessage>> ChannelEofReceived;

    public event EventHandler<MessageEventArgs<ChannelCloseMessage>> ChannelCloseReceived;

    public event EventHandler<MessageEventArgs<ChannelRequestMessage>> ChannelRequestReceived;

    public event EventHandler<MessageEventArgs<ChannelSuccessMessage>> ChannelSuccessReceived;

    public event EventHandler<MessageEventArgs<ChannelFailureMessage>> ChannelFailureReceived;

    internal Session(ConnectionInfo connectionInfo, IServiceFactory serviceFactory)
    {
      if (connectionInfo == null)
        throw new ArgumentNullException(nameof (connectionInfo));
      if (serviceFactory == null)
        throw new ArgumentNullException(nameof (serviceFactory));
      this.ClientVersion = "SSH-2.0-Renci.SshNet.SshClient.0.0.1";
      this.ConnectionInfo = connectionInfo;
      this._serviceFactory = serviceFactory;
      this._messageListenerCompleted = (EventWaitHandle) new ManualResetEvent(true);
    }

    public void Connect()
    {
      if (this.IsConnected)
        return;
      try
      {
        Session.AuthenticationConnection.Wait();
        if (this.IsConnected)
          return;
        lock (this)
        {
          if (this.IsConnected)
            return;
          this.Reset();
          this._sshMessageFactory = new SshMessageFactory();
          switch (this.ConnectionInfo.ProxyType)
          {
            case ProxyTypes.None:
              this.SocketConnect(this.ConnectionInfo.Host, this.ConnectionInfo.Port);
              break;
            case ProxyTypes.Socks4:
              this.SocketConnect(this.ConnectionInfo.ProxyHost, this.ConnectionInfo.ProxyPort);
              this.ConnectSocks4();
              break;
            case ProxyTypes.Socks5:
              this.SocketConnect(this.ConnectionInfo.ProxyHost, this.ConnectionInfo.ProxyPort);
              this.ConnectSocks5();
              break;
            case ProxyTypes.Http:
              this.SocketConnect(this.ConnectionInfo.ProxyHost, this.ConnectionInfo.ProxyPort);
              this.ConnectHttp();
              break;
          }
          string input;
          Match match;
          do
          {
            input = this.SocketReadLine(this.ConnectionInfo.Timeout);
            if (input != null)
              match = Session.ServerVersionRe.Match(input);
            else
              goto label_12;
          }
          while (!match.Success);
          goto label_14;
label_12:
          throw new SshConnectionException("Server response does not contain SSH protocol identification.", DisconnectReason.ProtocolError);
label_14:
          this.ServerVersion = input;
          this.ConnectionInfo.ServerVersion = this.ServerVersion;
          this.ConnectionInfo.ClientVersion = this.ClientVersion;
          string str1 = match.Result("${protoversion}");
          string str2 = match.Result("${softwareversion}");
          DiagnosticAbstraction.Log(string.Format("Server version '{0}' on '{1}'.", (object) str1, (object) str2));
          if (!str1.Equals("2.0") && !str1.Equals("1.99"))
            throw new SshConnectionException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Server version '{0}' is not supported.", (object) str1), DisconnectReason.ProtocolVersionNotSupported);
          SocketAbstraction.Send(this._socket, Encoding.UTF8.GetBytes(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "{0}\r\n", (object) this.ClientVersion)));
          this.RegisterMessage("SSH_MSG_DISCONNECT");
          this.RegisterMessage("SSH_MSG_IGNORE");
          this.RegisterMessage("SSH_MSG_UNIMPLEMENTED");
          this.RegisterMessage("SSH_MSG_DEBUG");
          this.RegisterMessage("SSH_MSG_SERVICE_ACCEPT");
          this.RegisterMessage("SSH_MSG_KEXINIT");
          this.RegisterMessage("SSH_MSG_NEWKEYS");
          this.RegisterMessage("SSH_MSG_USERAUTH_BANNER");
          this._messageListenerCompleted.Reset();
          ThreadAbstraction.ExecuteThread(new Action(this.MessageListener));
          this.WaitOnHandle((WaitHandle) this._keyExchangeCompletedWaitHandle);
          if (this.SessionId == null)
          {
            this.Disconnect();
          }
          else
          {
            this.SendMessage((Message) new ServiceRequestMessage(ServiceName.UserAuthentication));
            this.WaitOnHandle((WaitHandle) this._serviceAccepted);
            if (string.IsNullOrEmpty(this.ConnectionInfo.Username))
              throw new SshException("Username is not specified.");
            this.RegisterMessage("SSH_MSG_GLOBAL_REQUEST");
            this.ConnectionInfo.Authenticate((ISession) this, this._serviceFactory);
            this._isAuthenticated = true;
            this.RegisterMessage("SSH_MSG_REQUEST_SUCCESS");
            this.RegisterMessage("SSH_MSG_REQUEST_FAILURE");
            this.RegisterMessage("SSH_MSG_CHANNEL_OPEN_CONFIRMATION");
            this.RegisterMessage("SSH_MSG_CHANNEL_OPEN_FAILURE");
            this.RegisterMessage("SSH_MSG_CHANNEL_WINDOW_ADJUST");
            this.RegisterMessage("SSH_MSG_CHANNEL_EXTENDED_DATA");
            this.RegisterMessage("SSH_MSG_CHANNEL_REQUEST");
            this.RegisterMessage("SSH_MSG_CHANNEL_SUCCESS");
            this.RegisterMessage("SSH_MSG_CHANNEL_FAILURE");
            this.RegisterMessage("SSH_MSG_CHANNEL_DATA");
            this.RegisterMessage("SSH_MSG_CHANNEL_EOF");
            this.RegisterMessage("SSH_MSG_CHANNEL_CLOSE");
          }
        }
      }
      finally
      {
        Session.AuthenticationConnection.Release();
      }
    }

    public void Disconnect()
    {
      DiagnosticAbstraction.Log(string.Format("[{0}] Disconnecting session.", (object) Session.ToHex(this.SessionId)));
      this.Disconnect(DisconnectReason.ByApplication, "Connection terminated by the client.");
      if (this._messageListenerCompleted == null)
        return;
      this._messageListenerCompleted.WaitOne();
    }

    private void Disconnect(DisconnectReason reason, string message)
    {
      this._isDisconnecting = true;
      if (this.IsConnected)
        this.TrySendDisconnect(reason, message);
      this.SocketDisconnectAndDispose();
    }

    void ISession.WaitOnHandle(WaitHandle waitHandle) => this.WaitOnHandle(waitHandle, this.ConnectionInfo.Timeout);

    void ISession.WaitOnHandle(WaitHandle waitHandle, TimeSpan timeout) => this.WaitOnHandle(waitHandle, timeout);

    internal void WaitOnHandle(WaitHandle waitHandle) => this.WaitOnHandle(waitHandle, this.ConnectionInfo.Timeout);

    WaitResult ISession.TryWait(WaitHandle waitHandle, TimeSpan timeout) => this.TryWait(waitHandle, timeout, out Exception _);

    WaitResult ISession.TryWait(
      WaitHandle waitHandle,
      TimeSpan timeout,
      out Exception exception)
    {
      return this.TryWait(waitHandle, timeout, out exception);
    }

    private WaitResult TryWait(
      WaitHandle waitHandle,
      TimeSpan timeout,
      out Exception exception)
    {
      if (waitHandle == null)
        throw new ArgumentNullException(nameof (waitHandle));
      switch (WaitHandle.WaitAny(new WaitHandle[3]
      {
        (WaitHandle) this._exceptionWaitHandle,
        (WaitHandle) this._messageListenerCompleted,
        waitHandle
      }, timeout))
      {
        case 0:
          if (this._exception is SshConnectionException)
          {
            exception = (Exception) null;
            return WaitResult.Disconnected;
          }
          exception = this._exception;
          return WaitResult.Failed;
        case 1:
          exception = (Exception) null;
          return WaitResult.Disconnected;
        case 2:
          exception = (Exception) null;
          return WaitResult.Success;
        case 258:
          exception = (Exception) null;
          return WaitResult.TimedOut;
        default:
          throw new InvalidOperationException("Unexpected result.");
      }
    }

    internal void WaitOnHandle(WaitHandle waitHandle, TimeSpan timeout)
    {
      if (waitHandle == null)
        throw new ArgumentNullException(nameof (waitHandle));
      switch (WaitHandle.WaitAny(new WaitHandle[3]
      {
        (WaitHandle) this._exceptionWaitHandle,
        (WaitHandle) this._messageListenerCompleted,
        waitHandle
      }, timeout))
      {
        case 0:
          throw this._exception;
        case 1:
          throw new SshConnectionException("Client not connected.");
        case 258:
          if (this._isDisconnecting)
            break;
          throw new SshOperationTimeoutException("Session operation has timed out");
      }
    }

    internal void SendMessage(Message message)
    {
      if (!this._socket.CanWrite())
        throw new SshConnectionException("Client not connected.");
      if (this._keyExchangeInProgress && !(message is IKeyExchangedAllowed))
        this.WaitOnHandle((WaitHandle) this._keyExchangeCompletedWaitHandle);
      DiagnosticAbstraction.Log(string.Format("[{0}] Sending message '{1}' to server: '{2}'.", (object) Session.ToHex(this.SessionId), (object) message.GetType().Name, (object) message));
      byte paddingMultiplier = this._clientCipher == null ? (byte) 8 : Math.Max((byte) 8, this._serverCipher.MinimumSize);
      byte[] numArray1 = message.GetPacket(paddingMultiplier, this._clientCompression);
      lock (this._socketWriteLock)
      {
        byte[] src = (byte[]) null;
        int num1 = 4;
        if (this._clientMac != null)
        {
          Pack.UInt32ToBigEndian(this._outboundPacketSequence, numArray1);
          src = this._clientMac.ComputeHash(numArray1);
        }
        if (this._clientCipher != null)
        {
          numArray1 = this._clientCipher.Encrypt(numArray1, num1, numArray1.Length - num1);
          num1 = 0;
        }
        if (numArray1.Length > 68536)
          throw new InvalidOperationException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Packet is too big. Maximum packet size is {0} bytes.", (object) 68536));
        int num2 = numArray1.Length - num1;
        if (src == null)
        {
          this.SendPacket(numArray1, num1, num2);
        }
        else
        {
          byte[] numArray2 = new byte[num2 + src.Length];
          Buffer.BlockCopy((Array) numArray1, num1, (Array) numArray2, 0, num2);
          Buffer.BlockCopy((Array) src, 0, (Array) numArray2, num2, src.Length);
          this.SendPacket(numArray2, 0, numArray2.Length);
        }
        ++this._outboundPacketSequence;
      }
    }

    private void SendPacket(byte[] packet, int offset, int length)
    {
      lock (this._socketDisposeLock)
      {
        if (!this._socket.IsConnected())
          throw new SshConnectionException("Client not connected.");
        SocketAbstraction.Send(this._socket, packet, offset, length);
      }
    }

    private bool TrySendMessage(Message message)
    {
      try
      {
        this.SendMessage(message);
        return true;
      }
      catch (SshException ex)
      {
        DiagnosticAbstraction.Log(string.Format("Failure sending message '{0}' to server: '{1}' => {2}", (object) message.GetType().Name, (object) message, (object) ex));
        return false;
      }
      catch (SocketException ex)
      {
        DiagnosticAbstraction.Log(string.Format("Failure sending message '{0}' to server: '{1}' => {2}", (object) message.GetType().Name, (object) message, (object) ex));
        return false;
      }
    }

    private Message ReceiveMessage()
    {
      byte length1 = this._serverCipher == null ? (byte) 8 : Math.Max((byte) 8, this._serverCipher.MinimumSize);
      int count = this._serverMac != null ? this._serverMac.HashSize / 8 : 0;
      uint uint32;
      byte[] numArray1;
      lock (this._socketReadLock)
      {
        byte[] numArray2 = new byte[(int) length1];
        if (this.TrySocketRead(numArray2, 0, (int) length1) == 0)
          return (Message) null;
        if (this._serverCipher != null)
          numArray2 = this._serverCipher.Decrypt(numArray2);
        uint32 = Pack.BigEndianToUInt32(numArray2);
        if ((long) uint32 < (long) ((int) Math.Max((byte) 16, length1) - 4) || uint32 > 68532U)
          throw new SshConnectionException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Bad packet length: {0}.", (object) uint32), DisconnectReason.ProtocolError);
        int length2 = (int) ((long) uint32 - (long) ((int) length1 - 4)) + count;
        numArray1 = new byte[length2 + (int) length1 + 4];
        Pack.UInt32ToBigEndian(this._inboundPacketSequence, numArray1);
        Buffer.BlockCopy((Array) numArray2, 0, (Array) numArray1, 4, numArray2.Length);
        if (length2 > 0)
        {
          if (this.TrySocketRead(numArray1, (int) length1 + 4, length2) == 0)
            return (Message) null;
        }
      }
      if (this._serverCipher != null)
      {
        int length3 = numArray1.Length - ((int) length1 + 4 + count);
        if (length3 > 0)
        {
          byte[] src = this._serverCipher.Decrypt(numArray1, (int) length1 + 4, length3);
          Buffer.BlockCopy((Array) src, 0, (Array) numArray1, (int) length1 + 4, src.Length);
        }
      }
      byte num1 = numArray1[8];
      int num2 = (int) uint32 - (int) num1 - 1;
      int offset = 9;
      if (this._serverMac != null)
      {
        byte[] hash = this._serverMac.ComputeHash(numArray1, 0, numArray1.Length - count);
        if (!numArray1.Take(numArray1.Length - count, count).IsEqualTo(hash))
          throw new SshConnectionException("MAC error", DisconnectReason.MacError);
      }
      if (this._serverDecompression != null)
      {
        numArray1 = this._serverDecompression.Decompress(numArray1, offset, num2);
        offset = 0;
        num2 = numArray1.Length;
      }
      ++this._inboundPacketSequence;
      return this.LoadMessage(numArray1, offset, num2);
    }

    private void TrySendDisconnect(DisconnectReason reasonCode, string message)
    {
      this.TrySendMessage((Message) new DisconnectMessage(reasonCode, message));
      this._isDisconnectMessageSent = true;
    }

    internal void OnDisconnectReceived(DisconnectMessage message)
    {
      DiagnosticAbstraction.Log(string.Format("[{0}] Disconnect received: {1} {2}.", (object) Session.ToHex(this.SessionId), (object) message.ReasonCode, (object) message.Description));
      this._isDisconnecting = true;
      this._exception = (Exception) new SshConnectionException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "The connection was closed by the server: {0} ({1}).", (object) message.Description, (object) message.ReasonCode), message.ReasonCode);
      this._exceptionWaitHandle.Set();
      EventHandler<MessageEventArgs<DisconnectMessage>> disconnectReceived = this.DisconnectReceived;
      if (disconnectReceived != null)
        disconnectReceived((object) this, new MessageEventArgs<DisconnectMessage>(message));
      EventHandler<EventArgs> disconnected = this.Disconnected;
      if (disconnected != null)
        disconnected((object) this, new EventArgs());
      this.SocketDisconnectAndDispose();
    }

    internal void OnIgnoreReceived(IgnoreMessage message)
    {
      EventHandler<MessageEventArgs<IgnoreMessage>> ignoreReceived = this.IgnoreReceived;
      if (ignoreReceived == null)
        return;
      ignoreReceived((object) this, new MessageEventArgs<IgnoreMessage>(message));
    }

    internal void OnUnimplementedReceived(UnimplementedMessage message)
    {
      EventHandler<MessageEventArgs<UnimplementedMessage>> unimplementedReceived = this.UnimplementedReceived;
      if (unimplementedReceived == null)
        return;
      unimplementedReceived((object) this, new MessageEventArgs<UnimplementedMessage>(message));
    }

    internal void OnDebugReceived(DebugMessage message)
    {
      EventHandler<MessageEventArgs<DebugMessage>> debugReceived = this.DebugReceived;
      if (debugReceived == null)
        return;
      debugReceived((object) this, new MessageEventArgs<DebugMessage>(message));
    }

    internal void OnServiceRequestReceived(ServiceRequestMessage message)
    {
      EventHandler<MessageEventArgs<ServiceRequestMessage>> serviceRequestReceived = this.ServiceRequestReceived;
      if (serviceRequestReceived == null)
        return;
      serviceRequestReceived((object) this, new MessageEventArgs<ServiceRequestMessage>(message));
    }

    internal void OnServiceAcceptReceived(ServiceAcceptMessage message)
    {
      EventHandler<MessageEventArgs<ServiceAcceptMessage>> serviceAcceptReceived = this.ServiceAcceptReceived;
      if (serviceAcceptReceived != null)
        serviceAcceptReceived((object) this, new MessageEventArgs<ServiceAcceptMessage>(message));
      this._serviceAccepted.Set();
    }

    internal void OnKeyExchangeDhGroupExchangeGroupReceived(KeyExchangeDhGroupExchangeGroup message)
    {
      EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeGroup>> exchangeGroupReceived = this.KeyExchangeDhGroupExchangeGroupReceived;
      if (exchangeGroupReceived == null)
        return;
      exchangeGroupReceived((object) this, new MessageEventArgs<KeyExchangeDhGroupExchangeGroup>(message));
    }

    internal void OnKeyExchangeDhGroupExchangeReplyReceived(KeyExchangeDhGroupExchangeReply message)
    {
      EventHandler<MessageEventArgs<KeyExchangeDhGroupExchangeReply>> exchangeReplyReceived = this.KeyExchangeDhGroupExchangeReplyReceived;
      if (exchangeReplyReceived == null)
        return;
      exchangeReplyReceived((object) this, new MessageEventArgs<KeyExchangeDhGroupExchangeReply>(message));
    }

    internal void OnKeyExchangeInitReceived(KeyExchangeInitMessage message)
    {
      this._keyExchangeInProgress = true;
      this._keyExchangeCompletedWaitHandle.Reset();
      this._sshMessageFactory.DisableNonKeyExchangeMessages();
      this._keyExchange = this._serviceFactory.CreateKeyExchange(this.ConnectionInfo.KeyExchangeAlgorithms, message.KeyExchangeAlgorithms);
      this.ConnectionInfo.CurrentKeyExchangeAlgorithm = this._keyExchange.Name;
      this._keyExchange.HostKeyReceived += new EventHandler<HostKeyEventArgs>(this.KeyExchange_HostKeyReceived);
      this._keyExchange.Start(this, message);
      EventHandler<MessageEventArgs<KeyExchangeInitMessage>> exchangeInitReceived = this.KeyExchangeInitReceived;
      if (exchangeInitReceived == null)
        return;
      exchangeInitReceived((object) this, new MessageEventArgs<KeyExchangeInitMessage>(message));
    }

    internal void OnKeyExchangeDhReplyMessageReceived(KeyExchangeDhReplyMessage message)
    {
      EventHandler<MessageEventArgs<KeyExchangeDhReplyMessage>> replyMessageReceived = this.KeyExchangeDhReplyMessageReceived;
      if (replyMessageReceived == null)
        return;
      replyMessageReceived((object) this, new MessageEventArgs<KeyExchangeDhReplyMessage>(message));
    }

    internal void OnNewKeysReceived(NewKeysMessage message)
    {
      if (this.SessionId == null)
        this.SessionId = this._keyExchange.ExchangeHash;
      if (this._serverMac != null)
      {
        this._serverMac.Dispose();
        this._serverMac = (HashAlgorithm) null;
      }
      if (this._clientMac != null)
      {
        this._clientMac.Dispose();
        this._clientMac = (HashAlgorithm) null;
      }
      this._serverCipher = this._keyExchange.CreateServerCipher();
      this._clientCipher = this._keyExchange.CreateClientCipher();
      this._serverMac = this._keyExchange.CreateServerHash();
      this._clientMac = this._keyExchange.CreateClientHash();
      this._clientCompression = this._keyExchange.CreateCompressor();
      this._serverDecompression = this._keyExchange.CreateDecompressor();
      if (this._keyExchange != null)
      {
        this._keyExchange.HostKeyReceived -= new EventHandler<HostKeyEventArgs>(this.KeyExchange_HostKeyReceived);
        this._keyExchange.Dispose();
        this._keyExchange = (IKeyExchange) null;
      }
      this._sshMessageFactory.EnableActivatedMessages();
      EventHandler<MessageEventArgs<NewKeysMessage>> newKeysReceived = this.NewKeysReceived;
      if (newKeysReceived != null)
        newKeysReceived((object) this, new MessageEventArgs<NewKeysMessage>(message));
      this._keyExchangeCompletedWaitHandle.Set();
      this._keyExchangeInProgress = false;
    }

    void ISession.OnDisconnecting() => this._isDisconnecting = true;

    internal void OnUserAuthenticationRequestReceived(RequestMessage message)
    {
      EventHandler<MessageEventArgs<RequestMessage>> authenticationRequestReceived = this.UserAuthenticationRequestReceived;
      if (authenticationRequestReceived == null)
        return;
      authenticationRequestReceived((object) this, new MessageEventArgs<RequestMessage>(message));
    }

    internal void OnUserAuthenticationFailureReceived(FailureMessage message)
    {
      EventHandler<MessageEventArgs<FailureMessage>> authenticationFailureReceived = this.UserAuthenticationFailureReceived;
      if (authenticationFailureReceived == null)
        return;
      authenticationFailureReceived((object) this, new MessageEventArgs<FailureMessage>(message));
    }

    internal void OnUserAuthenticationSuccessReceived(SuccessMessage message)
    {
      EventHandler<MessageEventArgs<SuccessMessage>> authenticationSuccessReceived = this.UserAuthenticationSuccessReceived;
      if (authenticationSuccessReceived == null)
        return;
      authenticationSuccessReceived((object) this, new MessageEventArgs<SuccessMessage>(message));
    }

    internal void OnUserAuthenticationBannerReceived(BannerMessage message)
    {
      EventHandler<MessageEventArgs<BannerMessage>> authenticationBannerReceived = this.UserAuthenticationBannerReceived;
      if (authenticationBannerReceived == null)
        return;
      authenticationBannerReceived((object) this, new MessageEventArgs<BannerMessage>(message));
    }

    internal void OnUserAuthenticationInformationRequestReceived(InformationRequestMessage message)
    {
      EventHandler<MessageEventArgs<InformationRequestMessage>> informationRequestReceived = this.UserAuthenticationInformationRequestReceived;
      if (informationRequestReceived == null)
        return;
      informationRequestReceived((object) this, new MessageEventArgs<InformationRequestMessage>(message));
    }

    internal void OnUserAuthenticationPasswordChangeRequiredReceived(
      PasswordChangeRequiredMessage message)
    {
      EventHandler<MessageEventArgs<PasswordChangeRequiredMessage>> requiredReceived = this.UserAuthenticationPasswordChangeRequiredReceived;
      if (requiredReceived == null)
        return;
      requiredReceived((object) this, new MessageEventArgs<PasswordChangeRequiredMessage>(message));
    }

    internal void OnUserAuthenticationPublicKeyReceived(PublicKeyMessage message)
    {
      EventHandler<MessageEventArgs<PublicKeyMessage>> publicKeyReceived = this.UserAuthenticationPublicKeyReceived;
      if (publicKeyReceived == null)
        return;
      publicKeyReceived((object) this, new MessageEventArgs<PublicKeyMessage>(message));
    }

    internal void OnGlobalRequestReceived(GlobalRequestMessage message)
    {
      EventHandler<MessageEventArgs<GlobalRequestMessage>> globalRequestReceived = this.GlobalRequestReceived;
      if (globalRequestReceived == null)
        return;
      globalRequestReceived((object) this, new MessageEventArgs<GlobalRequestMessage>(message));
    }

    internal void OnRequestSuccessReceived(RequestSuccessMessage message)
    {
      EventHandler<MessageEventArgs<RequestSuccessMessage>> requestSuccessReceived = this.RequestSuccessReceived;
      if (requestSuccessReceived == null)
        return;
      requestSuccessReceived((object) this, new MessageEventArgs<RequestSuccessMessage>(message));
    }

    internal void OnRequestFailureReceived(RequestFailureMessage message)
    {
      EventHandler<MessageEventArgs<RequestFailureMessage>> requestFailureReceived = this.RequestFailureReceived;
      if (requestFailureReceived == null)
        return;
      requestFailureReceived((object) this, new MessageEventArgs<RequestFailureMessage>(message));
    }

    internal void OnChannelOpenReceived(ChannelOpenMessage message)
    {
      EventHandler<MessageEventArgs<ChannelOpenMessage>> channelOpenReceived = this.ChannelOpenReceived;
      if (channelOpenReceived == null)
        return;
      channelOpenReceived((object) this, new MessageEventArgs<ChannelOpenMessage>(message));
    }

    internal void OnChannelOpenConfirmationReceived(ChannelOpenConfirmationMessage message)
    {
      EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>> confirmationReceived = this.ChannelOpenConfirmationReceived;
      if (confirmationReceived == null)
        return;
      confirmationReceived((object) this, new MessageEventArgs<ChannelOpenConfirmationMessage>(message));
    }

    internal void OnChannelOpenFailureReceived(ChannelOpenFailureMessage message)
    {
      EventHandler<MessageEventArgs<ChannelOpenFailureMessage>> openFailureReceived = this.ChannelOpenFailureReceived;
      if (openFailureReceived == null)
        return;
      openFailureReceived((object) this, new MessageEventArgs<ChannelOpenFailureMessage>(message));
    }

    internal void OnChannelWindowAdjustReceived(ChannelWindowAdjustMessage message)
    {
      EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> windowAdjustReceived = this.ChannelWindowAdjustReceived;
      if (windowAdjustReceived == null)
        return;
      windowAdjustReceived((object) this, new MessageEventArgs<ChannelWindowAdjustMessage>(message));
    }

    internal void OnChannelDataReceived(ChannelDataMessage message)
    {
      EventHandler<MessageEventArgs<ChannelDataMessage>> channelDataReceived = this.ChannelDataReceived;
      if (channelDataReceived == null)
        return;
      channelDataReceived((object) this, new MessageEventArgs<ChannelDataMessage>(message));
    }

    internal void OnChannelExtendedDataReceived(ChannelExtendedDataMessage message)
    {
      EventHandler<MessageEventArgs<ChannelExtendedDataMessage>> extendedDataReceived = this.ChannelExtendedDataReceived;
      if (extendedDataReceived == null)
        return;
      extendedDataReceived((object) this, new MessageEventArgs<ChannelExtendedDataMessage>(message));
    }

    internal void OnChannelEofReceived(ChannelEofMessage message)
    {
      EventHandler<MessageEventArgs<ChannelEofMessage>> channelEofReceived = this.ChannelEofReceived;
      if (channelEofReceived == null)
        return;
      channelEofReceived((object) this, new MessageEventArgs<ChannelEofMessage>(message));
    }

    internal void OnChannelCloseReceived(ChannelCloseMessage message)
    {
      EventHandler<MessageEventArgs<ChannelCloseMessage>> channelCloseReceived = this.ChannelCloseReceived;
      if (channelCloseReceived == null)
        return;
      channelCloseReceived((object) this, new MessageEventArgs<ChannelCloseMessage>(message));
    }

    internal void OnChannelRequestReceived(ChannelRequestMessage message)
    {
      EventHandler<MessageEventArgs<ChannelRequestMessage>> channelRequestReceived = this.ChannelRequestReceived;
      if (channelRequestReceived == null)
        return;
      channelRequestReceived((object) this, new MessageEventArgs<ChannelRequestMessage>(message));
    }

    internal void OnChannelSuccessReceived(ChannelSuccessMessage message)
    {
      EventHandler<MessageEventArgs<ChannelSuccessMessage>> channelSuccessReceived = this.ChannelSuccessReceived;
      if (channelSuccessReceived == null)
        return;
      channelSuccessReceived((object) this, new MessageEventArgs<ChannelSuccessMessage>(message));
    }

    internal void OnChannelFailureReceived(ChannelFailureMessage message)
    {
      EventHandler<MessageEventArgs<ChannelFailureMessage>> channelFailureReceived = this.ChannelFailureReceived;
      if (channelFailureReceived == null)
        return;
      channelFailureReceived((object) this, new MessageEventArgs<ChannelFailureMessage>(message));
    }

    private void KeyExchange_HostKeyReceived(object sender, HostKeyEventArgs e)
    {
      EventHandler<HostKeyEventArgs> hostKeyReceived = this.HostKeyReceived;
      if (hostKeyReceived == null)
        return;
      hostKeyReceived((object) this, e);
    }

    public void RegisterMessage(string messageName) => this._sshMessageFactory.EnableAndActivateMessage(messageName);

    public void UnRegisterMessage(string messageName) => this._sshMessageFactory.DisableAndDeactivateMessage(messageName);

    private Message LoadMessage(byte[] data, int offset, int count)
    {
      Message message = this._sshMessageFactory.Create(data[offset]);
      message.Load(data, offset + 1, count - 1);
      DiagnosticAbstraction.Log(string.Format("[{0}] Received message '{1}' from server: '{2}'.", (object) Session.ToHex(this.SessionId), (object) message.GetType().Name, (object) message));
      return message;
    }

    private static string ToHex(byte[] bytes, int offset)
    {
      int num1 = bytes.Length - offset;
      StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
      for (int index = offset; index < num1; ++index)
      {
        byte num2 = bytes[index];
        stringBuilder.Append(num2.ToString("X2"));
      }
      return stringBuilder.ToString();
    }

    internal static string ToHex(byte[] bytes) => bytes == null ? (string) null : Session.ToHex(bytes, 0);

    private void SocketConnect(string host, int port)
    {
      IPEndPoint remoteEndpoint = new IPEndPoint(DnsAbstraction.GetHostAddresses(host)[0], port);
      DiagnosticAbstraction.Log(string.Format("Initiating connection to '{0}:{1}'.", (object) host, (object) port));
      this._socket = SocketAbstraction.Connect(remoteEndpoint, this.ConnectionInfo.Timeout);
      this._socket.SendBufferSize = 137072;
      this._socket.ReceiveBufferSize = 137072;
    }

    private int SocketRead(byte[] buffer, int offset, int length)
    {
      int num = SocketAbstraction.Read(this._socket, buffer, offset, length, Session.InfiniteTimeSpan);
      return num != 0 ? num : throw new SshConnectionException("An established connection was aborted by the server.", DisconnectReason.ConnectionLost);
    }

    private bool IsSocketConnected()
    {
      lock (this._socketDisposeLock)
      {
        if (!this._socket.IsConnected())
          return false;
        lock (this._socketReadLock)
          return !this._socket.Poll(0, SelectMode.SelectRead) || this._socket.Available != 0;
      }
    }

    private int TrySocketRead(byte[] buffer, int offset, int length) => SocketAbstraction.Read(this._socket, buffer, offset, length, Session.InfiniteTimeSpan);

    private string SocketReadLine(TimeSpan timeout)
    {
      Encoding ascii = SshData.Ascii;
      List<byte> byteList = new List<byte>();
      byte[] buffer = new byte[1];
      while (SocketAbstraction.Read(this._socket, buffer, 0, buffer.Length, timeout) != 0)
      {
        byteList.Add(buffer[0]);
        if (byteList.Count > 0 && (byteList[byteList.Count - 1] == (byte) 10 || byteList[byteList.Count - 1] <= (byte) 0))
          break;
      }
      if (byteList.Count == 0)
        return (string) null;
      if (byteList.Count == 1 && byteList[byteList.Count - 1] == (byte) 0)
        return string.Empty;
      if (byteList.Count > 1 && byteList[byteList.Count - 2] == (byte) 13)
        return ascii.GetString(byteList.ToArray(), 0, byteList.Count - 2);
      return byteList.Count > 1 && byteList[byteList.Count - 1] == (byte) 10 ? ascii.GetString(byteList.ToArray(), 0, byteList.Count - 1) : ascii.GetString(byteList.ToArray(), 0, byteList.Count);
    }

    private void SocketDisconnectAndDispose()
    {
      if (this._socket == null)
        return;
      lock (this._socketDisposeLock)
      {
        if (this._socket != null)
        {
          if (this._socket.Connected)
          {
            try
            {
              DiagnosticAbstraction.Log(string.Format("[{0}] Shutting down socket.", (object) Session.ToHex(this.SessionId)));
              this._socket.Shutdown(SocketShutdown.Send);
            }
            catch (SocketException ex)
            {
              DiagnosticAbstraction.Log("Failure shutting down socket: " + ex?.ToString());
            }
          }
          DiagnosticAbstraction.Log(string.Format("[{0}] Disposing socket.", (object) Session.ToHex(this.SessionId)));
          this._socket.Dispose();
          DiagnosticAbstraction.Log(string.Format("[{0}] Disposed socket.", (object) Session.ToHex(this.SessionId)));
          this._socket = (Socket) null;
        }
      }
    }

    private void MessageListener()
    {
      List<Socket> checkRead = new List<Socket>()
      {
        this._socket
      };
      try
      {
        while (this._socket.IsConnected())
        {
          Socket.Select((IList) checkRead, (IList) null, (IList) null, -1);
          if (this._socket.IsConnected())
          {
            Message message = this.ReceiveMessage();
            if (message != null)
              message.Process(this);
            else
              break;
          }
          else
            break;
        }
        this.RaiseError((Exception) Session.CreateConnectionAbortedByServerException());
      }
      catch (SocketException ex)
      {
        this.RaiseError((Exception) new SshConnectionException(ex.Message, DisconnectReason.ConnectionLost, (Exception) ex));
      }
      catch (Exception ex)
      {
        this.RaiseError(ex);
      }
      finally
      {
        this._messageListenerCompleted.Set();
      }
    }

    private byte SocketReadByte()
    {
      byte[] buffer = new byte[1];
      this.SocketRead(buffer, 0, 1);
      return buffer[0];
    }

    private void ConnectSocks4()
    {
      SocketAbstraction.Send(this._socket, Session.CreateSocks4ConnectionRequest(this.ConnectionInfo.Host, (ushort) this.ConnectionInfo.Port, this.ConnectionInfo.ProxyUsername));
      if (this.SocketReadByte() > (byte) 0)
        throw new ProxyException("SOCKS4: Null is expected.");
      switch (this.SocketReadByte())
      {
        case 90:
          this.SocketRead(new byte[6], 0, 6);
          break;
        case 91:
          throw new ProxyException("SOCKS4: Connection rejected.");
        case 92:
          throw new ProxyException("SOCKS4: Client is not running identd or not reachable from the server.");
        case 93:
          throw new ProxyException("SOCKS4: Client's identd could not confirm the user ID string in the request.");
        default:
          throw new ProxyException("SOCKS4: Not valid response.");
      }
    }

    private void ConnectSocks5()
    {
      SocketAbstraction.Send(this._socket, new byte[4]
      {
        (byte) 5,
        (byte) 2,
        (byte) 0,
        (byte) 2
      });
      byte num1 = this.SocketReadByte();
      if (num1 != (byte) 5)
        throw new ProxyException(string.Format("SOCKS Version '{0}' is not supported.", (object) num1));
      switch (this.SocketReadByte())
      {
        case 2:
          SocketAbstraction.Send(this._socket, Session.CreateSocks5UserNameAndPasswordAuthenticationRequest(this.ConnectionInfo.ProxyUsername, this.ConnectionInfo.ProxyPassword));
          byte[] numArray = SocketAbstraction.Read(this._socket, 2, this.ConnectionInfo.Timeout);
          if (numArray[0] != (byte) 1)
            throw new ProxyException("SOCKS5: Server authentication version is not valid.");
          if (numArray[1] > (byte) 0)
            throw new ProxyException("SOCKS5: Username/Password authentication failed.");
          break;
        case byte.MaxValue:
          throw new ProxyException("SOCKS5: No acceptable authentication methods were offered.");
      }
      SocketAbstraction.Send(this._socket, Session.CreateSocks5ConnectionRequest(this.ConnectionInfo.Host, (ushort) this.ConnectionInfo.Port));
      if (this.SocketReadByte() != (byte) 5)
        throw new ProxyException("SOCKS5: Version 5 is expected.");
      switch (this.SocketReadByte())
      {
        case 0:
          if (this.SocketReadByte() > (byte) 0)
            throw new ProxyException("SOCKS5: 0 byte is expected.");
          byte num2 = this.SocketReadByte();
          switch (num2)
          {
            case 1:
              this.SocketRead(new byte[4], 0, 4);
              break;
            case 4:
              this.SocketRead(new byte[16], 0, 16);
              break;
            default:
              throw new ProxyException(string.Format("Address type '{0}' is not supported.", (object) num2));
          }
          this.SocketRead(new byte[2], 0, 2);
          break;
        case 1:
          throw new ProxyException("SOCKS5: General failure.");
        case 2:
          throw new ProxyException("SOCKS5: Connection not allowed by ruleset.");
        case 3:
          throw new ProxyException("SOCKS5: Network unreachable.");
        case 4:
          throw new ProxyException("SOCKS5: Host unreachable.");
        case 5:
          throw new ProxyException("SOCKS5: Connection refused by destination host.");
        case 6:
          throw new ProxyException("SOCKS5: TTL expired.");
        case 7:
          throw new ProxyException("SOCKS5: Command not supported or protocol error.");
        case 8:
          throw new ProxyException("SOCKS5: Address type not supported.");
        default:
          throw new ProxyException("SOCKS5: Not valid response.");
      }
    }

    private static byte[] CreateSocks5UserNameAndPasswordAuthenticationRequest(
      string username,
      string password)
    {
      if (username.Length > (int) byte.MaxValue)
        throw new ProxyException("Proxy username is too long.");
      if (password.Length > (int) byte.MaxValue)
        throw new ProxyException("Proxy password is too long.");
      byte[] bytes = new byte[2 + username.Length + 1 + password.Length];
      int num1 = 0;
      byte[] numArray1 = bytes;
      int index1 = num1;
      int num2 = index1 + 1;
      numArray1[index1] = (byte) 1;
      byte[] numArray2 = bytes;
      int index2 = num2;
      int byteIndex1 = index2 + 1;
      int length1 = (int) (byte) username.Length;
      numArray2[index2] = (byte) length1;
      SshData.Ascii.GetBytes(username, 0, username.Length, bytes, byteIndex1);
      int num3 = byteIndex1 + username.Length;
      byte[] numArray3 = bytes;
      int index3 = num3;
      int byteIndex2 = index3 + 1;
      int length2 = (int) (byte) password.Length;
      numArray3[index3] = (byte) length2;
      SshData.Ascii.GetBytes(password, 0, password.Length, bytes, byteIndex2);
      return bytes;
    }

    private static byte[] CreateSocks4ConnectionRequest(
      string hostname,
      ushort port,
      string username)
    {
      byte[] destinationAddress = Session.GetSocks4DestinationAddress(hostname);
      byte[] connectionRequest = new byte[4 + destinationAddress.Length + username.Length + 1];
      int num1 = 0;
      byte[] numArray1 = connectionRequest;
      int index1 = num1;
      int num2 = index1 + 1;
      numArray1[index1] = (byte) 4;
      byte[] numArray2 = connectionRequest;
      int index2 = num2;
      int offset = index2 + 1;
      numArray2[index2] = (byte) 1;
      Pack.UInt16ToBigEndian(port, connectionRequest, offset);
      int dstOffset = offset + 2;
      Buffer.BlockCopy((Array) destinationAddress, 0, (Array) connectionRequest, dstOffset, destinationAddress.Length);
      int index3 = dstOffset + destinationAddress.Length;
      connectionRequest[index3] = (byte) 0;
      return connectionRequest;
    }

    private static byte[] CreateSocks5ConnectionRequest(string hostname, ushort port)
    {
      byte addressType;
      byte[] destinationAddress = Session.GetSocks5DestinationAddress(hostname, out addressType);
      byte[] connectionRequest = new byte[4 + destinationAddress.Length + 2];
      int num1 = 0;
      byte[] numArray1 = connectionRequest;
      int index1 = num1;
      int num2 = index1 + 1;
      numArray1[index1] = (byte) 5;
      byte[] numArray2 = connectionRequest;
      int index2 = num2;
      int num3 = index2 + 1;
      numArray2[index2] = (byte) 1;
      byte[] numArray3 = connectionRequest;
      int index3 = num3;
      int num4 = index3 + 1;
      numArray3[index3] = (byte) 0;
      byte[] numArray4 = connectionRequest;
      int index4 = num4;
      int dstOffset = index4 + 1;
      int num5 = (int) addressType;
      numArray4[index4] = (byte) num5;
      Buffer.BlockCopy((Array) destinationAddress, 0, (Array) connectionRequest, dstOffset, destinationAddress.Length);
      int offset = dstOffset + destinationAddress.Length;
      Pack.UInt16ToBigEndian(port, connectionRequest, offset);
      return connectionRequest;
    }

    private static byte[] GetSocks4DestinationAddress(string hostname)
    {
      foreach (IPAddress hostAddress in DnsAbstraction.GetHostAddresses(hostname))
      {
        if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
          return hostAddress.GetAddressBytes();
      }
      throw new ProxyException(string.Format("SOCKS4 only supports IPv4. No such address found for '{0}'.", (object) hostname));
    }

    private static byte[] GetSocks5DestinationAddress(string hostname, out byte addressType)
    {
      IPAddress hostAddress = DnsAbstraction.GetHostAddresses(hostname)[0];
      byte[] addressBytes;
      switch (hostAddress.AddressFamily)
      {
        case AddressFamily.InterNetwork:
          addressType = (byte) 1;
          addressBytes = hostAddress.GetAddressBytes();
          break;
        case AddressFamily.InterNetworkV6:
          addressType = (byte) 4;
          addressBytes = hostAddress.GetAddressBytes();
          break;
        default:
          throw new ProxyException(string.Format("SOCKS5: IP address '{0}' is not supported.", (object) hostAddress));
      }
      return addressBytes;
    }

    private void ConnectHttp()
    {
      Regex regex1 = new Regex("HTTP/(?<version>\\d[.]\\d) (?<statusCode>\\d{3}) (?<reasonPhrase>.+)$");
      Regex regex2 = new Regex("(?<fieldName>[^\\[\\]()<>@,;:\\\"/?={} \\t]+):(?<fieldValue>.+)?");
      SocketAbstraction.Send(this._socket, SshData.Ascii.GetBytes(string.Format("CONNECT {0}:{1} HTTP/1.0\r\n", (object) this.ConnectionInfo.Host, (object) this.ConnectionInfo.Port)));
      if (!string.IsNullOrEmpty(this.ConnectionInfo.ProxyUsername))
      {
        string s = string.Format("Proxy-Authorization: Basic {0}\r\n", (object) Convert.ToBase64String(SshData.Ascii.GetBytes(string.Format("{0}:{1}", (object) this.ConnectionInfo.ProxyUsername, (object) this.ConnectionInfo.ProxyPassword))));
        SocketAbstraction.Send(this._socket, SshData.Ascii.GetBytes(s));
      }
      SocketAbstraction.Send(this._socket, SshData.Ascii.GetBytes("\r\n"));
      HttpStatusCode? nullable1 = new HttpStatusCode?();
      int length = 0;
      Match match1;
      string s1;
      while (true)
      {
        HttpStatusCode? nullable2;
        HttpStatusCode httpStatusCode;
        Match match2;
        do
        {
          do
          {
            string input;
            do
            {
              do
              {
                input = this.SocketReadLine(this.ConnectionInfo.Timeout);
                if (input != null)
                {
                  if (nullable1.HasValue)
                  {
                    match2 = regex2.Match(input);
                    if (match2.Success)
                      goto label_8;
                  }
                  else
                    goto label_4;
                }
                else
                  goto label_15;
              }
              while (input.Length != 0);
              goto label_11;
label_8:;
            }
            while (!match2.Result("${fieldName}").Equals("Content-Length", StringComparison.OrdinalIgnoreCase));
            goto label_9;
label_4:
            match1 = regex1.Match(input);
          }
          while (!match1.Success);
          s1 = match1.Result("${statusCode}");
          nullable1 = new HttpStatusCode?((HttpStatusCode) int.Parse(s1));
          nullable2 = nullable1;
          httpStatusCode = HttpStatusCode.OK;
        }
        while (nullable2.GetValueOrDefault() == httpStatusCode & nullable2.HasValue);
        break;
label_9:
        length = int.Parse(match2.Result("${fieldValue}"));
      }
      string str = match1.Result("${reasonPhrase}");
      throw new ProxyException(string.Format("HTTP: Status code {0}, \"{1}\"", (object) s1, (object) str));
label_11:
      if (length > 0)
        this.SocketRead(new byte[length], 0, length);
label_15:
      if (!nullable1.HasValue)
        throw new ProxyException("HTTP response does not contain status line.");
    }

    private void RaiseError(Exception exp)
    {
      SshConnectionException connectionException = exp as SshConnectionException;
      DiagnosticAbstraction.Log(string.Format("[{0}] Raised exception: {1}", (object) Session.ToHex(this.SessionId), (object) exp));
      if (this._isDisconnecting && (connectionException != null || exp is SocketException socketException && socketException.SocketErrorCode == SocketError.TimedOut))
        return;
      this._exception = exp;
      this._exceptionWaitHandle.Set();
      EventHandler<ExceptionEventArgs> errorOccured = this.ErrorOccured;
      if (errorOccured != null)
        errorOccured((object) this, new ExceptionEventArgs(exp));
      if (connectionException == null)
        return;
      DiagnosticAbstraction.Log(string.Format("[{0}] Disconnecting after exception: {1}", (object) Session.ToHex(this.SessionId), (object) exp));
      this.Disconnect(connectionException.DisconnectReason, exp.ToString());
    }

    private void Reset()
    {
      if (this._exceptionWaitHandle != null)
        this._exceptionWaitHandle.Reset();
      if (this._keyExchangeCompletedWaitHandle != null)
        this._keyExchangeCompletedWaitHandle.Reset();
      if (this._messageListenerCompleted != null)
        this._messageListenerCompleted.Set();
      this.SessionId = (byte[]) null;
      this._isDisconnectMessageSent = false;
      this._isDisconnecting = false;
      this._isAuthenticated = false;
      this._exception = (Exception) null;
      this._keyExchangeInProgress = false;
    }

    private static SshConnectionException CreateConnectionAbortedByServerException() => new SshConnectionException("An established connection was aborted by the server.", DisconnectReason.ConnectionLost);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._disposed || !disposing)
        return;
      DiagnosticAbstraction.Log(string.Format("[{0}] Disposing session.", (object) Session.ToHex(this.SessionId)));
      this.Disconnect();
      EventWaitHandle serviceAccepted = this._serviceAccepted;
      if (serviceAccepted != null)
      {
        serviceAccepted.Dispose();
        this._serviceAccepted = (EventWaitHandle) null;
      }
      EventWaitHandle exceptionWaitHandle = this._exceptionWaitHandle;
      if (exceptionWaitHandle != null)
      {
        exceptionWaitHandle.Dispose();
        this._exceptionWaitHandle = (EventWaitHandle) null;
      }
      EventWaitHandle completedWaitHandle = this._keyExchangeCompletedWaitHandle;
      if (completedWaitHandle != null)
      {
        completedWaitHandle.Dispose();
        this._keyExchangeCompletedWaitHandle = (EventWaitHandle) null;
      }
      HashAlgorithm serverMac = this._serverMac;
      if (serverMac != null)
      {
        serverMac.Dispose();
        this._serverMac = (HashAlgorithm) null;
      }
      HashAlgorithm clientMac = this._clientMac;
      if (clientMac != null)
      {
        clientMac.Dispose();
        this._clientMac = (HashAlgorithm) null;
      }
      IKeyExchange keyExchange = this._keyExchange;
      if (keyExchange != null)
      {
        keyExchange.HostKeyReceived -= new EventHandler<HostKeyEventArgs>(this.KeyExchange_HostKeyReceived);
        keyExchange.Dispose();
        this._keyExchange = (IKeyExchange) null;
      }
      EventWaitHandle listenerCompleted = this._messageListenerCompleted;
      if (listenerCompleted != null)
      {
        listenerCompleted.Dispose();
        this._messageListenerCompleted = (EventWaitHandle) null;
      }
      this._disposed = true;
    }

    ~Session() => this.Dispose(false);

    IConnectionInfo ISession.ConnectionInfo => (IConnectionInfo) this.ConnectionInfo;

    WaitHandle ISession.MessageListenerCompleted => (WaitHandle) this._messageListenerCompleted;

    IChannelSession ISession.CreateChannelSession() => (IChannelSession) new ChannelSession((ISession) this, this.NextChannelNumber, (uint) int.MaxValue, 65536U);

    IChannelDirectTcpip ISession.CreateChannelDirectTcpip() => (IChannelDirectTcpip) new ChannelDirectTcpip((ISession) this, this.NextChannelNumber, (uint) int.MaxValue, 65536U);

    IChannelForwardedTcpip ISession.CreateChannelForwardedTcpip(
      uint remoteChannelNumber,
      uint remoteWindowSize,
      uint remoteChannelDataPacketSize)
    {
      return (IChannelForwardedTcpip) new ChannelForwardedTcpip((ISession) this, this.NextChannelNumber, (uint) int.MaxValue, 65536U, remoteChannelNumber, remoteWindowSize, remoteChannelDataPacketSize);
    }

    void ISession.SendMessage(Message message) => this.SendMessage(message);

    bool ISession.TrySendMessage(Message message) => this.TrySendMessage(message);
  }
}
