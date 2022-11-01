// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.RequestMessageHost
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
  internal class RequestMessageHost : RequestMessage
  {
    public byte[] PublicKeyAlgorithm { get; private set; }

    public byte[] PublicHostKey { get; private set; }

    public byte[] ClientHostName { get; private set; }

    public byte[] ClientUsername { get; private set; }

    public byte[] Signature { get; private set; }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this.PublicKeyAlgorithm.Length + 4 + this.PublicHostKey.Length + 4 + this.ClientHostName.Length + 4 + this.ClientUsername.Length + 4 + this.Signature.Length;

    public RequestMessageHost(
      ServiceName serviceName,
      string username,
      string publicKeyAlgorithm,
      byte[] publicHostKey,
      string clientHostName,
      string clientUsername,
      byte[] signature)
      : base(serviceName, username, "hostbased")
    {
      this.PublicKeyAlgorithm = SshData.Ascii.GetBytes(publicKeyAlgorithm);
      this.PublicHostKey = publicHostKey;
      this.ClientHostName = SshData.Ascii.GetBytes(clientHostName);
      this.ClientUsername = SshData.Utf8.GetBytes(clientUsername);
      this.Signature = signature;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.WriteBinaryString(this.PublicKeyAlgorithm);
      this.WriteBinaryString(this.PublicHostKey);
      this.WriteBinaryString(this.ClientHostName);
      this.WriteBinaryString(this.ClientUsername);
      this.WriteBinaryString(this.Signature);
    }
  }
}
