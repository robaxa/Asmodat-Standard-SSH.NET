// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.SymmetricCipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Security.Cryptography
{
  public abstract class SymmetricCipher : Cipher
  {
    protected byte[] Key { get; private set; }

    protected SymmetricCipher(byte[] key) => this.Key = key != null ? key : throw new ArgumentNullException(nameof (key));

    public abstract int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset);

    public abstract int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset);
  }
}
