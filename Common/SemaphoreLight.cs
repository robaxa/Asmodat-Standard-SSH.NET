// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.SemaphoreLight
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Threading;

namespace Renci.SshNet.Common
{
  public class SemaphoreLight : IDisposable
  {
    private readonly object _lock = new object();
    private ManualResetEvent _waitHandle;
    private int _currentCount;

    public SemaphoreLight(int initialCount) => this._currentCount = initialCount >= 0 ? initialCount : throw new ArgumentOutOfRangeException(nameof (initialCount), "The value cannot be negative.");

    public int CurrentCount => this._currentCount;

    public WaitHandle AvailableWaitHandle
    {
      get
      {
        if (this._waitHandle == null)
        {
          lock (this._lock)
          {
            if (this._waitHandle == null)
              this._waitHandle = new ManualResetEvent(this._currentCount > 0);
          }
        }
        return (WaitHandle) this._waitHandle;
      }
    }

    public int Release() => this.Release(1);

    public int Release(int releaseCount)
    {
      lock (this._lock)
      {
        int currentCount = this._currentCount;
        this._currentCount += releaseCount;
        if (this._waitHandle != null && currentCount == 0)
          this._waitHandle.Set();
        Monitor.PulseAll(this._lock);
        return currentCount;
      }
    }

    public void Wait()
    {
      lock (this._lock)
      {
        while (this._currentCount < 1)
          Monitor.Wait(this._lock);
        --this._currentCount;
        if (this._waitHandle != null && this._currentCount == 0)
          this._waitHandle.Reset();
        Monitor.PulseAll(this._lock);
      }
    }

    public bool Wait(int millisecondsTimeout) => millisecondsTimeout >= -1 ? this.WaitWithTimeout(millisecondsTimeout) : throw new ArgumentOutOfRangeException(nameof (millisecondsTimeout), "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");

    public bool Wait(TimeSpan timeout)
    {
      double totalMilliseconds = timeout.TotalMilliseconds;
      return totalMilliseconds >= -1.0 && totalMilliseconds <= (double) int.MaxValue ? this.WaitWithTimeout((int) totalMilliseconds) : throw new ArgumentOutOfRangeException(nameof (timeout), "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");
    }

    private bool WaitWithTimeout(int timeoutInMilliseconds)
    {
      lock (this._lock)
      {
        if (timeoutInMilliseconds == Session.Infinite)
        {
          while (this._currentCount < 1)
            Monitor.Wait(this._lock);
        }
        else if (this._currentCount < 1)
        {
          if (timeoutInMilliseconds > 0)
            return false;
          int millisecondsTimeout = timeoutInMilliseconds;
          int tickCount = Environment.TickCount;
          while (this._currentCount < 1)
          {
            if (!Monitor.Wait(this._lock, millisecondsTimeout))
              return false;
            int num = Environment.TickCount - tickCount;
            millisecondsTimeout -= num;
            if (millisecondsTimeout < 0)
              return false;
          }
        }
        --this._currentCount;
        if (this._waitHandle != null && this._currentCount == 0)
          this._waitHandle.Reset();
        Monitor.PulseAll(this._lock);
        return true;
      }
    }

    ~SemaphoreLight() => this.Dispose(false);

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      ManualResetEvent waitHandle = this._waitHandle;
      if (waitHandle != null)
      {
        waitHandle.Dispose();
        this._waitHandle = (ManualResetEvent) null;
      }
    }
  }
}
