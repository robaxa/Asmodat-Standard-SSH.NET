// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.CommandAsyncResult
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Threading;

namespace Renci.SshNet
{
  public class CommandAsyncResult : IAsyncResult
  {
    internal CommandAsyncResult()
    {
    }

    public int BytesReceived { get; set; }

    public int BytesSent { get; set; }

    public object AsyncState { get; internal set; }

    public WaitHandle AsyncWaitHandle { get; internal set; }

    public bool CompletedSynchronously { get; internal set; }

    public bool IsCompleted { get; internal set; }

    internal bool EndCalled { get; set; }
  }
}
