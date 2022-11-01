// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Sftp.SftpMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Globalization;
using System.IO;

namespace Renci.SshNet.Sftp
{
  internal abstract class SftpMessage : SshData
  {
    protected override int BufferCapacity => 5;

    public abstract SftpMessageTypes SftpMessageType { get; }

    protected override void LoadData()
    {
    }

    protected override void SaveData() => this.Write((byte) this.SftpMessageType);

    protected override void WriteBytes(SshDataStream stream)
    {
      long position1 = stream.Position;
      stream.Seek(4L, SeekOrigin.Current);
      base.WriteBytes(stream);
      long position2 = stream.Position;
      long num = position2 - position1 - 4L;
      stream.Position = position1;
      stream.Write((uint) num);
      stream.Position = position2;
    }

    protected SftpFileAttributes ReadAttributes() => SftpFileAttributes.FromBytes(this.DataStream);

    public override string ToString() => string.Format((IFormatProvider) CultureInfo.CurrentCulture, "SFTP Message : {0}", (object) this.SftpMessageType);
  }
}
