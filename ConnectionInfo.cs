// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ConnectionInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Renci.SshNet
{
  public class ConnectionInfo : IConnectionInfoInternal, IConnectionInfo
  {
    internal static int DefaultPort = 22;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30.0);
    private static readonly TimeSpan DefaultChannelCloseTimeout = TimeSpan.FromSeconds(1.0);

    public IDictionary<string, Type> KeyExchangeAlgorithms { get; private set; }

    public IDictionary<string, CipherInfo> Encryptions { get; private set; }

    public IDictionary<string, HashInfo> HmacAlgorithms { get; private set; }

    public IDictionary<string, Func<byte[], KeyHostAlgorithm>> HostKeyAlgorithms { get; private set; }

    public IList<AuthenticationMethod> AuthenticationMethods { get; private set; }

    public IDictionary<string, Type> CompressionAlgorithms { get; private set; }

    public IDictionary<string, RequestInfo> ChannelRequests { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public string Host { get; private set; }

    public int Port { get; private set; }

    public string Username { get; private set; }

    public ProxyTypes ProxyType { get; private set; }

    public string ProxyHost { get; private set; }

    public int ProxyPort { get; private set; }

    public string ProxyUsername { get; private set; }

    public string ProxyPassword { get; private set; }

    public TimeSpan Timeout { get; set; }

    public TimeSpan ChannelCloseTimeout { get; set; }

    public Encoding Encoding { get; set; }

    public int RetryAttempts { get; set; }

    public int MaxSessions { get; set; }

    public event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;

    public string CurrentKeyExchangeAlgorithm { get; internal set; }

    public string CurrentServerEncryption { get; internal set; }

    public string CurrentClientEncryption { get; internal set; }

    public string CurrentServerHmacAlgorithm { get; internal set; }

    public string CurrentClientHmacAlgorithm { get; internal set; }

    public string CurrentHostKeyAlgorithm { get; internal set; }

    public string CurrentServerCompressionAlgorithm { get; internal set; }

    public string ServerVersion { get; internal set; }

    public string ClientVersion { get; internal set; }

    public string CurrentClientCompressionAlgorithm { get; internal set; }

    public ConnectionInfo(
      string host,
      string username,
      params AuthenticationMethod[] authenticationMethods)
      : this(host, ConnectionInfo.DefaultPort, username, ProxyTypes.None, (string) null, 0, (string) null, (string) null, authenticationMethods)
    {
    }

    public ConnectionInfo(
      string host,
      int port,
      string username,
      params AuthenticationMethod[] authenticationMethods)
      : this(host, port, username, ProxyTypes.None, (string) null, 0, (string) null, (string) null, authenticationMethods)
    {
    }

    public ConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword,
      params AuthenticationMethod[] authenticationMethods)
    {
      if (host == null)
        throw new ArgumentNullException(nameof (host));
      port.ValidatePort(nameof (port));
      if (username == null)
        throw new ArgumentNullException(nameof (username));
      if (username.All<char>(new Func<char, bool>(char.IsWhiteSpace)))
        throw new ArgumentException("Cannot be empty or contain only whitespace.", nameof (username));
      if (proxyType != 0)
      {
        if (proxyHost == null)
          throw new ArgumentNullException(nameof (proxyHost));
        proxyPort.ValidatePort(nameof (proxyPort));
      }
      if (authenticationMethods == null)
        throw new ArgumentNullException(nameof (authenticationMethods));
      if (authenticationMethods.Length == 0)
        throw new ArgumentException("At least one authentication method should be specified.", nameof (authenticationMethods));
      this.Timeout = ConnectionInfo.DefaultTimeout;
      this.ChannelCloseTimeout = ConnectionInfo.DefaultChannelCloseTimeout;
      this.RetryAttempts = 10;
      this.MaxSessions = 10;
      this.Encoding = Encoding.UTF8;
      this.KeyExchangeAlgorithms = (IDictionary<string, Type>) new Dictionary<string, Type>()
      {
        {
          "diffie-hellman-group-exchange-sha256",
          typeof (KeyExchangeDiffieHellmanGroupExchangeSha256)
        },
        {
          "diffie-hellman-group-exchange-sha1",
          typeof (KeyExchangeDiffieHellmanGroupExchangeSha1)
        },
        {
          "diffie-hellman-group14-sha1",
          typeof (KeyExchangeDiffieHellmanGroup14Sha1)
        },
        {
          "diffie-hellman-group1-sha1",
          typeof (KeyExchangeDiffieHellmanGroup1Sha1)
        }
      };
      this.Encryptions = (IDictionary<string, CipherInfo>) new Dictionary<string, CipherInfo>()
      {
        {
          "aes256-ctr",
          new CipherInfo(256, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CtrCipherMode(iv), (CipherPadding) null)))
        },
        {
          "3des-cbc",
          new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TripleDesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "aes128-cbc",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "aes192-cbc",
          new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "aes256-cbc",
          new CipherInfo(256, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "blowfish-cbc",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new BlowfishCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "twofish-cbc",
          new CipherInfo(256, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TwofishCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "twofish192-cbc",
          new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TwofishCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "twofish128-cbc",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TwofishCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "twofish256-cbc",
          new CipherInfo(256, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TwofishCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "arcfour",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new Arc4Cipher(key, false)))
        },
        {
          "arcfour128",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new Arc4Cipher(key, true)))
        },
        {
          "arcfour256",
          new CipherInfo(256, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new Arc4Cipher(key, true)))
        },
        {
          "cast128-cbc",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new CastCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) null)))
        },
        {
          "aes128-ctr",
          new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CtrCipherMode(iv), (CipherPadding) null)))
        },
        {
          "aes192-ctr",
          new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CtrCipherMode(iv), (CipherPadding) null)))
        }
      };
      this.HmacAlgorithms = (IDictionary<string, HashInfo>) new Dictionary<string, HashInfo>()
      {
        {
          "hmac-md5",
          new HashInfo(128, new Func<byte[], HashAlgorithm>(CryptoAbstraction.CreateHMACMD5))
        },
        {
          "hmac-md5-96",
          new HashInfo(128, (Func<byte[], HashAlgorithm>) (key => (HashAlgorithm) CryptoAbstraction.CreateHMACMD5(key, 96)))
        },
        {
          "hmac-sha1",
          new HashInfo(160, new Func<byte[], HashAlgorithm>(CryptoAbstraction.CreateHMACSHA1))
        },
        {
          "hmac-sha1-96",
          new HashInfo(160, (Func<byte[], HashAlgorithm>) (key => (HashAlgorithm) CryptoAbstraction.CreateHMACSHA1(key, 96)))
        },
        {
          "hmac-sha2-256",
          new HashInfo(256, new Func<byte[], HashAlgorithm>(CryptoAbstraction.CreateHMACSHA256))
        },
        {
          "hmac-sha2-256-96",
          new HashInfo(256, (Func<byte[], HashAlgorithm>) (key => (HashAlgorithm) CryptoAbstraction.CreateHMACSHA256(key, 96)))
        },
        {
          "hmac-sha2-512",
          new HashInfo(512, new Func<byte[], HashAlgorithm>(CryptoAbstraction.CreateHMACSHA512))
        },
        {
          "hmac-sha2-512-96",
          new HashInfo(512, (Func<byte[], HashAlgorithm>) (key => (HashAlgorithm) CryptoAbstraction.CreateHMACSHA512(key, 96)))
        }
      };
      this.HostKeyAlgorithms = (IDictionary<string, Func<byte[], KeyHostAlgorithm>>) new Dictionary<string, Func<byte[], KeyHostAlgorithm>>()
      {
        {
          "ssh-rsa",
          (Func<byte[], KeyHostAlgorithm>) (data => new KeyHostAlgorithm("ssh-rsa", (Key) new RsaKey(), data))
        },
        {
          "ssh-dss",
          (Func<byte[], KeyHostAlgorithm>) (data => new KeyHostAlgorithm("ssh-dss", (Key) new DsaKey(), data))
        }
      };
      this.CompressionAlgorithms = (IDictionary<string, Type>) new Dictionary<string, Type>()
      {
        {
          "none",
          (Type) null
        }
      };
      this.ChannelRequests = (IDictionary<string, RequestInfo>) new Dictionary<string, RequestInfo>()
      {
        {
          "env",
          (RequestInfo) new EnvironmentVariableRequestInfo()
        },
        {
          "exec",
          (RequestInfo) new ExecRequestInfo()
        },
        {
          "exit-signal",
          (RequestInfo) new ExitSignalRequestInfo()
        },
        {
          "exit-status",
          (RequestInfo) new ExitStatusRequestInfo()
        },
        {
          "pty-req",
          (RequestInfo) new PseudoTerminalRequestInfo()
        },
        {
          "shell",
          (RequestInfo) new ShellRequestInfo()
        },
        {
          "signal",
          (RequestInfo) new SignalRequestInfo()
        },
        {
          "subsystem",
          (RequestInfo) new SubsystemRequestInfo()
        },
        {
          "window-change",
          (RequestInfo) new WindowChangeRequestInfo()
        },
        {
          "x11-req",
          (RequestInfo) new X11ForwardingRequestInfo()
        },
        {
          "xon-xoff",
          (RequestInfo) new XonXoffRequestInfo()
        },
        {
          "eow@openssh.com",
          (RequestInfo) new EndOfWriteRequestInfo()
        },
        {
          "keepalive@openssh.com",
          (RequestInfo) new KeepAliveRequestInfo()
        }
      };
      this.Host = host;
      this.Port = port;
      this.Username = username;
      this.ProxyType = proxyType;
      this.ProxyHost = proxyHost;
      this.ProxyPort = proxyPort;
      this.ProxyUsername = proxyUsername;
      this.ProxyPassword = proxyPassword;
      this.AuthenticationMethods = (IList<AuthenticationMethod>) authenticationMethods;
    }

    internal void Authenticate(ISession session, IServiceFactory serviceFactory)
    {
      if (serviceFactory == null)
        throw new ArgumentNullException(nameof (serviceFactory));
      this.IsAuthenticated = false;
      serviceFactory.CreateClientAuthentication().Authenticate((IConnectionInfoInternal) this, session);
      this.IsAuthenticated = true;
    }

    void IConnectionInfoInternal.UserAuthenticationBannerReceived(
      object sender,
      MessageEventArgs<BannerMessage> e)
    {
      EventHandler<AuthenticationBannerEventArgs> authenticationBanner = this.AuthenticationBanner;
      if (authenticationBanner == null)
        return;
      authenticationBanner((object) this, new AuthenticationBannerEventArgs(this.Username, e.Message.Message, e.Message.Language));
    }

    IAuthenticationMethod IConnectionInfoInternal.CreateNoneAuthenticationMethod() => (IAuthenticationMethod) new NoneAuthenticationMethod(this.Username);

    IList<IAuthenticationMethod> IConnectionInfoInternal.AuthenticationMethods => (IList<IAuthenticationMethod>) this.AuthenticationMethods.Cast<IAuthenticationMethod>().ToList<IAuthenticationMethod>();
  }
}
