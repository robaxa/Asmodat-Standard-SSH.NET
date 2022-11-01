// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.BannerMessage
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
  [Renci.SshNet.Messages.Message("SSH_MSG_USERAUTH_BANNER", 53)]
  public class BannerMessage : Renci.SshNet.Messages.Message
  {
    private byte[] _message;
    private byte[] _language;

    public string Message => SshData.Utf8.GetString(this._message, 0, this._message.Length);

    public string Language => SshData.Utf8.GetString(this._language, 0, this._language.Length);

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._message.Length + 4 + this._language.Length;

    internal override void Process(Session session) => session.OnUserAuthenticationBannerReceived(this);

    protected override void LoadData()
    {
      this._message = this.ReadBinary();
      this._language = this.ReadBinary();
    }

    protected override void SaveData()
    {
      this.WriteBinaryString(this._message);
      this.WriteBinaryString(this._language);
    }
  }
}
