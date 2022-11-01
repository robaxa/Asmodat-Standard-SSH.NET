// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.PrivateKeyConnectionInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Renci.SshNet
{
  public class PrivateKeyConnectionInfo : ConnectionInfo, IDisposable
  {
    private bool _isDisposed;

    public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

    public PrivateKeyConnectionInfo(string host, string username, params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      int port,
      string username,
      params PrivateKeyFile[] keyFiles)
      : this(host, port, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      params PrivateKeyFile[] keyFiles)
      : this(host, port, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      params PrivateKeyFile[] keyFiles)
      : this(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword,
      params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, keyFiles)
    {
    }

    public PrivateKeyConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword,
      params PrivateKeyFile[] keyFiles)
      : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, (AuthenticationMethod) new PrivateKeyAuthenticationMethod(username, keyFiles))
    {
      this.KeyFiles = (ICollection<PrivateKeyFile>) new Collection<PrivateKeyFile>((IList<PrivateKeyFile>) keyFiles);
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
      if (this.AuthenticationMethods != null)
      {
        foreach (AuthenticationMethod authenticationMethod in (IEnumerable<AuthenticationMethod>) this.AuthenticationMethods)
        {
          if (authenticationMethod is IDisposable disposable)
            disposable.Dispose();
        }
      }
      this._isDisposed = true;
    }

    ~PrivateKeyConnectionInfo() => this.Dispose(false);
  }
}
