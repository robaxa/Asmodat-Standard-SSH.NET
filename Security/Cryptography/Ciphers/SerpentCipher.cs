// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.Cryptography.Ciphers.SerpentCipher
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
  public sealed class SerpentCipher : BlockCipher
  {
    private const int Rounds = 32;
    private const int Phi = -1640531527;
    private readonly int[] _workingKey;
    private int _x0;
    private int _x1;
    private int _x2;
    private int _x3;

    public SerpentCipher(byte[] key, CipherMode mode, CipherPadding padding)
      : base(key, (byte) 16, mode, padding)
    {
      int num1 = key.Length * 8;
      int num2;
      switch (num1)
      {
        case 128:
        case 192:
          num2 = 0;
          break;
        default:
          num2 = num1 != 256 ? 1 : 0;
          break;
      }
      if (num2 != 0)
        throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", (object) num1));
      this._workingKey = this.MakeWorkingKey(key);
    }

    public override int EncryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputCount != (int) this.BlockSize)
        throw new ArgumentException(nameof (inputCount));
      this._x3 = SerpentCipher.BytesToWord(inputBuffer, inputOffset);
      this._x2 = SerpentCipher.BytesToWord(inputBuffer, inputOffset + 4);
      this._x1 = SerpentCipher.BytesToWord(inputBuffer, inputOffset + 8);
      this._x0 = SerpentCipher.BytesToWord(inputBuffer, inputOffset + 12);
      this.Sb0(this._workingKey[0] ^ this._x0, this._workingKey[1] ^ this._x1, this._workingKey[2] ^ this._x2, this._workingKey[3] ^ this._x3);
      this.LT();
      this.Sb1(this._workingKey[4] ^ this._x0, this._workingKey[5] ^ this._x1, this._workingKey[6] ^ this._x2, this._workingKey[7] ^ this._x3);
      this.LT();
      this.Sb2(this._workingKey[8] ^ this._x0, this._workingKey[9] ^ this._x1, this._workingKey[10] ^ this._x2, this._workingKey[11] ^ this._x3);
      this.LT();
      this.Sb3(this._workingKey[12] ^ this._x0, this._workingKey[13] ^ this._x1, this._workingKey[14] ^ this._x2, this._workingKey[15] ^ this._x3);
      this.LT();
      this.Sb4(this._workingKey[16] ^ this._x0, this._workingKey[17] ^ this._x1, this._workingKey[18] ^ this._x2, this._workingKey[19] ^ this._x3);
      this.LT();
      this.Sb5(this._workingKey[20] ^ this._x0, this._workingKey[21] ^ this._x1, this._workingKey[22] ^ this._x2, this._workingKey[23] ^ this._x3);
      this.LT();
      this.Sb6(this._workingKey[24] ^ this._x0, this._workingKey[25] ^ this._x1, this._workingKey[26] ^ this._x2, this._workingKey[27] ^ this._x3);
      this.LT();
      this.Sb7(this._workingKey[28] ^ this._x0, this._workingKey[29] ^ this._x1, this._workingKey[30] ^ this._x2, this._workingKey[31] ^ this._x3);
      this.LT();
      this.Sb0(this._workingKey[32] ^ this._x0, this._workingKey[33] ^ this._x1, this._workingKey[34] ^ this._x2, this._workingKey[35] ^ this._x3);
      this.LT();
      this.Sb1(this._workingKey[36] ^ this._x0, this._workingKey[37] ^ this._x1, this._workingKey[38] ^ this._x2, this._workingKey[39] ^ this._x3);
      this.LT();
      this.Sb2(this._workingKey[40] ^ this._x0, this._workingKey[41] ^ this._x1, this._workingKey[42] ^ this._x2, this._workingKey[43] ^ this._x3);
      this.LT();
      this.Sb3(this._workingKey[44] ^ this._x0, this._workingKey[45] ^ this._x1, this._workingKey[46] ^ this._x2, this._workingKey[47] ^ this._x3);
      this.LT();
      this.Sb4(this._workingKey[48] ^ this._x0, this._workingKey[49] ^ this._x1, this._workingKey[50] ^ this._x2, this._workingKey[51] ^ this._x3);
      this.LT();
      this.Sb5(this._workingKey[52] ^ this._x0, this._workingKey[53] ^ this._x1, this._workingKey[54] ^ this._x2, this._workingKey[55] ^ this._x3);
      this.LT();
      this.Sb6(this._workingKey[56] ^ this._x0, this._workingKey[57] ^ this._x1, this._workingKey[58] ^ this._x2, this._workingKey[59] ^ this._x3);
      this.LT();
      this.Sb7(this._workingKey[60] ^ this._x0, this._workingKey[61] ^ this._x1, this._workingKey[62] ^ this._x2, this._workingKey[63] ^ this._x3);
      this.LT();
      this.Sb0(this._workingKey[64] ^ this._x0, this._workingKey[65] ^ this._x1, this._workingKey[66] ^ this._x2, this._workingKey[67] ^ this._x3);
      this.LT();
      this.Sb1(this._workingKey[68] ^ this._x0, this._workingKey[69] ^ this._x1, this._workingKey[70] ^ this._x2, this._workingKey[71] ^ this._x3);
      this.LT();
      this.Sb2(this._workingKey[72] ^ this._x0, this._workingKey[73] ^ this._x1, this._workingKey[74] ^ this._x2, this._workingKey[75] ^ this._x3);
      this.LT();
      this.Sb3(this._workingKey[76] ^ this._x0, this._workingKey[77] ^ this._x1, this._workingKey[78] ^ this._x2, this._workingKey[79] ^ this._x3);
      this.LT();
      this.Sb4(this._workingKey[80] ^ this._x0, this._workingKey[81] ^ this._x1, this._workingKey[82] ^ this._x2, this._workingKey[83] ^ this._x3);
      this.LT();
      this.Sb5(this._workingKey[84] ^ this._x0, this._workingKey[85] ^ this._x1, this._workingKey[86] ^ this._x2, this._workingKey[87] ^ this._x3);
      this.LT();
      this.Sb6(this._workingKey[88] ^ this._x0, this._workingKey[89] ^ this._x1, this._workingKey[90] ^ this._x2, this._workingKey[91] ^ this._x3);
      this.LT();
      this.Sb7(this._workingKey[92] ^ this._x0, this._workingKey[93] ^ this._x1, this._workingKey[94] ^ this._x2, this._workingKey[95] ^ this._x3);
      this.LT();
      this.Sb0(this._workingKey[96] ^ this._x0, this._workingKey[97] ^ this._x1, this._workingKey[98] ^ this._x2, this._workingKey[99] ^ this._x3);
      this.LT();
      this.Sb1(this._workingKey[100] ^ this._x0, this._workingKey[101] ^ this._x1, this._workingKey[102] ^ this._x2, this._workingKey[103] ^ this._x3);
      this.LT();
      this.Sb2(this._workingKey[104] ^ this._x0, this._workingKey[105] ^ this._x1, this._workingKey[106] ^ this._x2, this._workingKey[107] ^ this._x3);
      this.LT();
      this.Sb3(this._workingKey[108] ^ this._x0, this._workingKey[109] ^ this._x1, this._workingKey[110] ^ this._x2, this._workingKey[111] ^ this._x3);
      this.LT();
      this.Sb4(this._workingKey[112] ^ this._x0, this._workingKey[113] ^ this._x1, this._workingKey[114] ^ this._x2, this._workingKey[115] ^ this._x3);
      this.LT();
      this.Sb5(this._workingKey[116] ^ this._x0, this._workingKey[117] ^ this._x1, this._workingKey[118] ^ this._x2, this._workingKey[119] ^ this._x3);
      this.LT();
      this.Sb6(this._workingKey[120] ^ this._x0, this._workingKey[121] ^ this._x1, this._workingKey[122] ^ this._x2, this._workingKey[123] ^ this._x3);
      this.LT();
      this.Sb7(this._workingKey[124] ^ this._x0, this._workingKey[125] ^ this._x1, this._workingKey[126] ^ this._x2, this._workingKey[(int) sbyte.MaxValue] ^ this._x3);
      SerpentCipher.WordToBytes(this._workingKey[131] ^ this._x3, outputBuffer, outputOffset);
      SerpentCipher.WordToBytes(this._workingKey[130] ^ this._x2, outputBuffer, outputOffset + 4);
      SerpentCipher.WordToBytes(this._workingKey[129] ^ this._x1, outputBuffer, outputOffset + 8);
      SerpentCipher.WordToBytes(this._workingKey[128] ^ this._x0, outputBuffer, outputOffset + 12);
      return (int) this.BlockSize;
    }

    public override int DecryptBlock(
      byte[] inputBuffer,
      int inputOffset,
      int inputCount,
      byte[] outputBuffer,
      int outputOffset)
    {
      if (inputCount != (int) this.BlockSize)
        throw new ArgumentException(nameof (inputCount));
      this._x3 = this._workingKey[131] ^ SerpentCipher.BytesToWord(inputBuffer, inputOffset);
      this._x2 = this._workingKey[130] ^ SerpentCipher.BytesToWord(inputBuffer, inputOffset + 4);
      this._x1 = this._workingKey[129] ^ SerpentCipher.BytesToWord(inputBuffer, inputOffset + 8);
      this._x0 = this._workingKey[128] ^ SerpentCipher.BytesToWord(inputBuffer, inputOffset + 12);
      this.Ib7(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[124];
      this._x1 ^= this._workingKey[125];
      this._x2 ^= this._workingKey[126];
      this._x3 ^= this._workingKey[(int) sbyte.MaxValue];
      this.InverseLT();
      this.Ib6(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[120];
      this._x1 ^= this._workingKey[121];
      this._x2 ^= this._workingKey[122];
      this._x3 ^= this._workingKey[123];
      this.InverseLT();
      this.Ib5(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[116];
      this._x1 ^= this._workingKey[117];
      this._x2 ^= this._workingKey[118];
      this._x3 ^= this._workingKey[119];
      this.InverseLT();
      this.Ib4(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[112];
      this._x1 ^= this._workingKey[113];
      this._x2 ^= this._workingKey[114];
      this._x3 ^= this._workingKey[115];
      this.InverseLT();
      this.Ib3(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[108];
      this._x1 ^= this._workingKey[109];
      this._x2 ^= this._workingKey[110];
      this._x3 ^= this._workingKey[111];
      this.InverseLT();
      this.Ib2(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[104];
      this._x1 ^= this._workingKey[105];
      this._x2 ^= this._workingKey[106];
      this._x3 ^= this._workingKey[107];
      this.InverseLT();
      this.Ib1(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[100];
      this._x1 ^= this._workingKey[101];
      this._x2 ^= this._workingKey[102];
      this._x3 ^= this._workingKey[103];
      this.InverseLT();
      this.Ib0(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[96];
      this._x1 ^= this._workingKey[97];
      this._x2 ^= this._workingKey[98];
      this._x3 ^= this._workingKey[99];
      this.InverseLT();
      this.Ib7(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[92];
      this._x1 ^= this._workingKey[93];
      this._x2 ^= this._workingKey[94];
      this._x3 ^= this._workingKey[95];
      this.InverseLT();
      this.Ib6(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[88];
      this._x1 ^= this._workingKey[89];
      this._x2 ^= this._workingKey[90];
      this._x3 ^= this._workingKey[91];
      this.InverseLT();
      this.Ib5(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[84];
      this._x1 ^= this._workingKey[85];
      this._x2 ^= this._workingKey[86];
      this._x3 ^= this._workingKey[87];
      this.InverseLT();
      this.Ib4(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[80];
      this._x1 ^= this._workingKey[81];
      this._x2 ^= this._workingKey[82];
      this._x3 ^= this._workingKey[83];
      this.InverseLT();
      this.Ib3(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[76];
      this._x1 ^= this._workingKey[77];
      this._x2 ^= this._workingKey[78];
      this._x3 ^= this._workingKey[79];
      this.InverseLT();
      this.Ib2(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[72];
      this._x1 ^= this._workingKey[73];
      this._x2 ^= this._workingKey[74];
      this._x3 ^= this._workingKey[75];
      this.InverseLT();
      this.Ib1(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[68];
      this._x1 ^= this._workingKey[69];
      this._x2 ^= this._workingKey[70];
      this._x3 ^= this._workingKey[71];
      this.InverseLT();
      this.Ib0(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[64];
      this._x1 ^= this._workingKey[65];
      this._x2 ^= this._workingKey[66];
      this._x3 ^= this._workingKey[67];
      this.InverseLT();
      this.Ib7(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[60];
      this._x1 ^= this._workingKey[61];
      this._x2 ^= this._workingKey[62];
      this._x3 ^= this._workingKey[63];
      this.InverseLT();
      this.Ib6(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[56];
      this._x1 ^= this._workingKey[57];
      this._x2 ^= this._workingKey[58];
      this._x3 ^= this._workingKey[59];
      this.InverseLT();
      this.Ib5(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[52];
      this._x1 ^= this._workingKey[53];
      this._x2 ^= this._workingKey[54];
      this._x3 ^= this._workingKey[55];
      this.InverseLT();
      this.Ib4(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[48];
      this._x1 ^= this._workingKey[49];
      this._x2 ^= this._workingKey[50];
      this._x3 ^= this._workingKey[51];
      this.InverseLT();
      this.Ib3(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[44];
      this._x1 ^= this._workingKey[45];
      this._x2 ^= this._workingKey[46];
      this._x3 ^= this._workingKey[47];
      this.InverseLT();
      this.Ib2(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[40];
      this._x1 ^= this._workingKey[41];
      this._x2 ^= this._workingKey[42];
      this._x3 ^= this._workingKey[43];
      this.InverseLT();
      this.Ib1(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[36];
      this._x1 ^= this._workingKey[37];
      this._x2 ^= this._workingKey[38];
      this._x3 ^= this._workingKey[39];
      this.InverseLT();
      this.Ib0(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[32];
      this._x1 ^= this._workingKey[33];
      this._x2 ^= this._workingKey[34];
      this._x3 ^= this._workingKey[35];
      this.InverseLT();
      this.Ib7(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[28];
      this._x1 ^= this._workingKey[29];
      this._x2 ^= this._workingKey[30];
      this._x3 ^= this._workingKey[31];
      this.InverseLT();
      this.Ib6(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[24];
      this._x1 ^= this._workingKey[25];
      this._x2 ^= this._workingKey[26];
      this._x3 ^= this._workingKey[27];
      this.InverseLT();
      this.Ib5(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[20];
      this._x1 ^= this._workingKey[21];
      this._x2 ^= this._workingKey[22];
      this._x3 ^= this._workingKey[23];
      this.InverseLT();
      this.Ib4(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[16];
      this._x1 ^= this._workingKey[17];
      this._x2 ^= this._workingKey[18];
      this._x3 ^= this._workingKey[19];
      this.InverseLT();
      this.Ib3(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[12];
      this._x1 ^= this._workingKey[13];
      this._x2 ^= this._workingKey[14];
      this._x3 ^= this._workingKey[15];
      this.InverseLT();
      this.Ib2(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[8];
      this._x1 ^= this._workingKey[9];
      this._x2 ^= this._workingKey[10];
      this._x3 ^= this._workingKey[11];
      this.InverseLT();
      this.Ib1(this._x0, this._x1, this._x2, this._x3);
      this._x0 ^= this._workingKey[4];
      this._x1 ^= this._workingKey[5];
      this._x2 ^= this._workingKey[6];
      this._x3 ^= this._workingKey[7];
      this.InverseLT();
      this.Ib0(this._x0, this._x1, this._x2, this._x3);
      SerpentCipher.WordToBytes(this._x3 ^ this._workingKey[3], outputBuffer, outputOffset);
      SerpentCipher.WordToBytes(this._x2 ^ this._workingKey[2], outputBuffer, outputOffset + 4);
      SerpentCipher.WordToBytes(this._x1 ^ this._workingKey[1], outputBuffer, outputOffset + 8);
      SerpentCipher.WordToBytes(this._x0 ^ this._workingKey[0], outputBuffer, outputOffset + 12);
      return (int) this.BlockSize;
    }

    private int[] MakeWorkingKey(byte[] key)
    {
      int[] src = new int[16];
      int num = 0;
      int srcOff;
      for (srcOff = key.Length - 4; srcOff > 0; srcOff -= 4)
        src[num++] = SerpentCipher.BytesToWord(key, srcOff);
      if (srcOff != 0)
        throw new ArgumentException("key must be a multiple of 4 bytes");
      int[] numArray = src;
      int index1 = num;
      int index2 = index1 + 1;
      int word = SerpentCipher.BytesToWord(key, 0);
      numArray[index1] = word;
      if (index2 < 8)
        src[index2] = 1;
      int[] dst = new int[132];
      for (int index3 = 8; index3 < 16; ++index3)
        src[index3] = SerpentCipher.RotateLeft(src[index3 - 8] ^ src[index3 - 5] ^ src[index3 - 3] ^ src[index3 - 1] ^ -1640531527 ^ index3 - 8, 11);
      Buffer.BlockCopy((Array) src, 8, (Array) dst, 0, 8);
      for (int index4 = 8; index4 < 132; ++index4)
        dst[index4] = SerpentCipher.RotateLeft(dst[index4 - 8] ^ dst[index4 - 5] ^ dst[index4 - 3] ^ dst[index4 - 1] ^ -1640531527 ^ index4, 11);
      this.Sb3(dst[0], dst[1], dst[2], dst[3]);
      dst[0] = this._x0;
      dst[1] = this._x1;
      dst[2] = this._x2;
      dst[3] = this._x3;
      this.Sb2(dst[4], dst[5], dst[6], dst[7]);
      dst[4] = this._x0;
      dst[5] = this._x1;
      dst[6] = this._x2;
      dst[7] = this._x3;
      this.Sb1(dst[8], dst[9], dst[10], dst[11]);
      dst[8] = this._x0;
      dst[9] = this._x1;
      dst[10] = this._x2;
      dst[11] = this._x3;
      this.Sb0(dst[12], dst[13], dst[14], dst[15]);
      dst[12] = this._x0;
      dst[13] = this._x1;
      dst[14] = this._x2;
      dst[15] = this._x3;
      this.Sb7(dst[16], dst[17], dst[18], dst[19]);
      dst[16] = this._x0;
      dst[17] = this._x1;
      dst[18] = this._x2;
      dst[19] = this._x3;
      this.Sb6(dst[20], dst[21], dst[22], dst[23]);
      dst[20] = this._x0;
      dst[21] = this._x1;
      dst[22] = this._x2;
      dst[23] = this._x3;
      this.Sb5(dst[24], dst[25], dst[26], dst[27]);
      dst[24] = this._x0;
      dst[25] = this._x1;
      dst[26] = this._x2;
      dst[27] = this._x3;
      this.Sb4(dst[28], dst[29], dst[30], dst[31]);
      dst[28] = this._x0;
      dst[29] = this._x1;
      dst[30] = this._x2;
      dst[31] = this._x3;
      this.Sb3(dst[32], dst[33], dst[34], dst[35]);
      dst[32] = this._x0;
      dst[33] = this._x1;
      dst[34] = this._x2;
      dst[35] = this._x3;
      this.Sb2(dst[36], dst[37], dst[38], dst[39]);
      dst[36] = this._x0;
      dst[37] = this._x1;
      dst[38] = this._x2;
      dst[39] = this._x3;
      this.Sb1(dst[40], dst[41], dst[42], dst[43]);
      dst[40] = this._x0;
      dst[41] = this._x1;
      dst[42] = this._x2;
      dst[43] = this._x3;
      this.Sb0(dst[44], dst[45], dst[46], dst[47]);
      dst[44] = this._x0;
      dst[45] = this._x1;
      dst[46] = this._x2;
      dst[47] = this._x3;
      this.Sb7(dst[48], dst[49], dst[50], dst[51]);
      dst[48] = this._x0;
      dst[49] = this._x1;
      dst[50] = this._x2;
      dst[51] = this._x3;
      this.Sb6(dst[52], dst[53], dst[54], dst[55]);
      dst[52] = this._x0;
      dst[53] = this._x1;
      dst[54] = this._x2;
      dst[55] = this._x3;
      this.Sb5(dst[56], dst[57], dst[58], dst[59]);
      dst[56] = this._x0;
      dst[57] = this._x1;
      dst[58] = this._x2;
      dst[59] = this._x3;
      this.Sb4(dst[60], dst[61], dst[62], dst[63]);
      dst[60] = this._x0;
      dst[61] = this._x1;
      dst[62] = this._x2;
      dst[63] = this._x3;
      this.Sb3(dst[64], dst[65], dst[66], dst[67]);
      dst[64] = this._x0;
      dst[65] = this._x1;
      dst[66] = this._x2;
      dst[67] = this._x3;
      this.Sb2(dst[68], dst[69], dst[70], dst[71]);
      dst[68] = this._x0;
      dst[69] = this._x1;
      dst[70] = this._x2;
      dst[71] = this._x3;
      this.Sb1(dst[72], dst[73], dst[74], dst[75]);
      dst[72] = this._x0;
      dst[73] = this._x1;
      dst[74] = this._x2;
      dst[75] = this._x3;
      this.Sb0(dst[76], dst[77], dst[78], dst[79]);
      dst[76] = this._x0;
      dst[77] = this._x1;
      dst[78] = this._x2;
      dst[79] = this._x3;
      this.Sb7(dst[80], dst[81], dst[82], dst[83]);
      dst[80] = this._x0;
      dst[81] = this._x1;
      dst[82] = this._x2;
      dst[83] = this._x3;
      this.Sb6(dst[84], dst[85], dst[86], dst[87]);
      dst[84] = this._x0;
      dst[85] = this._x1;
      dst[86] = this._x2;
      dst[87] = this._x3;
      this.Sb5(dst[88], dst[89], dst[90], dst[91]);
      dst[88] = this._x0;
      dst[89] = this._x1;
      dst[90] = this._x2;
      dst[91] = this._x3;
      this.Sb4(dst[92], dst[93], dst[94], dst[95]);
      dst[92] = this._x0;
      dst[93] = this._x1;
      dst[94] = this._x2;
      dst[95] = this._x3;
      this.Sb3(dst[96], dst[97], dst[98], dst[99]);
      dst[96] = this._x0;
      dst[97] = this._x1;
      dst[98] = this._x2;
      dst[99] = this._x3;
      this.Sb2(dst[100], dst[101], dst[102], dst[103]);
      dst[100] = this._x0;
      dst[101] = this._x1;
      dst[102] = this._x2;
      dst[103] = this._x3;
      this.Sb1(dst[104], dst[105], dst[106], dst[107]);
      dst[104] = this._x0;
      dst[105] = this._x1;
      dst[106] = this._x2;
      dst[107] = this._x3;
      this.Sb0(dst[108], dst[109], dst[110], dst[111]);
      dst[108] = this._x0;
      dst[109] = this._x1;
      dst[110] = this._x2;
      dst[111] = this._x3;
      this.Sb7(dst[112], dst[113], dst[114], dst[115]);
      dst[112] = this._x0;
      dst[113] = this._x1;
      dst[114] = this._x2;
      dst[115] = this._x3;
      this.Sb6(dst[116], dst[117], dst[118], dst[119]);
      dst[116] = this._x0;
      dst[117] = this._x1;
      dst[118] = this._x2;
      dst[119] = this._x3;
      this.Sb5(dst[120], dst[121], dst[122], dst[123]);
      dst[120] = this._x0;
      dst[121] = this._x1;
      dst[122] = this._x2;
      dst[123] = this._x3;
      this.Sb4(dst[124], dst[125], dst[126], dst[(int) sbyte.MaxValue]);
      dst[124] = this._x0;
      dst[125] = this._x1;
      dst[126] = this._x2;
      dst[(int) sbyte.MaxValue] = this._x3;
      this.Sb3(dst[128], dst[129], dst[130], dst[131]);
      dst[128] = this._x0;
      dst[129] = this._x1;
      dst[130] = this._x2;
      dst[131] = this._x3;
      return dst;
    }

    private static int RotateLeft(int x, int bits) => x << bits | (int) ((uint) x >> 32 - bits);

    private static int RotateRight(int x, int bits) => (int) ((uint) x >> bits) | x << 32 - bits;

    private static int BytesToWord(byte[] src, int srcOff) => ((int) src[srcOff] & (int) byte.MaxValue) << 24 | ((int) src[srcOff + 1] & (int) byte.MaxValue) << 16 | ((int) src[srcOff + 2] & (int) byte.MaxValue) << 8 | (int) src[srcOff + 3] & (int) byte.MaxValue;

    private static void WordToBytes(int word, byte[] dst, int dstOff)
    {
      dst[dstOff + 3] = (byte) word;
      dst[dstOff + 2] = (byte) ((uint) word >> 8);
      dst[dstOff + 1] = (byte) ((uint) word >> 16);
      dst[dstOff] = (byte) ((uint) word >> 24);
    }

    private void Sb0(int a, int b, int c, int d)
    {
      int num1 = a ^ d;
      int num2 = c ^ num1;
      int num3 = b ^ num2;
      this._x3 = a & d ^ num3;
      int num4 = a ^ b & num1;
      this._x2 = num3 ^ (c | num4);
      int num5 = this._x3 & (num2 ^ num4);
      this._x1 = ~num2 ^ num5;
      this._x0 = num5 ^ ~num4;
    }

    private void Ib0(int a, int b, int c, int d)
    {
      int num1 = ~a;
      int num2 = a ^ b;
      int num3 = d ^ (num1 | num2);
      int num4 = c ^ num3;
      this._x2 = num2 ^ num4;
      int num5 = num1 ^ d & num2;
      this._x1 = num3 ^ this._x2 & num5;
      this._x3 = a & num3 ^ (num4 | this._x1);
      this._x0 = this._x3 ^ num4 ^ num5;
    }

    private void Sb1(int a, int b, int c, int d)
    {
      int num1 = b ^ ~a;
      int num2 = c ^ (a | num1);
      this._x2 = d ^ num2;
      int num3 = b ^ (d | num1);
      int num4 = num1 ^ this._x2;
      this._x3 = num4 ^ num2 & num3;
      int num5 = num2 ^ num3;
      this._x1 = this._x3 ^ num5;
      this._x0 = num2 ^ num4 & num5;
    }

    private void Ib1(int a, int b, int c, int d)
    {
      int num1 = b ^ d;
      int num2 = a ^ b & num1;
      int num3 = num1 ^ num2;
      this._x3 = c ^ num3;
      int num4 = b ^ num1 & num2;
      int num5 = this._x3 | num4;
      this._x1 = num2 ^ num5;
      int num6 = ~this._x1;
      int num7 = this._x3 ^ num4;
      this._x0 = num6 ^ num7;
      this._x2 = num3 ^ (num6 | num7);
    }

    private void Sb2(int a, int b, int c, int d)
    {
      int num1 = ~a;
      int num2 = b ^ d;
      int num3 = c & num1;
      this._x0 = num2 ^ num3;
      int num4 = c ^ num1;
      int num5 = c ^ this._x0;
      int num6 = b & num5;
      this._x3 = num4 ^ num6;
      this._x2 = a ^ (d | num6) & (this._x0 | num4);
      this._x1 = num2 ^ this._x3 ^ this._x2 ^ (d | num1);
    }

    private void Ib2(int a, int b, int c, int d)
    {
      int num1 = b ^ d;
      int num2 = ~num1;
      int num3 = a ^ c;
      int num4 = c ^ num1;
      int num5 = b & num4;
      this._x0 = num3 ^ num5;
      int num6 = a | num2;
      int num7 = d ^ num6;
      int num8 = num3 | num7;
      this._x3 = num1 ^ num8;
      int num9 = ~num4;
      int num10 = this._x0 | this._x3;
      this._x1 = num9 ^ num10;
      this._x2 = d & num9 ^ num3 ^ num10;
    }

    private void Sb3(int a, int b, int c, int d)
    {
      int num1 = a ^ b;
      int num2 = a & c;
      int num3 = a | d;
      int num4 = c ^ d;
      int num5 = num1 & num3;
      int num6 = num2 | num5;
      this._x2 = num4 ^ num6;
      int num7 = b ^ num3;
      int num8 = num6 ^ num7;
      int num9 = num4 & num8;
      this._x0 = num1 ^ num9;
      int num10 = this._x2 & this._x0;
      this._x1 = num8 ^ num10;
      this._x3 = (b | d) ^ num4 ^ num10;
    }

    private void Ib3(int a, int b, int c, int d)
    {
      int num1 = a | b;
      int num2 = b ^ c;
      int num3 = b & num2;
      int num4 = a ^ num3;
      int num5 = c ^ num4;
      int num6 = d | num4;
      this._x0 = num2 ^ num6;
      int num7 = num2 | num6;
      int num8 = d ^ num7;
      this._x2 = num5 ^ num8;
      int num9 = num1 ^ num8;
      int num10 = this._x0 & num9;
      this._x3 = num4 ^ num10;
      this._x1 = this._x3 ^ this._x0 ^ num9;
    }

    private void Sb4(int a, int b, int c, int d)
    {
      int num1 = a ^ d;
      int num2 = d & num1;
      int num3 = c ^ num2;
      int num4 = b | num3;
      this._x3 = num1 ^ num4;
      int num5 = ~b;
      int num6 = num1 | num5;
      this._x0 = num3 ^ num6;
      int num7 = a & this._x0;
      int num8 = num1 ^ num5;
      int num9 = num4 & num8;
      this._x2 = num7 ^ num9;
      this._x1 = a ^ num3 ^ num8 & this._x2;
    }

    private void Ib4(int a, int b, int c, int d)
    {
      int num1 = c | d;
      int num2 = a & num1;
      int num3 = b ^ num2;
      int num4 = a & num3;
      int num5 = c ^ num4;
      this._x1 = d ^ num5;
      int num6 = ~a;
      int num7 = num5 & this._x1;
      this._x3 = num3 ^ num7;
      int num8 = this._x1 | num6;
      int num9 = d ^ num8;
      this._x0 = this._x3 ^ num9;
      this._x2 = num3 & num9 ^ this._x1 ^ num6;
    }

    private void Sb5(int a, int b, int c, int d)
    {
      int num1 = ~a;
      int num2 = a ^ b;
      int num3 = a ^ d;
      this._x0 = c ^ num1 ^ (num2 | num3);
      int num4 = d & this._x0;
      int num5 = num2 ^ this._x0;
      this._x1 = num4 ^ num5;
      int num6 = num1 | this._x0;
      int num7 = num2 | num4;
      int num8 = num3 ^ num6;
      this._x2 = num7 ^ num8;
      this._x3 = b ^ num4 ^ this._x1 & num8;
    }

    private void Ib5(int a, int b, int c, int d)
    {
      int num1 = ~c;
      int num2 = b & num1;
      int num3 = d ^ num2;
      int num4 = a & num3;
      int num5 = b ^ num1;
      this._x3 = num4 ^ num5;
      int num6 = b | this._x3;
      int num7 = a & num6;
      this._x1 = num3 ^ num7;
      int num8 = a | d;
      int num9 = num1 ^ num6;
      this._x0 = num8 ^ num9;
      this._x2 = b & num8 ^ (num4 | a ^ c);
    }

    private void Sb6(int a, int b, int c, int d)
    {
      int num1 = ~a;
      int num2 = a ^ d;
      int num3 = b ^ num2;
      int num4 = num1 | num2;
      int num5 = c ^ num4;
      this._x1 = b ^ num5;
      int num6 = num2 | this._x1;
      int num7 = d ^ num6;
      int num8 = num5 & num7;
      this._x2 = num3 ^ num8;
      int num9 = num5 ^ num7;
      this._x0 = this._x2 ^ num9;
      this._x3 = ~num5 ^ num3 & num9;
    }

    private void Ib6(int a, int b, int c, int d)
    {
      int num1 = ~a;
      int num2 = a ^ b;
      int num3 = c ^ num2;
      int num4 = c | num1;
      int num5 = d ^ num4;
      this._x1 = num3 ^ num5;
      int num6 = num3 & num5;
      int num7 = num2 ^ num6;
      int num8 = b | num7;
      this._x3 = num5 ^ num8;
      int num9 = b | this._x3;
      this._x0 = num7 ^ num9;
      this._x2 = d & num1 ^ num3 ^ num9;
    }

    private void Sb7(int a, int b, int c, int d)
    {
      int num1 = b ^ c;
      int num2 = c & num1;
      int num3 = d ^ num2;
      int num4 = a ^ num3;
      int num5 = d | num1;
      int num6 = num4 & num5;
      this._x1 = b ^ num6;
      int num7 = num3 | this._x1;
      int num8 = a & num4;
      this._x3 = num1 ^ num8;
      int num9 = num4 ^ num7;
      int num10 = this._x3 & num9;
      this._x2 = num3 ^ num10;
      this._x0 = ~num9 ^ this._x3 & this._x2;
    }

    private void Ib7(int a, int b, int c, int d)
    {
      int num1 = c | a & b;
      int num2 = d & (a | b);
      this._x3 = num1 ^ num2;
      int num3 = ~d;
      int num4 = b ^ num2;
      int num5 = num4 | this._x3 ^ num3;
      this._x1 = a ^ num5;
      this._x0 = c ^ num4 ^ (d | this._x1);
      this._x2 = num1 ^ this._x1 ^ this._x0 ^ a & this._x3;
    }

    private void LT()
    {
      int num1 = SerpentCipher.RotateLeft(this._x0, 13);
      int num2 = SerpentCipher.RotateLeft(this._x2, 3);
      int x1 = this._x1 ^ num1 ^ num2;
      int x2 = this._x3 ^ num2 ^ num1 << 3;
      this._x1 = SerpentCipher.RotateLeft(x1, 1);
      this._x3 = SerpentCipher.RotateLeft(x2, 7);
      this._x0 = SerpentCipher.RotateLeft(num1 ^ this._x1 ^ this._x3, 5);
      this._x2 = SerpentCipher.RotateLeft(num2 ^ this._x3 ^ this._x1 << 7, 22);
    }

    private void InverseLT()
    {
      int x1 = SerpentCipher.RotateRight(this._x2, 22) ^ this._x3 ^ this._x1 << 7;
      int x2 = SerpentCipher.RotateRight(this._x0, 5) ^ this._x1 ^ this._x3;
      int num1 = SerpentCipher.RotateRight(this._x3, 7);
      int num2 = SerpentCipher.RotateRight(this._x1, 1);
      this._x3 = num1 ^ x1 ^ x2 << 3;
      this._x1 = num2 ^ x2 ^ x1;
      this._x2 = SerpentCipher.RotateRight(x1, 3);
      this._x0 = SerpentCipher.RotateRight(x2, 13);
    }
  }
}
