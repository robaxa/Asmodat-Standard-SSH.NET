// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Authentication.RequestMessagePublicKey
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
  public class RequestMessagePublicKey : RequestMessage
  {
    public byte[] PublicKeyAlgorithmName { get; private set; }

    public byte[] PublicKeyData { get; private set; }

    public byte[] Signature { get; set; }

    protected override int BufferCapacity
    {
      get
      {
        int bufferCapacity = base.BufferCapacity + 1 + 4 + this.PublicKeyAlgorithmName.Length + 4 + this.PublicKeyData.Length;
        if (this.Signature != null)
          bufferCapacity = bufferCapacity + 4 + this.Signature.Length;
        return bufferCapacity;
      }
    }

    public RequestMessagePublicKey(
      ServiceName serviceName,
      string username,
      string keyAlgorithmName,
      byte[] keyData)
      : base(serviceName, username, "publickey")
    {
      this.PublicKeyAlgorithmName = SshData.Ascii.GetBytes(keyAlgorithmName);
      this.PublicKeyData = keyData;
    }

    public RequestMessagePublicKey(
      ServiceName serviceName,
      string username,
      string keyAlgorithmName,
      byte[] keyData,
      byte[] signature)
      : this(serviceName, username, keyAlgorithmName, keyData)
    {
      this.Signature = signature;
    }

    protected override void SaveData()
    {
      base.SaveData();
      this.Write(this.Signature != null);
      this.WriteBinaryString(this.PublicKeyAlgorithmName);
      this.WriteBinaryString(this.PublicKeyData);
      if (this.Signature == null)
        return;
      this.WriteBinaryString(this.Signature);
    }
  }
}
