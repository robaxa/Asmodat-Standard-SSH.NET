// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Connection.X11ForwardingRequestInfo
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
  internal class X11ForwardingRequestInfo : RequestInfo
  {
    private byte[] _authenticationProtocol;
    public const string Name = "x11-req";

    public override string RequestName => "x11-req";

    public bool IsSingleConnection { get; set; }

    public string AuthenticationProtocol
    {
      get => SshData.Ascii.GetString(this._authenticationProtocol, 0, this._authenticationProtocol.Length);
      private set => this._authenticationProtocol = SshData.Ascii.GetBytes(value);
    }

    public byte[] AuthenticationCookie { get; set; }

    public uint ScreenNumber { get; set; }

    protected override int BufferCapacity => base.BufferCapacity + 1 + 4 + this._authenticationProtocol.Length + 4 + this.AuthenticationCookie.Length + 4;

    public X11ForwardingRequestInfo() => this.WantReply = true;

    public X11ForwardingRequestInfo(
      bool isSingleConnection,
      string protocol,
      byte[] cookie,
      uint screenNumber)
      : this()
    {
      this.IsSingleConnection = isSingleConnection;
      this.AuthenticationProtocol = protocol;
      this.AuthenticationCookie = cookie;
      this.ScreenNumber = screenNumber;
    }

    protected override void LoadData()
    {
      base.LoadData();
      this.IsSingleConnection = this.ReadBoolean();
      this._authenticationProtocol = this.ReadBinary();
      this.AuthenticationCookie = this.ReadBinary();
      this.ScreenNumber = this.ReadUInt32();
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.IsSingleConnection);
      this.WriteBinaryString(this._authenticationProtocol);
      this.WriteBinaryString(this.AuthenticationCookie);
      this.Write(this.ScreenNumber);
    }
  }
}
