// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.NoneAuthenticationMethod
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Threading;

namespace Renci.SshNet
{
  public class NoneAuthenticationMethod : AuthenticationMethod, IDisposable
  {
    private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
    private EventWaitHandle _authenticationCompleted = (EventWaitHandle) new AutoResetEvent(false);
    private bool _isDisposed;

    public override string Name => "none";

    public NoneAuthenticationMethod(string username)
      : base(username)
    {
    }

    public override AuthenticationResult Authenticate(Session session)
    {
      if (session == null)
        throw new ArgumentNullException(nameof (session));
      session.UserAuthenticationSuccessReceived += new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
      session.UserAuthenticationFailureReceived += new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
      try
      {
        session.SendMessage((Message) new RequestMessageNone(ServiceName.Connection, this.Username));
        session.WaitOnHandle((WaitHandle) this._authenticationCompleted);
      }
      finally
      {
        session.UserAuthenticationSuccessReceived -= new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
        session.UserAuthenticationFailureReceived -= new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
      }
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

    ~NoneAuthenticationMethod() => this.Dispose(false);
  }
}
