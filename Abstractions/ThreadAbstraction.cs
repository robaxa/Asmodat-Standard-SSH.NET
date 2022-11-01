﻿// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Abstractions.ThreadAbstraction
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Threading;

namespace Renci.SshNet.Abstractions
{
  internal static class ThreadAbstraction
  {
    public static void Sleep(int millisecondsTimeout) => Thread.Sleep(millisecondsTimeout);

    public static void ExecuteThreadLongRunning(Action action) => new Thread((ThreadStart) (() => action())).Start();

    public static void ExecuteThread(Action action)
    {
      if (action == null)
        throw new ArgumentNullException(nameof (action));
      ThreadPool.QueueUserWorkItem((WaitCallback) (o => action()));
    }
  }
}
