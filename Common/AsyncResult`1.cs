// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.AsyncResult`1
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Common
{
  public abstract class AsyncResult<TResult> : AsyncResult
  {
    private TResult _result;

    protected AsyncResult(AsyncCallback asyncCallback, object state)
      : base(asyncCallback, state)
    {
    }

    public void SetAsCompleted(TResult result, bool completedSynchronously)
    {
      this._result = result;
      this.SetAsCompleted((Exception) null, completedSynchronously);
    }

    public TResult EndInvoke()
    {
      base.EndInvoke();
      return this._result;
    }
  }
}
