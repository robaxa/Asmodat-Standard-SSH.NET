// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.CipherPadding
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public abstract class CipherPadding
  {
    public byte[] Pad(int blockSize, byte[] input) => this.Pad(blockSize, input, 0, input.Length);

    public abstract byte[] Pad(int blockSize, byte[] input, int offset, int length);

    public byte[] Pad(byte[] input, int paddinglength) => this.Pad(input, 0, input.Length, paddinglength);

    public abstract byte[] Pad(byte[] input, int offset, int length, int paddinglength);
  }
}
