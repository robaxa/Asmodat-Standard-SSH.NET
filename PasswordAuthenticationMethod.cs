// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.PasswordAuthenticationMethod
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Text;
using System.Threading;

namespace Renci.SshNet
{
  public class PasswordAuthenticationMethod : AuthenticationMethod, IDisposable
  {
    private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
    private Session _session;
    private EventWaitHandle _authenticationCompleted = (EventWaitHandle) new AutoResetEvent(false);
    private Exception _exception;
    private readonly RequestMessage _requestMessage;
    private readonly byte[] _password;
    private bool _isDisposed;

    public override string Name => this._requestMessage.MethodName;

    internal byte[] Password => this._password;

    public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;

    public PasswordAuthenticationMethod(string username, string password)
      : this(username, Encoding.UTF8.GetBytes(password))
    {
    }

    public PasswordAuthenticationMethod(string username, byte[] password)
      : base(username)
    {
      this._password = password != null ? password : throw new ArgumentNullException(nameof (password));
      this._requestMessage = (RequestMessage) new RequestMessagePassword(ServiceName.Connection, this.Username, this._password);
    }

    public override AuthenticationResult Authenticate(Session session)
    {
      this._session = session != null ? session : throw new ArgumentNullException(nameof (session));
      session.UserAuthenticationSuccessReceived += new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
      session.UserAuthenticationFailureReceived += new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
      session.UserAuthenticationPasswordChangeRequiredReceived += new EventHandler<MessageEventArgs<PasswordChangeRequiredMessage>>(this.Session_UserAuthenticationPasswordChangeRequiredReceived);
      try
      {
        session.RegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        session.SendMessage((Message) this._requestMessage);
        session.WaitOnHandle((WaitHandle) this._authenticationCompleted);
      }
      finally
      {
        session.UnRegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
        session.UserAuthenticationSuccessReceived -= new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
        session.UserAuthenticationFailureReceived -= new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
        session.UserAuthenticationPasswordChangeRequiredReceived -= new EventHandler<MessageEventArgs<PasswordChangeRequiredMessage>>(this.Session_UserAuthenticationPasswordChangeRequiredReceived);
      }
      if (this._exception != null)
        throw this._exception;
      return this._authenticationResult;
    }

    private void Session_UserAuthenticationSuccessReceived(
      object sender,
      MessageEventArgs<SuccessMessage> e)
    {
      this._authenticationResult = AuthenticationResult.Success;
      this._authenticationCompleted.Set();
    }

    private void Session_UserAuthenticationFailureReceived(
      object sender,
      MessageEventArgs<FailureMessage> e)
    {
      this._authenticationResult = !e.Message.PartialSuccess ? AuthenticationResult.Failure : AuthenticationResult.PartialSuccess;
      this.AllowedAuthentications = e.Message.AllowedAuthentications;
      this._authenticationCompleted.Set();
    }

    private void Session_UserAuthenticationPasswordChangeRequiredReceived(
      object sender,
      MessageEventArgs<PasswordChangeRequiredMessage> e)
    {
      this._session.UnRegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          AuthenticationPasswordChangeEventArgs e1 = new AuthenticationPasswordChangeEventArgs(this.Username);
          if (this.PasswordExpired != null)
            this.PasswordExpired((object) this, e1);
          this._session.SendMessage((Message) new RequestMessagePassword(ServiceName.Connection, this.Username, this._password, e1.NewPassword));
        }
        catch (Exception ex)
        {
          this._exception = ex;
          this._authenticationCompleted.Set();
        }
      }));
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
      EventWaitHandle authenticationCompleted = this._authenticationCompleted;
      if (authenticationCompleted != null)
      {
        authenticationCompleted.Dispose();
        this._authenticationCompleted = (EventWaitHandle) null;
      }
      this._isDisposed = true;
    }

    ~PasswordAuthenticationMethod() => this.Dispose(false);
  }
}
