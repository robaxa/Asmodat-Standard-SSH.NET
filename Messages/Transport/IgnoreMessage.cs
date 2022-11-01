// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Transport.IgnoreMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Globalization;

namespace Renci.SshNet.Messages.Transport
{
  [Message("SSH_MSG_IGNORE", 2)]
  public class IgnoreMessage : Message
  {
    internal const byte MessageNumber = 2;

    public byte[] Data { get; private set; }

    public IgnoreMessage() => this.Data = Array<byte>.Empty;

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.Data.Length;

    public IgnoreMessage(byte[] data) => this.Data = data != null ? data : throw new ArgumentNullException(nameof (data));

    protected override void LoadData()
    {
      uint length = this.ReadUInt32();
      if (length > (uint) int.MaxValue)
        throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Data longer than {0} is not supported.", (object) int.MaxValue));
      if ((long) length > this.DataStream.Length - this.DataStream.Position)
      {
        DiagnosticAbstraction.Log("SSH_MSG_IGNORE: Length exceeds data bytes, data ignored.");
        this.Data = Array<byte>.Empty;
      }
      else
        this.Data = this.ReadBytes((int) length);
    }

    protected override void SaveData() => this.WriteBinaryString(this.Data);

    internal override void Process(Session session) => session.OnIgnoreReceived(this);
  }
}
