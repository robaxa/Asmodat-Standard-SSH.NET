// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ISubsystemSession
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Threading;

namespace Renci.SshNet
{
  internal interface ISubsystemSession : IDisposable
  {
    int OperationTimeout { get; }

    bool IsOpen { get; }

    void Connect();

    void Disconnect();

    void WaitOnHandle(WaitHandle waitHandle, int millisecondsTimeout);

    bool WaitOne(WaitHandle waitHandle, int millisecondsTimeout);

    int WaitAny(WaitHandle waitHandleA, WaitHandle waitHandleB, int millisecondsTimeout);

    int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout);

    WaitHandle[] CreateWaitHandleArray(params WaitHandle[] waitHandles);

    WaitHandle[] CreateWaitHandleArray(WaitHandle waitHandle1, WaitHandle waitHandle2);
  }
}
