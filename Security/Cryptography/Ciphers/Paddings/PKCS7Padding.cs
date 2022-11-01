// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.Paddings.PKCS7Padding
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Paddings
{
  public class PKCS7Padding : CipherPadding
  {
    public override byte[] Pad(int blockSize, byte[] input, int offset, int length)
    {
      int paddinglength = blockSize - length % blockSize;
      return this.Pad(input, offset, length, paddinglength);
    }

    public override byte[] Pad(byte[] input, int offset, int length, int paddinglength)
    {
      byte[] dst = new byte[length + paddinglength];
      Buffer.BlockCopy((Array) input, offset, (Array) dst, 0, length);
      for (int index = 0; index < paddinglength; ++index)
        dst[length + index] = (byte) paddinglength;
      return dst;
    }
  }
}
