// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.AsyncResult
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Threading;

namespace Renci.SshNet.Common
{
  public abstract class AsyncResult : IAsyncResult
  {
    private readonly AsyncCallback _asyncCallback;
    private readonly object _asyncState;
    private const int StatePending = 0;
    private const int StateCompletedSynchronously = 1;
    private const int StateCompletedAsynchronously = 2;
    private int _completedState = 0;
    private ManualResetEvent _asyncWaitHandle;
    private Exception _exception;

    public bool EndInvokeCalled { get; private set; }

    protected AsyncResult(AsyncCallback asyncCallback, object state)
    {
      this._asyncCallback = asyncCallback;
      this._asyncState = state;
    }

    public void SetAsCompleted(Exception exception, bool completedSynchronously)
    {
      this._exception = exception;
      if (Interlocked.Exchange(ref this._completedState, completedSynchronously ? 1 : 2) != 0)
        throw new InvalidOperationException("You can set a result only once");
      if (this._asyncWaitHandle != null)
        this._asyncWaitHandle.Set();
      if (this._asyncCallback == null)
        return;
      this._asyncCallback((IAsyncResult) this);
    }

    internal void EndInvoke()
    {
      if (!this.IsCompleted)
      {
        this.AsyncWaitHandle.WaitOne();
        this._asyncWaitHandle = (ManualResetEvent) null;
        this.AsyncWaitHandle.Dispose();
      }
      this.EndInvokeCalled = true;
      if (this._exception != null)
        throw this._exception;
    }

    public object AsyncState => this._asyncState;

    public bool CompletedSynchronously => this._completedState == 1;

    public WaitHandle AsyncWaitHandle
    {
      get
      {
        if (this._asyncWaitHandle == null)
        {
          bool isCompleted = this.IsCompleted;
          ManualResetEvent manualResetEvent = new ManualResetEvent(isCompleted);
          if (Interlocked.CompareExchange<ManualResetEvent>(ref this._asyncWaitHandle, manualResetEvent, (ManualResetEvent) null) != null)
            manualResetEvent.Dispose();
          else if (!isCompleted && this.IsCompleted)
            this._asyncWaitHandle.Set();
        }
        return (WaitHandle) this._asyncWaitHandle;
      }
    }

    public bool IsCompleted => this._completedState != 0;
  }
}
