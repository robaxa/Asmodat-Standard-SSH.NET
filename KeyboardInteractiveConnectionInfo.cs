// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.KeyboardInteractiveConnectionInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;

namespace Renci.SshNet
{
  public class KeyboardInteractiveConnectionInfo : ConnectionInfo, IDisposable
  {
    private bool _isDisposed;

    public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;

    public KeyboardInteractiveConnectionInfo(string host, string username)
      : this(host, ConnectionInfo.DefaultPort, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
    {
    }

    public KeyboardInteractiveConnectionInfo(string host, int port, string username)
      : this(host, port, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
    {
    }

    public KeyboardInteractiveConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort)
      : this(host, port, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
    {
    }

    public KeyboardInteractiveConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername)
      : this(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
    {
    }

    public KeyboardInteractiveConnectionInfo(
      string host,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort)
      : this(host, ConnectionInfo.DefaultPort, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
    {
    }

    public KeyboardInteractiveConnectionInfo(
      string host,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername)
      : this(host, ConnectionInfo.DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
    {
    }

    public KeyboardInteractiveConnectionInfo(
      string host,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword)
      : this(host, ConnectionInfo.DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword)
    {
    }

    public KeyboardInteractiveConnectionInfo(
      string host,
      int port,
      string username,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword)
      : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, (AuthenticationMethod) new KeyboardInteractiveAuthenticationMethod(username))
    {
      foreach (AuthenticationMethod authenticationMethod1 in (IEnumerable<AuthenticationMethod>) this.AuthenticationMethods)
      {
        if (authenticationMethod1 is KeyboardInteractiveAuthenticationMethod authenticationMethod2)
          authenticationMethod2.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(this.AuthenticationMethod_AuthenticationPrompt);
      }
    }

    private void AuthenticationMethod_AuthenticationPrompt(
      object sender,
      AuthenticationPromptEventArgs e)
    {
      if (this.AuthenticationPrompt == null)
        return;
      this.AuthenticationPrompt(sender, e);
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

    ~KeyboardInteractiveConnectionInfo() => this.Dispose(false);
  }
}
