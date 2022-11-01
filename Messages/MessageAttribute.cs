// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.MessageAttribute
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Messages
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public sealed class MessageAttribute : Attribute
  {
    public string Name { get; set; }

    public byte Number { get; set; }

    public MessageAttribute(string name, byte number)
    {
      this.Name = name;
      this.Number = number;
    }
  }
}
