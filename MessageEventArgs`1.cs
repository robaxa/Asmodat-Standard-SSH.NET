// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.MessageEventArgs`1
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet
{
  public class MessageEventArgs<T> : EventArgs
  {
    public T Message { get; private set; }

    public MessageEventArgs(T message) => this.Message = (object) message != null ? message : throw new ArgumentNullException(nameof (message));
  }
}
