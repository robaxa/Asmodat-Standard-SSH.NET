// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.RequestMessagePassword
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Messages.Authentication
{
  internal class RequestMessagePassword : RequestMessage
  {
    public byte[] Password { get; private set; }

    public byte[] NewPassword { get; private set; }

    protected override int BufferCapacity
    {
      get
      {
        int bufferCapacity = base.BufferCapacity + 1 + 4 + this.Password.Length;
        if (this.NewPassword != null)
          bufferCapacity = bufferCapacity + 4 + this.NewPassword.Length;
        return bufferCapacity;
      }
    }

    public RequestMessagePassword(ServiceName serviceName, string username, byte[] password)
      : base(serviceName, username, nameof (password))
    {
      this.Password = password;
    }

    public RequestMessagePassword(
      ServiceName serviceName,
      string username,
      byte[] password,
      byte[] newPassword)
      : this(serviceName, username, password)
    {
      this.NewPassword = newPassword;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.NewPassword != null);
      this.WriteBinaryString(this.Password);
      if (this.NewPassword == null)
        return;
      this.WriteBinaryString(this.NewPassword);
    }
  }
}
