// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.DesCipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public class DesCipher : BlockCipher
  {
    private int[] _encryptionKey;
    private int[] _decryptionKey;
    private static readonly short[] Bytebit = new short[8]
    {
      (short) 128,
      (short) 64,
      (short) 32,
      (short) 16,
      (short) 8,
      (short) 4,
      (short) 2,
      (short) 1
    };
    private static readonly int[] Bigbyte = new int[24]
    {
      8388608,
      4194304,
      2097152,
      1048576,
      524288,
      262144,
      131072,
      65536,
      32768,
      16384,
      8192,
      4096,
      2048,
      1024,
      512,
      256,
      128,
      64,
      32,
      16,
      8,
      4,
      2,
      1
    };
    private static readonly byte[] Pc1 = new byte[56]
    {
      (byte) 56,
      (byte) 48,
      (byte) 40,
      (byte) 32,
      (byte) 24,
      (byte) 16,
      (byte) 8,
      (byte) 0,
      (byte) 57,
      (byte) 49,
      (byte) 41,
      (byte) 33,
      (byte) 25,
      (byte) 17,
      (byte) 9,
      (byte) 1,
      (byte) 58,
      (byte) 50,
      (byte) 42,
      (byte) 34,
      (byte) 26,
      (byte) 18,
      (byte) 10,
      (byte) 2,
      (byte) 59,
      (byte) 51,
      (byte) 43,
      (byte) 35,
      (byte) 62,
      (byte) 54,
      (byte) 46,
      (byte) 38,
      (byte) 30,
      (byte) 22,
      (byte) 14,
      (byte) 6,
      (byte) 61,
      (byte) 53,
      (byte) 45,
      (byte) 37,
      (byte) 29,
      (byte) 21,
      (byte) 13,
      (byte) 5,
      (byte) 60,
      (byte) 52,
      (byte) 44,
      (byte) 36,
      (byte) 28,
      (byte) 20,
      (byte) 12,
      (byte) 4,
      (byte) 27,
      (byte) 19,
      (byte) 11,
      (byte) 3
    };
    private static readonly byte[] Totrot = new byte[16]
    {
      (byte) 1,
      (byte) 2,
      (byte) 4,
      (byte) 6,
      (byte) 8,
      (byte) 10,
      (byte) 12,
      (byte) 14,
      (byte) 15,
      (byte) 17,
      (byte) 19,
      (byte) 21,
      (byte) 23,
      (byte) 25,
      (byte) 27,
      (byte) 28
    };
    private static readonly byte[] Pc2 = new byte[48]
    {
      (byte) 13,
      (byte) 16,
      (byte) 10,
      (byte) 23,
      (byte) 0,
      (byte) 4,
      (byte) 2,
      (byte) 27,
      (byte) 14,
      (byte) 5,
      (byte) 20,
      (byte) 9,
      (byte) 22,
      (byte) 18,
      (byte) 11,
      (byte) 3,
      (byte) 25,
      (byte) 7,
      (byte) 15,
      (byte) 6,
      (byte) 26,
      (byte) 19,
      (byte) 12,
      (byte) 1,
      (byte) 40,
      (byte) 51,
      (byte) 30,
      (byte) 36,
      (byte) 46,
      (byte) 54,
      (byte) 29,
      (byte) 39,
      (byte) 50,
      (byte) 44,
      (byte) 32,
      (byte) 47,
      (byte) 43,
      (byte) 48,
      (byte) 38,
      (byte) 55,
      (byte) 33,
      (byte) 52,
      (byte) 45,
      (byte) 41,
      (byte) 49,
      (byte) 35,
      (byte) 28,
      (byte) 31
    };
    private static readonly uint[] Sp1 = new uint[64]
    {
      16843776U,
      0U,
      65536U,
      16843780U,
      16842756U,
      66564U,
      4U,
      65536U,
      1024U,
      16843776U,
      16843780U,
      1024U,
      16778244U,
      16842756U,
      16777216U,
      4U,
      1028U,
      16778240U,
      16778240U,
      66560U,
      66560U,
      16842752U,
      16842752U,
      16778244U,
      65540U,
      16777220U,
      16777220U,
      65540U,
      0U,
      1028U,
      66564U,
      16777216U,
      65536U,
      16843780U,
      4U,
      16842752U,
      16843776U,
      16777216U,
      16777216U,
      1024U,
      16842756U,
      65536U,
      66560U,
      16777220U,
      1024U,
      4U,
      16778244U,
      66564U,
      16843780U,
      65540U,
      16842752U,
      16778244U,
      16777220U,
      1028U,
      66564U,
      16843776U,
      1028U,
      16778240U,
      16778240U,
      0U,
      65540U,
      66560U,
      0U,
      16842756U
    };
    private static readonly uint[] Sp2 = new uint[64]
    {
      2148565024U,
      2147516416U,
      32768U,
      1081376U,
      1048576U,
      32U,
      2148532256U,
      2147516448U,
      2147483680U,
      2148565024U,
      2148564992U,
      2147483648U,
      2147516416U,
      1048576U,
      32U,
      2148532256U,
      1081344U,
      1048608U,
      2147516448U,
      0U,
      2147483648U,
      32768U,
      1081376U,
      2148532224U,
      1048608U,
      2147483680U,
      0U,
      1081344U,
      32800U,
      2148564992U,
      2148532224U,
      32800U,
      0U,
      1081376U,
      2148532256U,
      1048576U,
      2147516448U,
      2148532224U,
      2148564992U,
      32768U,
      2148532224U,
      2147516416U,
      32U,
      2148565024U,
      1081376U,
      32U,
      32768U,
      2147483648U,
      32800U,
      2148564992U,
      1048576U,
      2147483680U,
      1048608U,
      2147516448U,
      2147483680U,
      1048608U,
      1081344U,
      0U,
      2147516416U,
      32800U,
      2147483648U,
      2148532256U,
      2148565024U,
      1081344U
    };
    private static readonly uint[] Sp3 = new uint[64]
    {
      520U,
      134349312U,
      0U,
      134348808U,
      134218240U,
      0U,
      131592U,
      134218240U,
      131080U,
      134217736U,
      134217736U,
      131072U,
      134349320U,
      131080U,
      134348800U,
      520U,
      134217728U,
      8U,
      134349312U,
      512U,
      131584U,
      134348800U,
      134348808U,
      131592U,
      134218248U,
      131584U,
      131072U,
      134218248U,
      8U,
      134349320U,
      512U,
      134217728U,
      134349312U,
      134217728U,
      131080U,
      520U,
      131072U,
      134349312U,
      134218240U,
      0U,
      512U,
      131080U,
      134349320U,
      134218240U,
      134217736U,
      512U,
      0U,
      134348808U,
      134218248U,
      131072U,
      134217728U,
      134349320U,
      8U,
      131592U,
      131584U,
      134217736U,
      134348800U,
      134218248U,
      520U,
      134348800U,
      131592U,
      8U,
      134348808U,
      131584U
    };
    private static readonly uint[] Sp4 = new uint[64]
    {
      8396801U,
      8321U,
      8321U,
      128U,
      8396928U,
      8388737U,
      8388609U,
      8193U,
      0U,
      8396800U,
      8396800U,
      8396929U,
      129U,
      0U,
      8388736U,
      8388609U,
      1U,
      8192U,
      8388608U,
      8396801U,
      128U,
      8388608U,
      8193U,
      8320U,
      8388737U,
      1U,
      8320U,
      8388736U,
      8192U,
      8396928U,
      8396929U,
      129U,
      8388736U,
      8388609U,
      8396800U,
      8396929U,
      129U,
      0U,
      0U,
      8396800U,
      8320U,
      8388736U,
      8388737U,
      1U,
      8396801U,
      8321U,
      8321U,
      128U,
      8396929U,
      129U,
      1U,
      8192U,
      8388609U,
      8193U,
      8396928U,
      8388737U,
      8193U,
      8320U,
      8388608U,
      8396801U,
      128U,
      8388608U,
      8192U,
      8396928U
    };
    private static readonly uint[] Sp5 = new uint[64]
    {
      256U,
      34078976U,
      34078720U,
      1107296512U,
      524288U,
      256U,
      1073741824U,
      34078720U,
      1074266368U,
      524288U,
      33554688U,
      1074266368U,
      1107296512U,
      1107820544U,
      524544U,
      1073741824U,
      33554432U,
      1074266112U,
      1074266112U,
      0U,
      1073742080U,
      1107820800U,
      1107820800U,
      33554688U,
      1107820544U,
      1073742080U,
      0U,
      1107296256U,
      34078976U,
      33554432U,
      1107296256U,
      524544U,
      524288U,
      1107296512U,
      256U,
      33554432U,
      1073741824U,
      34078720U,
      1107296512U,
      1074266368U,
      33554688U,
      1073741824U,
      1107820544U,
      34078976U,
      1074266368U,
      256U,
      33554432U,
      1107820544U,
      1107820800U,
      524544U,
      1107296256U,
      1107820800U,
      34078720U,
      0U,
      1074266112U,
      1107296256U,
      524544U,
      33554688U,
      1073742080U,
      524288U,
      0U,
      1074266112U,
      34078976U,
      1073742080U
    };
    private static readonly uint[] Sp6 = new uint[64]
    {
      536870928U,
      541065216U,
      16384U,
      541081616U,
      541065216U,
      16U,
      541081616U,
      4194304U,
      536887296U,
      4210704U,
      4194304U,
      536870928U,
      4194320U,
      536887296U,
      536870912U,
      16400U,
      0U,
      4194320U,
      536887312U,
      16384U,
      4210688U,
      536887312U,
      16U,
      541065232U,
      541065232U,
      0U,
      4210704U,
      541081600U,
      16400U,
      4210688U,
      541081600U,
      536870912U,
      536887296U,
      16U,
      541065232U,
      4210688U,
      541081616U,
      4194304U,
      16400U,
      536870928U,
      4194304U,
      536887296U,
      536870912U,
      16400U,
      536870928U,
      541081616U,
      4210688U,
      541065216U,
      4210704U,
      541081600U,
      0U,
      541065232U,
      16U,
      16384U,
      541065216U,
      4210704U,
      16384U,
      4194320U,
      536887312U,
      0U,
      541081600U,
      536870912U,
      4194320U,
      536887312U
    };
    private static readonly uint[] Sp7 = new uint[64]
    {
      2097152U,
      69206018U,
      67110914U,
      0U,
      2048U,
      67110914U,
      2099202U,
      69208064U,
      69208066U,
      2097152U,
      0U,
      67108866U,
      2U,
      67108864U,
      69206018U,
      2050U,
      67110912U,
      2099202U,
      2097154U,
      67110912U,
      67108866U,
      69206016U,
      69208064U,
      2097154U,
      69206016U,
      2048U,
      2050U,
      69208066U,
      2099200U,
      2U,
      67108864U,
      2099200U,
      67108864U,
      2099200U,
      2097152U,
      67110914U,
      67110914U,
      69206018U,
      69206018U,
      2U,
      2097154U,
      67108864U,
      67110912U,
      2097152U,
      69208064U,
      2050U,
      2099202U,
      69208064U,
      2050U,
      67108866U,
      69208066U,
      69206016U,
      2099200U,
      0U,
      2U,
      69208066U,
      0U,
      2099202U,
      69206016U,
      2048U,
      67108866U,
      67110912U,
      2048U,
      2097154U
    };
    private static readonly uint[] Sp8 = new uint[64]
    {
      268439616U,
      4096U,
      262144U,
      268701760U,
      268435456U,
      268439616U,
      64U,
      268435456U,
      262208U,
      268697600U,
      268701760U,
      266240U,
      268701696U,
      266304U,
      4096U,
      64U,
      268697600U,
      268435520U,
      268439552U,
      4160U,
      266240U,
      262208U,
      268697664U,
      268701696U,
      4160U,
      0U,
      0U,
      268697664U,
      268435520U,
      268439552U,
      266304U,
      262144U,
      266304U,
      262144U,
      268701696U,
      4096U,
      64U,
      268697664U,
      4096U,
      266304U,
      268439552U,
      64U,
      268435520U,
      268697600U,
      268697664U,
      268435456U,
      262144U,
      268439616U,
      0U,
      268701760U,
      262208U,
      268435520U,
      268697600U,
      268439552U,
      268439616U,
      0U,
      268701760U,
      266240U,
      266240U,
      4160U,
      4160U,
      262208U,
      268435456U,
      268701696U
    };

    public DesCipher(byte[] key, CipherMode mode, CipherPadding padding)
      : base(key, (byte) 8, mode, padding)
    {
    }

    public override int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputOffset + (int) this.BlockSize > inputBuffer.Length)
        throw new IndexOutOfRangeException("input buffer too short");
      if (outputOffset + (int) this.BlockSize > outputBuffer.Length)
        throw new IndexOutOfRangeException("output buffer too short");
      if (this._encryptionKey == null)
        this._encryptionKey = this.GenerateWorkingKey(true, this.Key);
      DesCipher.DesFunc(this._encryptionKey, inputBuffer, inputOffset, outputBuffer, outputOffset);
      return (int) this.BlockSize;
    }

    public override int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputOffset + (int) this.BlockSize > inputBuffer.Length)
        throw new IndexOutOfRangeException("input buffer too short");
      if (outputOffset + (int) this.BlockSize > outputBuffer.Length)
        throw new IndexOutOfRangeException("output buffer too short");
      if (this._decryptionKey == null)
        this._decryptionKey = this.GenerateWorkingKey(false, this.Key);
      DesCipher.DesFunc(this._decryptionKey, inputBuffer, inputOffset, outputBuffer, outputOffset);
      return (int) this.BlockSize;
    }

    protected int[] GenerateWorkingKey(bool encrypting, byte[] key)
    {
      this.ValidateKey();
      int[] workingKey = new int[32];
      bool[] flagArray1 = new bool[56];
      bool[] flagArray2 = new bool[56];
      for (int index = 0; index < 56; ++index)
      {
        int num = (int) DesCipher.Pc1[index];
        flagArray1[index] = ((uint) key[(int) ((uint) num >> 3)] & (uint) DesCipher.Bytebit[num & 7]) > 0U;
      }
      for (int index1 = 0; index1 < 16; ++index1)
      {
        int index2 = !encrypting ? 15 - index1 << 1 : index1 << 1;
        int index3 = index2 + 1;
        workingKey[index2] = workingKey[index3] = 0;
        for (int index4 = 0; index4 < 28; ++index4)
        {
          int index5 = index4 + (int) DesCipher.Totrot[index1];
          flagArray2[index4] = index5 >= 28 ? flagArray1[index5 - 28] : flagArray1[index5];
        }
        for (int index6 = 28; index6 < 56; ++index6)
        {
          int index7 = index6 + (int) DesCipher.Totrot[index1];
          flagArray2[index6] = index7 >= 56 ? flagArray1[index7 - 28] : flagArray1[index7];
        }
        for (int index8 = 0; index8 < 24; ++index8)
        {
          if (flagArray2[(int) DesCipher.Pc2[index8]])
            workingKey[index2] |= DesCipher.Bigbyte[index8];
          if (flagArray2[(int) DesCipher.Pc2[index8 + 24]])
            workingKey[index3] |= DesCipher.Bigbyte[index8];
        }
      }
      for (int index = 0; index != 32; index += 2)
      {
        int num1 = workingKey[index];
        int num2 = workingKey[index + 1];
        workingKey[index] = (num1 & 16515072) << 6 | (num1 & 4032) << 10 | (int) ((uint) (num2 & 16515072) >> 10) | (int) ((uint) (num2 & 4032) >> 6);
        workingKey[index + 1] = (num1 & 258048) << 12 | (num1 & 63) << 16 | (int) ((uint) (num2 & 258048) >> 4) | num2 & 63;
      }
      return workingKey;
    }

    protected virtual void ValidateKey()
    {
      int num = this.Key.Length * 8;
      if (num != 64)
        throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", (object) num));
    }

    protected static void DesFunc(
      int[] wKey,
      byte[] input,
      int inOff,
      byte[] outBytes,
      int outOff)
    {
      uint uint32_1 = Pack.BigEndianToUInt32(input, inOff);
      uint uint32_2 = Pack.BigEndianToUInt32(input, inOff + 4);
      uint num1 = (uint) (((int) (uint32_1 >> 4) ^ (int) uint32_2) & 252645135);
      uint num2 = uint32_2 ^ num1;
      uint num3 = uint32_1 ^ num1 << 4;
      uint num4 = (uint) (((int) (num3 >> 16) ^ (int) num2) & (int) ushort.MaxValue);
      uint num5 = num2 ^ num4;
      uint num6 = num3 ^ num4 << 16;
      uint num7 = (uint) (((int) (num5 >> 2) ^ (int) num6) & 858993459);
      uint num8 = num6 ^ num7;
      uint num9 = num5 ^ num7 << 2;
      uint num10 = (uint) (((int) (num9 >> 8) ^ (int) num8) & 16711935);
      uint num11 = num8 ^ num10;
      uint num12 = num9 ^ num10 << 8;
      uint num13 = num12 << 1 | num12 >> 31;
      uint num14 = (uint) (((int) num11 ^ (int) num13) & -1431655766);
      uint num15 = num11 ^ num14;
      uint num16 = num13 ^ num14;
      uint num17 = num15 << 1 | num15 >> 31;
      for (int index = 0; index < 8; ++index)
      {
        uint num18 = (num16 << 28 | num16 >> 4) ^ (uint) wKey[index * 4];
        uint num19 = DesCipher.Sp7[(int) num18 & 63] | DesCipher.Sp5[(int) (num18 >> 8) & 63] | DesCipher.Sp3[(int) (num18 >> 16) & 63] | DesCipher.Sp1[(int) (num18 >> 24) & 63];
        uint num20 = num16 ^ (uint) wKey[index * 4 + 1];
        uint num21 = num19 | DesCipher.Sp8[(int) num20 & 63] | DesCipher.Sp6[(int) (num20 >> 8) & 63] | DesCipher.Sp4[(int) (num20 >> 16) & 63] | DesCipher.Sp2[(int) (num20 >> 24) & 63];
        num17 ^= num21;
        uint num22 = (num17 << 28 | num17 >> 4) ^ (uint) wKey[index * 4 + 2];
        uint num23 = DesCipher.Sp7[(int) num22 & 63] | DesCipher.Sp5[(int) (num22 >> 8) & 63] | DesCipher.Sp3[(int) (num22 >> 16) & 63] | DesCipher.Sp1[(int) (num22 >> 24) & 63];
        uint num24 = num17 ^ (uint) wKey[index * 4 + 3];
        uint num25 = num23 | DesCipher.Sp8[(int) num24 & 63] | DesCipher.Sp6[(int) (num24 >> 8) & 63] | DesCipher.Sp4[(int) (num24 >> 16) & 63] | DesCipher.Sp2[(int) (num24 >> 24) & 63];
        num16 ^= num25;
      }
      uint num26 = num16 << 31 | num16 >> 1;
      uint num27 = (uint) (((int) num17 ^ (int) num26) & -1431655766);
      uint num28 = num17 ^ num27;
      uint num29 = num26 ^ num27;
      uint num30 = num28 << 31 | num28 >> 1;
      uint num31 = (uint) (((int) (num30 >> 8) ^ (int) num29) & 16711935);
      uint num32 = num29 ^ num31;
      uint num33 = num30 ^ num31 << 8;
      uint num34 = (uint) (((int) (num33 >> 2) ^ (int) num32) & 858993459);
      uint num35 = num32 ^ num34;
      uint num36 = num33 ^ num34 << 2;
      uint num37 = (uint) (((int) (num35 >> 16) ^ (int) num36) & (int) ushort.MaxValue);
      uint num38 = num36 ^ num37;
      uint num39 = num35 ^ num37 << 16;
      uint num40 = (uint) (((int) (num39 >> 4) ^ (int) num38) & 252645135);
      uint num41 = num38 ^ num40;
      Pack.UInt32ToBigEndian(num39 ^ num40 << 4, outBytes, outOff);
      Pack.UInt32ToBigEndian(num41, outBytes, outOff + 4);
    }
  }
}
