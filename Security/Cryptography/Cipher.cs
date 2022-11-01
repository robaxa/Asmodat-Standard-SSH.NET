// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Cipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Security.Cryptography
{
  public abstract class Cipher
  {
    public abstract byte MinimumSize { get; }

    public byte[] Encrypt(byte[] input) => this.Encrypt(input, 0, input.Length);

    public abstract byte[] Encrypt(byte[] input, int offset, int length);

    public abstract byte[] Decrypt(byte[] input);

    public abstract byte[] Decrypt(byte[] input, int offset, int length);
  }
}
