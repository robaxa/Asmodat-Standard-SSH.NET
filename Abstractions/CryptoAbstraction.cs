// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Abstractions.CryptoAbstraction
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System.Security.Cryptography;

namespace Renci.SshNet.Abstractions
{
  internal static class CryptoAbstraction
  {
    private static readonly RandomNumberGenerator Randomizer = CryptoAbstraction.CreateRandomNumberGenerator();

    public static byte[] GenerateRandom(int length)
    {
      byte[] data = new byte[length];
      CryptoAbstraction.GenerateRandom(data);
      return data;
    }

    public static void GenerateRandom(byte[] data) => CryptoAbstraction.Randomizer.GetBytes(data);

    public static RandomNumberGenerator CreateRandomNumberGenerator() => RandomNumberGenerator.Create();

    public static MD5 CreateMD5() => MD5.Create();

    public static SHA1 CreateSHA1() => SHA1.Create();

    public static SHA256 CreateSHA256() => SHA256.Create();

    public static SHA384 CreateSHA384() => SHA384.Create();

    public static SHA512 CreateSHA512() => SHA512.Create();

    public static System.Security.Cryptography.HMACMD5 CreateHMACMD5(byte[] key) => new System.Security.Cryptography.HMACMD5(key);

    public static Renci.SshNet.Security.Cryptography.HMACMD5 CreateHMACMD5(
      byte[] key,
      int hashSize)
    {
      return new Renci.SshNet.Security.Cryptography.HMACMD5(key, hashSize);
    }

    public static System.Security.Cryptography.HMACSHA1 CreateHMACSHA1(byte[] key) => new System.Security.Cryptography.HMACSHA1(key);

    public static Renci.SshNet.Security.Cryptography.HMACSHA1 CreateHMACSHA1(
      byte[] key,
      int hashSize)
    {
      return new Renci.SshNet.Security.Cryptography.HMACSHA1(key, hashSize);
    }

    public static System.Security.Cryptography.HMACSHA256 CreateHMACSHA256(byte[] key) => new System.Security.Cryptography.HMACSHA256(key);

    public static Renci.SshNet.Security.Cryptography.HMACSHA256 CreateHMACSHA256(
      byte[] key,
      int hashSize)
    {
      return new Renci.SshNet.Security.Cryptography.HMACSHA256(key, hashSize);
    }

    public static System.Security.Cryptography.HMACSHA384 CreateHMACSHA384(byte[] key) => new System.Security.Cryptography.HMACSHA384(key);

    public static Renci.SshNet.Security.Cryptography.HMACSHA384 CreateHMACSHA384(
      byte[] key,
      int hashSize)
    {
      return new Renci.SshNet.Security.Cryptography.HMACSHA384(key, hashSize);
    }

    public static System.Security.Cryptography.HMACSHA512 CreateHMACSHA512(byte[] key) => new System.Security.Cryptography.HMACSHA512(key);

    public static Renci.SshNet.Security.Cryptography.HMACSHA512 CreateHMACSHA512(
      byte[] key,
      int hashSize)
    {
      return new Renci.SshNet.Security.Cryptography.HMACSHA512(key, hashSize);
    }
  }
}
