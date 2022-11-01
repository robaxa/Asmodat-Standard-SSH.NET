// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.PseudoTerminalRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System.Collections.Generic;

namespace Renci.SshNet.Messages.Connection
{
  internal class PseudoTerminalRequestInfo : RequestInfo
  {
    public const string Name = "pty-req";

    public override string RequestName => "pty-req";

    public string EnvironmentVariable { get; set; }

    public uint Columns { get; set; }

    public uint Rows { get; set; }

    public uint PixelWidth { get; set; }

    public uint PixelHeight { get; set; }

    public IDictionary<TerminalModes, uint> TerminalModeValues { get; set; }

    protected override int BufferCapacity => -1;

    public PseudoTerminalRequestInfo() => this.WantReply = true;

    public PseudoTerminalRequestInfo(
      string environmentVariable,
      uint columns,
      uint rows,
      uint width,
      uint height,
      IDictionary<TerminalModes, uint> terminalModeValues)
      : this()
    {
      this.EnvironmentVariable = environmentVariable;
      this.Columns = columns;
      this.Rows = rows;
      this.PixelWidth = width;
      this.PixelHeight = height;
      this.TerminalModeValues = terminalModeValues;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.EnvironmentVariable);
      this.Write(this.Columns);
      this.Write(this.Rows);
      this.Write(this.PixelWidth);
      this.Write(this.PixelHeight);
      if (this.TerminalModeValues != null && this.TerminalModeValues.Count > 0)
      {
        this.Write((uint) (this.TerminalModeValues.Count * 5 + 1));
        foreach (KeyValuePair<TerminalModes, uint> terminalModeValue in (IEnumerable<KeyValuePair<TerminalModes, uint>>) this.TerminalModeValues)
        {
          this.Write((byte) terminalModeValue.Key);
          this.Write(terminalModeValue.Value);
        }
        this.Write((byte) 0);
      }
      else
        this.Write(0U);
    }
  }
}
