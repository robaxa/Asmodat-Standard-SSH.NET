// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.NetConf.NetConfSession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace Renci.SshNet.NetConf
{
  internal class NetConfSession : SubsystemSession, INetConfSession, ISubsystemSession, IDisposable
  {
    private const string Prompt = "]]>]]>";
    private readonly StringBuilder _data = new StringBuilder();
    private bool _usingFramingProtocol;
    private EventWaitHandle _serverCapabilitiesConfirmed = (EventWaitHandle) new AutoResetEvent(false);
    private EventWaitHandle _rpcReplyReceived = (EventWaitHandle) new AutoResetEvent(false);
    private StringBuilder _rpcReply = new StringBuilder();
    private int _messageId;

    public XmlDocument ServerCapabilities { get; private set; }

    public XmlDocument ClientCapabilities { get; private set; }

    public NetConfSession(ISession session, int operationTimeout)
      : base(session, "netconf", operationTimeout)
    {
      this.ClientCapabilities = new XmlDocument();
      this.ClientCapabilities.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><hello xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\"><capabilities><capability>urn:ietf:params:netconf:base:1.0</capability></capabilities></hello>");
    }

    public XmlDocument SendReceiveRpc(XmlDocument rpc, bool automaticMessageIdHandling)
    {
      this._data.Clear();
      XmlNamespaceManager nsmgr = (XmlNamespaceManager) null;
      if (automaticMessageIdHandling)
      {
        ++this._messageId;
        nsmgr = new XmlNamespaceManager(rpc.NameTable);
        nsmgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");
        rpc.SelectSingleNode("/nc:rpc/@message-id", nsmgr).Value = this._messageId.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      }
      this._rpcReply = new StringBuilder();
      this._rpcReplyReceived.Reset();
      XmlDocument rpc1 = new XmlDocument();
      if (this._usingFramingProtocol)
      {
        StringBuilder stringBuilder = new StringBuilder(rpc.InnerXml.Length + 10);
        stringBuilder.AppendFormat("\n#{0}\n", (object) rpc.InnerXml.Length);
        stringBuilder.Append(rpc.InnerXml);
        stringBuilder.Append("\n##\n");
        this.SendData(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        this.WaitOnHandle((WaitHandle) this._rpcReplyReceived, this.OperationTimeout);
        rpc1.LoadXml(this._rpcReply.ToString());
      }
      else
      {
        this.SendData(Encoding.UTF8.GetBytes(rpc.InnerXml + "]]>]]>"));
        this.WaitOnHandle((WaitHandle) this._rpcReplyReceived, this.OperationTimeout);
        rpc1.LoadXml(this._rpcReply.ToString());
      }
      if (automaticMessageIdHandling && rpc.SelectSingleNode("/nc:rpc/@message-id", nsmgr).Value != this._messageId.ToString((IFormatProvider) CultureInfo.InvariantCulture))
        throw new NetConfServerException("The rpc message id does not match the rpc-reply message id.");
      return rpc1;
    }

    protected override void OnChannelOpen()
    {
      this._data.Clear();
      this.SendData(Encoding.UTF8.GetBytes(string.Format("{0}{1}", (object) this.ClientCapabilities.InnerXml, (object) "]]>]]>")));
      this.WaitOnHandle((WaitHandle) this._serverCapabilitiesConfirmed, this.OperationTimeout);
    }

    protected override void OnDataReceived(byte[] data)
    {
      string str1 = Encoding.UTF8.GetString(data);
      if (this.ServerCapabilities == null)
      {
        this._data.Append(str1);
        if (!str1.Contains("]]>]]>"))
          return;
        try
        {
          string str2 = this._data.ToString();
          this._data.Clear();
          this.ServerCapabilities = new XmlDocument();
          this.ServerCapabilities.LoadXml(str2.Replace("]]>]]>", ""));
        }
        catch (XmlException ex)
        {
          throw new NetConfServerException("Server capabilities received are not well formed XML", (Exception) ex);
        }
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(this.ServerCapabilities.NameTable);
        nsmgr.AddNamespace("nc", "urn:ietf:params:xml:ns:netconf:base:1.0");
        this._usingFramingProtocol = this.ServerCapabilities.SelectSingleNode("/nc:hello/nc:capabilities/nc:capability[text()='urn:ietf:params:netconf:base:1.1']", nsmgr) != null;
        this._serverCapabilitiesConfirmed.Set();
      }
      else if (this._usingFramingProtocol)
      {
        int startIndex = 0;
        while (true)
        {
          Match match = Regex.Match(str1.Substring(startIndex), "\\n#(?<length>\\d+)\\n");
          if (match.Success)
          {
            int int32 = Convert.ToInt32(match.Groups["length"].Value);
            this._rpcReply.Append(str1, startIndex + match.Index + match.Length, int32);
            startIndex += match.Index + match.Length + int32;
          }
          else
            break;
        }
        if (!Regex.IsMatch(str1.Substring(startIndex), "\\n##\\n"))
          return;
        this._rpcReplyReceived.Set();
      }
      else
      {
        this._data.Append(str1);
        if (!str1.Contains("]]>]]>"))
          return;
        string str3 = this._data.ToString();
        this._data.Clear();
        this._rpcReply.Append(str3.Replace("]]>]]>", ""));
        this._rpcReplyReceived.Set();
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (!disposing)
        return;
      if (this._serverCapabilitiesConfirmed != null)
      {
        this._serverCapabilitiesConfirmed.Dispose();
        this._serverCapabilitiesConfirmed = (EventWaitHandle) null;
      }
      if (this._rpcReplyReceived != null)
      {
        this._rpcReplyReceived.Dispose();
        this._rpcReplyReceived = (EventWaitHandle) null;
      }
    }
  }
}
