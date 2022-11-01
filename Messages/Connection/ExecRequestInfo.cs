// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.ExecRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;
using System.Text;

namespace Renci.SshNet.Messages.Connection
{
  internal class ExecRequestInfo : RequestInfo
  {
    private byte[] _command;
    public const string Name = "exec";

    public override string RequestName => "exec";

    public string Command => this.Encoding.GetString(this._command, 0, this._command.Length);

    public Encoding Encoding { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._command.Length;

    public ExecRequestInfo() => this.WantReply = true;

    public ExecRequestInfo(string command, Encoding encoding)
      : this()
    {
      if (command == null)
        throw new ArgumentNullException(nameof (command));
      this._command = encoding != null ? encoding.GetBytes(command) : throw new ArgumentNullException(nameof (encoding));
      this.Encoding = encoding;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this._command = this.ReadBinary();
      this.Encoding = SshData.Utf8;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this._command);
    }
  }
}
