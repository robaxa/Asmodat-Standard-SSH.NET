// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.PasswordConnectionInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet
{
  public class PasswordConnectionInfo : ConnectionInfo, IDisposable
  {
    private bool _isDisposed;

    public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;

    public PasswordConnectionInfo(string host, string username, string password)
      : this(host, ConnectionInfo.DefaultPort, username, Encoding.UTF8.GetBytes(password))
    {
    }

    public PasswordConnectionInfo(string host, int port, string username, string password)
      : this(host, port, username, Encoding.UTF8.GetBytes(password), ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      int port,
      string username,
      string password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort)
      : this(host, port, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      int port,
      string username,
      string password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername)
      : this(host, port, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      string username,
      string password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort)
      : this(host, ConnectionInfo.DefaultPort, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      string username,
      string password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername)
      : this(host, ConnectionInfo.DefaultPort, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      string username,
      string password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword)
      : this(host, ConnectionInfo.DefaultPort, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword)
    {
    }

    public PasswordConnectionInfo(string host, string username, byte[] password)
      : this(host, ConnectionInfo.DefaultPort, username, password)
    {
    }

    public PasswordConnectionInfo(string host, int port, string username, byte[] password)
      : this(host, port, username, password, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      int port,
      string username,
      byte[] password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort)
      : this(host, port, username, password, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      int port,
      string username,
      byte[] password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername)
      : this(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      string username,
      byte[] password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort)
      : this(host, ConnectionInfo.DefaultPort, username, password, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      string username,
      byte[] password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername)
      : this(host, ConnectionInfo.DefaultPort, username, password, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
    {
    }

    public PasswordConnectionInfo(
      string host,
      string username,
      byte[] password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword)
      : this(host, ConnectionInfo.DefaultPort, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword)
    {
    }

    public PasswordConnectionInfo(
      string host,
      int port,
      string username,
      byte[] password,
      ProxyTypes proxyType,
      string proxyHost,
      int proxyPort,
      string proxyUsername,
      string proxyPassword)
      : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, (AuthenticationMethod) new PasswordAuthenticationMethod(username, password))
    {
      foreach (AuthenticationMethod authenticationMethod1 in (IEnumerable<AuthenticationMethod>) this.AuthenticationMethods)
      {
        if (authenticationMethod1 is PasswordAuthenticationMethod authenticationMethod2)
          authenticationMethod2.PasswordExpired += new EventHandler<AuthenticationPasswordChangeEventArgs>(this.AuthenticationMethod_PasswordExpired);
      }
    }

    private void AuthenticationMethod_PasswordExpired(
      object sender,
      AuthenticationPasswordChangeEventArgs e)
    {
      if (this.PasswordExpired == null)
        return;
      this.PasswordExpired(sender, e);
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

    ~PasswordConnectionInfo() => this.Dispose(false);
  }
}
