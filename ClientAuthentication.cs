// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ClientAuthentication
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using System;
using System.Collections.Generic;

namespace Renci.SshNet
{
  internal class ClientAuthentication : IClientAuthentication
  {
    private readonly int _partialSuccessLimit;

    public ClientAuthentication(int partialSuccessLimit) => this._partialSuccessLimit = partialSuccessLimit >= 1 ? partialSuccessLimit : throw new ArgumentOutOfRangeException(nameof (partialSuccessLimit), "Cannot be less than one.");

    internal int PartialSuccessLimit => this._partialSuccessLimit;

    public void Authenticate(IConnectionInfoInternal connectionInfo, ISession session)
    {
      if (connectionInfo == null)
        throw new ArgumentNullException(nameof (connectionInfo));
      if (session == null)
        throw new ArgumentNullException(nameof (session));
      session.RegisterMessage("SSH_MSG_USERAUTH_FAILURE");
      session.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
      session.RegisterMessage("SSH_MSG_USERAUTH_BANNER");
      session.UserAuthenticationBannerReceived += new EventHandler<MessageEventArgs<BannerMessage>>(connectionInfo.UserAuthenticationBannerReceived);
      try
      {
        SshAuthenticationException authenticationException = (SshAuthenticationException) null;
        IAuthenticationMethod authenticationMethod = connectionInfo.CreateNoneAuthenticationMethod();
        if (authenticationMethod.Authenticate(session) != 0 && !this.TryAuthenticate(session, new ClientAuthentication.AuthenticationState(connectionInfo.AuthenticationMethods), authenticationMethod.AllowedAuthentications, ref authenticationException))
          throw authenticationException;
      }
      finally
      {
        session.UserAuthenticationBannerReceived -= new EventHandler<MessageEventArgs<BannerMessage>>(connectionInfo.UserAuthenticationBannerReceived);
        session.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE");
        session.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
        session.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER");
      }
    }

    private bool TryAuthenticate(
      ISession session,
      ClientAuthentication.AuthenticationState authenticationState,
      string[] allowedAuthenticationMethods,
      ref SshAuthenticationException authenticationException)
    {
      if (allowedAuthenticationMethods.Length == 0)
      {
        authenticationException = new SshAuthenticationException("No authentication methods defined on SSH server.");
        return false;
      }
      List<IAuthenticationMethod> authenticationMethods = authenticationState.GetSupportedAuthenticationMethods(allowedAuthenticationMethods);
      if (authenticationMethods.Count == 0)
      {
        authenticationException = new SshAuthenticationException(string.Format("No suitable authentication method found to complete authentication ({0}).", (object) string.Join(",", allowedAuthenticationMethods)));
        return false;
      }
      foreach (IAuthenticationMethod authenticationMethod in authenticationState.GetActiveAuthenticationMethods(authenticationMethods))
      {
        if (authenticationState.GetPartialSuccessCount(authenticationMethod) >= this._partialSuccessLimit)
        {
          authenticationException = new SshAuthenticationException(string.Format("Reached authentication attempt limit for method ({0}).", (object) authenticationMethod.Name));
        }
        else
        {
          AuthenticationResult authenticationResult = authenticationMethod.Authenticate(session);
          switch (authenticationResult)
          {
            case AuthenticationResult.Success:
              authenticationException = (SshAuthenticationException) null;
              break;
            case AuthenticationResult.PartialSuccess:
              authenticationState.RecordPartialSuccess(authenticationMethod);
              if (this.TryAuthenticate(session, authenticationState, authenticationMethod.AllowedAuthentications, ref authenticationException))
              {
                authenticationResult = AuthenticationResult.Success;
                break;
              }
              break;
            case AuthenticationResult.Failure:
              authenticationState.RecordFailure(authenticationMethod);
              authenticationException = new SshAuthenticationException(string.Format("Permission denied ({0}).", (object) authenticationMethod.Name));
              break;
          }
          if (authenticationResult == AuthenticationResult.Success)
            return true;
        }
      }
      return false;
    }

    private class AuthenticationState
    {
      private readonly IList<IAuthenticationMethod> _supportedAuthenticationMethods;
      private readonly Dictionary<IAuthenticationMethod, int> _authenticationMethodPartialSuccessRegister;
      private readonly List<IAuthenticationMethod> _failedAuthenticationMethods;

      public AuthenticationState(
        IList<IAuthenticationMethod> supportedAuthenticationMethods)
      {
        this._supportedAuthenticationMethods = supportedAuthenticationMethods;
        this._failedAuthenticationMethods = new List<IAuthenticationMethod>();
        this._authenticationMethodPartialSuccessRegister = new Dictionary<IAuthenticationMethod, int>();
      }

      public void RecordFailure(IAuthenticationMethod authenticationMethod) => this._failedAuthenticationMethods.Add(authenticationMethod);

      public void RecordPartialSuccess(IAuthenticationMethod authenticationMethod)
      {
        int num1;
        if (this._authenticationMethodPartialSuccessRegister.TryGetValue(authenticationMethod, out num1))
        {
          int num2;
          this._authenticationMethodPartialSuccessRegister[authenticationMethod] = num2 = num1 + 1;
        }
        else
          this._authenticationMethodPartialSuccessRegister.Add(authenticationMethod, 1);
      }

      public int GetPartialSuccessCount(IAuthenticationMethod authenticationMethod)
      {
        int num;
        return this._authenticationMethodPartialSuccessRegister.TryGetValue(authenticationMethod, out num) ? num : 0;
      }

      public List<IAuthenticationMethod> GetSupportedAuthenticationMethods(
        string[] allowedAuthenticationMethods)
      {
        List<IAuthenticationMethod> authenticationMethods = new List<IAuthenticationMethod>();
        foreach (IAuthenticationMethod authenticationMethod in (IEnumerable<IAuthenticationMethod>) this._supportedAuthenticationMethods)
        {
          string name = authenticationMethod.Name;
          for (int index = 0; index < allowedAuthenticationMethods.Length; ++index)
          {
            if (allowedAuthenticationMethods[index] == name)
            {
              authenticationMethods.Add(authenticationMethod);
              break;
            }
          }
        }
        return authenticationMethods;
      }

      public IEnumerable<IAuthenticationMethod> GetActiveAuthenticationMethods(
        List<IAuthenticationMethod> matchingAuthenticationMethods)
      {
        List<IAuthenticationMethod> skippedAuthenticationMethods = new List<IAuthenticationMethod>();
        for (int i = 0; i < matchingAuthenticationMethods.Count; ++i)
        {
          IAuthenticationMethod authenticationMethod = matchingAuthenticationMethods[i];
          if (!this._failedAuthenticationMethods.Contains(authenticationMethod))
          {
            if (this._authenticationMethodPartialSuccessRegister.ContainsKey(authenticationMethod))
            {
              skippedAuthenticationMethods.Add(authenticationMethod);
            }
            else
            {
              yield return authenticationMethod;
              authenticationMethod = (IAuthenticationMethod) null;
            }
          }
        }
        foreach (IAuthenticationMethod authenticationMethod in skippedAuthenticationMethods)
          yield return authenticationMethod;
      }
    }
  }
}
