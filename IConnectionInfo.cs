// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.IConnectionInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet
{
  internal interface IConnectionInfo
  {
    TimeSpan ChannelCloseTimeout { get; }

    IDictionary<string, RequestInfo> ChannelRequests { get; }

    Encoding Encoding { get; }

    int RetryAttempts { get; }

    TimeSpan Timeout { get; }

    event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;
  }
}
