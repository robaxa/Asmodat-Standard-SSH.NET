// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ForwardedPortStatus
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Threading;

namespace Renci.SshNet
{
  internal class ForwardedPortStatus
  {
    private readonly int _value;
    private readonly string _name;
    public static readonly ForwardedPortStatus Stopped = new ForwardedPortStatus(1, nameof (Stopped));
    public static readonly ForwardedPortStatus Stopping = new ForwardedPortStatus(2, nameof (Stopping));
    public static readonly ForwardedPortStatus Started = new ForwardedPortStatus(3, nameof (Started));
    public static readonly ForwardedPortStatus Starting = new ForwardedPortStatus(4, nameof (Starting));

    private ForwardedPortStatus(int value, string name)
    {
      this._value = value;
      this._name = name;
    }

    public override bool Equals(object other)
    {
      if (other == null)
        return false;
      if ((object) this == other)
        return true;
      ForwardedPortStatus forwardedPortStatus = other as ForwardedPortStatus;
      return !(forwardedPortStatus == (ForwardedPortStatus) null) && forwardedPortStatus._value == this._value;
    }

    public static bool operator ==(ForwardedPortStatus left, ForwardedPortStatus right) => (object) left == null ? (object) right == null : left.Equals((object) right);

    public static bool operator !=(ForwardedPortStatus left, ForwardedPortStatus right) => !(left == right);

    public override int GetHashCode() => this._value;

    public override string ToString() => this._name;

    public static bool ToStopping(ref ForwardedPortStatus status)
    {
      ForwardedPortStatus forwardedPortStatus1 = Interlocked.CompareExchange<ForwardedPortStatus>(ref status, ForwardedPortStatus.Stopping, ForwardedPortStatus.Started);
      if (forwardedPortStatus1 == ForwardedPortStatus.Stopping || forwardedPortStatus1 == ForwardedPortStatus.Stopped)
        return false;
      if (status == ForwardedPortStatus.Stopping)
        return true;
      ForwardedPortStatus forwardedPortStatus2 = Interlocked.CompareExchange<ForwardedPortStatus>(ref status, ForwardedPortStatus.Stopping, ForwardedPortStatus.Starting);
      if (forwardedPortStatus2 == ForwardedPortStatus.Stopping || forwardedPortStatus2 == ForwardedPortStatus.Stopped)
        return false;
      if (status == ForwardedPortStatus.Stopping)
        return true;
      throw new InvalidOperationException(string.Format("Forwarded port cannot transition from '{0}' to '{1}'.", (object) forwardedPortStatus2, (object) ForwardedPortStatus.Stopping));
    }

    public static bool ToStarting(ref ForwardedPortStatus status)
    {
      ForwardedPortStatus forwardedPortStatus = Interlocked.CompareExchange<ForwardedPortStatus>(ref status, ForwardedPortStatus.Starting, ForwardedPortStatus.Stopped);
      if (forwardedPortStatus == ForwardedPortStatus.Starting || forwardedPortStatus == ForwardedPortStatus.Started)
        return false;
      if (status == ForwardedPortStatus.Starting)
        return true;
      throw new InvalidOperationException(string.Format("Forwarded port cannot transition from '{0}' to '{1}'.", (object) forwardedPortStatus, (object) ForwardedPortStatus.Starting));
    }
  }
}
