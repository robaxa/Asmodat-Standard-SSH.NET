// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.PrivateKeyAuthenticationMethod
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Renci.SshNet
{
  public class PrivateKeyAuthenticationMethod : AuthenticationMethod, IDisposable
  {
    private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
    private EventWaitHandle _authenticationCompleted = (EventWaitHandle) new ManualResetEvent(false);
    private bool _isSignatureRequired;
    private bool _isDisposed;

    public override string Name => "publickey";

    public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

    public PrivateKeyAuthenticationMethod(string username, params PrivateKeyFile[] keyFiles)
      : base(username)
    {
      this.KeyFiles = keyFiles != null ? (ICollection<PrivateKeyFile>) new Collection<PrivateKeyFile>((IList<PrivateKeyFile>) keyFiles) : throw new ArgumentNullException(nameof (keyFiles));
    }

    public override AuthenticationResult Authenticate(Session session)
    {
      session.UserAuthenticationSuccessReceived += new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
      session.UserAuthenticationFailureReceived += new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
      session.UserAuthenticationPublicKeyReceived += new EventHandler<MessageEventArgs<PublicKeyMessage>>(this.Session_UserAuthenticationPublicKeyReceived);
      session.RegisterMessage("SSH_MSG_USERAUTH_PK_OK");
      try
      {
        foreach (PrivateKeyFile keyFile in (IEnumerable<PrivateKeyFile>) this.KeyFiles)
        {
          this._authenticationCompleted.Reset();
          this._isSignatureRequired = false;
          RequestMessagePublicKey message = new RequestMessagePublicKey(ServiceName.Connection, this.Username, keyFile.HostKey.Name, keyFile.HostKey.Data);
          if (this.KeyFiles.Count < 2)
          {
            byte[] bytes = new PrivateKeyAuthenticationMethod.SignatureData(message, session.SessionId).GetBytes();
            message.Signature = keyFile.HostKey.Sign(bytes);
          }
          session.SendMessage((Message) message);
          session.WaitOnHandle((WaitHandle) this._authenticationCompleted);
          if (this._isSignatureRequired)
          {
            this._authenticationCompleted.Reset();
            RequestMessagePublicKey messagePublicKey = new RequestMessagePublicKey(ServiceName.Connection, this.Username, keyFile.HostKey.Name, keyFile.HostKey.Data);
            byte[] bytes = new PrivateKeyAuthenticationMethod.SignatureData(message, session.SessionId).GetBytes();
            messagePublicKey.Signature = keyFile.HostKey.Sign(bytes);
            session.SendMessage((Message) messagePublicKey);
          }
          session.WaitOnHandle((WaitHandle) this._authenticationCompleted);
          if (this._authenticationResult == AuthenticationResult.Success)
            break;
        }
        return this._authenticationResult;
      }
      finally
      {
        session.UserAuthenticationSuccessReceived -= new EventHandler<MessageEventArgs<SuccessMessage>>(this.Session_UserAuthenticationSuccessReceived);
        session.UserAuthenticationFailureReceived -= new EventHandler<MessageEventArgs<FailureMessage>>(this.Session_UserAuthenticationFailureReceived);
        session.UserAuthenticationPublicKeyReceived -= new EventHandler<MessageEventArgs<PublicKeyMessage>>(this.Session_UserAuthenticationPublicKeyReceived);
        session.UnRegisterMessage("SSH_MSG_USERAUTH_PK_OK");
      }
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

    private void Session_UserAuthenticationPublicKeyReceived(
      object sender,
      MessageEventArgs<PublicKeyMessage> e)
    {
      this._isSignatureRequired = true;
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
        this._authenticationCompleted = (EventWaitHandle) null;
        authenticationCompleted.Dispose();
      }
      this._isDisposed = true;
    }

    ~PrivateKeyAuthenticationMethod() => this.Dispose(false);

    private class SignatureData : SshData
    {
      private readonly RequestMessagePublicKey _message;
      private readonly byte[] _sessionId;
      private readonly byte[] _serviceName;
      private readonly byte[] _authenticationMethod;

      protected override int BufferCapacity => base.BufferCapacity + 4 + this._sessionId.Length + 1 + 4 + this._message.Username.Length + 4 + this._serviceName.Length + 4 + this._authenticationMethod.Length + 1 + 4 + this._message.PublicKeyAlgorithmName.Length + 4 + this._message.PublicKeyData.Length;

      public SignatureData(RequestMessagePublicKey message, byte[] sessionId)
      {
        this._message = message;
        this._sessionId = sessionId;
        this._serviceName = ServiceName.Connection.ToArray();
        this._authenticationMethod = SshData.Ascii.GetBytes("publickey");
      }

      protected override void LoadData() => throw new NotImplementedException();

      protected override void SaveData()
      {
        this.WriteBinaryString(this._sessionId);
        this.Write((byte) 50);
        this.WriteBinaryString(this._message.Username);
        this.WriteBinaryString(this._serviceName);
        this.WriteBinaryString(this._authenticationMethod);
        this.Write((byte) 1);
        this.WriteBinaryString(this._message.PublicKeyAlgorithmName);
        this.WriteBinaryString(this._message.PublicKeyData);
      }
    }
  }
}
