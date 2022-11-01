// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.NetConfClient
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.NetConf;
using System;
using System.Xml;

namespace Renci.SshNet
{
  public class NetConfClient : BaseClient
  {
    private int _operationTimeout;
    private INetConfSession _netConfSession;

    public TimeSpan OperationTimeout
    {
      get => TimeSpan.FromMilliseconds((double) this._operationTimeout);
      set
      {
        double totalMilliseconds = value.TotalMilliseconds;
        this._operationTimeout = totalMilliseconds >= -1.0 && totalMilliseconds <= (double) int.MaxValue ? (int) totalMilliseconds : throw new ArgumentOutOfRangeException(nameof (value), "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");
      }
    }

    internal INetConfSession NetConfSession => this._netConfSession;

    public NetConfClient(ConnectionInfo connectionInfo)
      : this(connectionInfo, false)
    {
    }

    public NetConfClient(string host, int port, string username, string password)
      : this((ConnectionInfo) new PasswordConnectionInfo(host, port, username, password), true)
    {
    }

    public NetConfClient(string host, string username, string password)
      : this(host, ConnectionInfo.DefaultPort, username, password)
    {
    }

    public NetConfClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
      : this((ConnectionInfo) new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
    {
    }

    public NetConfClient(string host, string username, params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
    {
    }

    private NetConfClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
      : this(connectionInfo, ownsConnectionInfo, (IServiceFactory) new ServiceFactory())
    {
    }

    internal NetConfClient(
      ConnectionInfo connectionInfo,
      bool ownsConnectionInfo,
      IServiceFactory serviceFactory)
      : base(connectionInfo, ownsConnectionInfo, serviceFactory)
    {
      this._operationTimeout = Session.Infinite;
      this.AutomaticMessageIdHandling = true;
    }

    public XmlDocument ServerCapabilities => this._netConfSession.ServerCapabilities;

    public XmlDocument ClientCapabilities => this._netConfSession.ClientCapabilities;

    public bool AutomaticMessageIdHandling { get; set; }

    public XmlDocument SendReceiveRpc(XmlDocument rpc) => this._netConfSession.SendReceiveRpc(rpc, this.AutomaticMessageIdHandling);

    public XmlDocument SendReceiveRpc(string xml)
    {
      XmlDocument rpc = new XmlDocument();
      rpc.LoadXml(xml);
      return this.SendReceiveRpc(rpc);
    }

    public XmlDocument SendCloseRpc()
    {
      XmlDocument rpc = new XmlDocument();
      rpc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><rpc message-id=\"6666\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\"><close-session/></rpc>");
      return this._netConfSession.SendReceiveRpc(rpc, this.AutomaticMessageIdHandling);
    }

    protected override void OnConnected()
    {
      base.OnConnected();
      this._netConfSession = this.CreateAndConnectNetConfSession();
    }

    protected override void OnDisconnecting()
    {
      base.OnDisconnecting();
      this._netConfSession.Disconnect();
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing || this._netConfSession == null)
        return;
      this._netConfSession.Dispose();
      this._netConfSession = (INetConfSession) null;
    }

    private INetConfSession CreateAndConnectNetConfSession()
    {
      INetConfSession netConfSession = this.ServiceFactory.CreateNetConfSession(this.Session, this._operationTimeout);
      try
      {
        netConfSession.Connect();
        return netConfSession;
      }
      catch
      {
        netConfSession.Dispose();
        throw;
      }
    }
  }
}
