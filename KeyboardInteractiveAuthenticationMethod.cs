// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.KeyboardInteractiveAuthenticationMethod
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Linq;
using System.Threading;

namespace Renci.SshNet
{
  public class KeyboardInteractiveAuthenticationMethod : AuthenticationMethod, IDisposable
  {
    private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
    private Session _session;
    private EventWaitHandle _authenticationCompleted = (EventWaitHandle) new AutoResetEvent(false);
    private Exception _exception;
    private readonly RequestMessage _requestMessage;
    private bool _isDisposed;

    public override string Name => this._requestMessage.MethodName;

    public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;

    public KeyboardInteractiveAuthenticationMethod(string username)
      : base(username)
    {
      this._requestMessage = (RequestMessage) new RequestMessageKeyboardInteractive(ServiceName.Connection, username);
    }

    public override AuthenticationResult Authenticate(Session session)
    {
      this._session = session;
      session.UserAuthenticationSuccessReceived += new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
      session.UserAuthenticationFailureReceived += new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
      session.UserAuthenticationInformationRequestReceived += new EventHandler<MessageEventArgs<InformationRequestMessage>>(this.Session_UserAuthenticationInformationRequestReceived);
      session.RegisterMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
      try
      {
        session.SendMessage((Message) this._requestMessage);
        session.WaitOnHandle((WaitHandle) this._authenticationCompleted);
      }
      finally
      {
        session.UnRegisterMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
        session.UserAuthenticationSuccessReceived -= new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
        session.UserAuthenticationFailureReceived -= new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
        session.UserAuthenticationInformationRequestReceived -= new EventHandler<MessageEventArgs<InformationRequestMessage>>(this.Session_UserAuthenticationInformationRequestReceived);
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

    private void Session_UserAuthenticationInformationRequestReceived(
      object sender,
      MessageEventArgs<InformationRequestMessage> e)
    {
      InformationRequestMessage message = e.Message;
      AuthenticationPromptEventArgs eventArgs = new AuthenticationPromptEventArgs(this.Username, message.Instruction, message.Language, message.Prompts);
      ThreadAbstraction.ExecuteThread((Action) (() =>
      {
        try
        {
          if (this.AuthenticationPrompt != null)
            this.AuthenticationPrompt((object) this, eventArgs);
          InformationResponseMessage informationResponseMessage = new InformationResponseMessage();
          foreach (string str in eventArgs.Prompts.OrderBy<Renci.SshNet.Common.AuthenticationPrompt, int>((Func<Renci.SshNet.Common.AuthenticationPrompt, int>) (r => r.Id)).Select<Renci.SshNet.Common.AuthenticationPrompt, string>((Func<Renci.SshNet.Common.AuthenticationPrompt, string>) (r => r.Response)))
            informationResponseMessage.Responses.Add(str);
          this._session.SendMessage((Message) informationResponseMessage);
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
        this._authenticationCompleted = (EventWaitHandle) null;
        authenticationCompleted.Dispose();
      }
      this._isDisposed = true;
    }

    ~KeyboardInteractiveAuthenticationMethod() => this.Dispose(false);
  }
}
