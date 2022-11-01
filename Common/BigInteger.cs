// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.BigInteger
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Renci.SshNet.Common
{
  public struct BigInteger : 
    IComparable,
    IFormattable,
    IComparable<BigInteger>,
    IEquatable<BigInteger>
  {
    private static readonly BigInteger ZeroSingleton = new BigInteger(0);
    private static readonly BigInteger OneSingleton = new BigInteger(1);
    private static readonly BigInteger MinusOneSingleton = new BigInteger(-1);
    private const ulong Base = 4294967296;
    private const int Bias = 1075;
    private const int DecimalSignMask = -2147483648;
    private readonly uint[] _data;
    private readonly short _sign;

    public int BitLength
    {
      get
      {
        if (this._sign == (short) 0)
          return 0;
        int index = this._data.Length - 1;
        while (this._data[index] == 0U)
          --index;
        int num = BigInteger.BitScanBackward(this._data[index]) + 1;
        return index * 4 * 8 + num + (this._sign > (short) 0 ? 0 : 1);
      }
    }

    public static BigInteger ModInverse(BigInteger bi, BigInteger modulus)
    {
      BigInteger bigInteger1 = modulus;
      BigInteger bigInteger2 = bi % modulus;
      BigInteger bigInteger3 = (BigInteger) 0;
      BigInteger bigInteger4 = (BigInteger) 1;
      while (!bigInteger2.IsZero)
      {
        if (bigInteger2.IsOne)
          return bigInteger4;
        bigInteger3 += bigInteger1 / bigInteger2 * bigInteger4;
        bigInteger1 %= bigInteger2;
        if (!bigInteger1.IsZero)
        {
          if (bigInteger1.IsOne)
            return modulus - bigInteger3;
          bigInteger4 += bigInteger2 / bigInteger1 * bigInteger3;
          bigInteger2 %= bigInteger1;
        }
        else
          break;
      }
      return (BigInteger) 0;
    }

    public static BigInteger PositiveMod(BigInteger dividend, BigInteger divisor)
    {
      BigInteger bigInteger = dividend % divisor;
      if (bigInteger < 0L)
        bigInteger += divisor;
      return bigInteger;
    }

    public static BigInteger Random(int bitLength)
    {
      byte[] data = new byte[bitLength / 8 + (bitLength % 8 > 0 ? 1 : 0)];
      CryptoAbstraction.GenerateRandom(data);
      data[data.Length - 1] = (byte) ((uint) data[data.Length - 1] & (uint) sbyte.MaxValue);
      return new BigInteger(data);
    }

    private BigInteger(short sign, uint[] data)
    {
      this._sign = sign;
      this._data = data;
    }

    public BigInteger(int value)
    {
      if (value == 0)
      {
        this._sign = (short) 0;
        this._data = (uint[]) null;
      }
      else if (value > 0)
      {
        this._sign = (short) 1;
        this._data = new uint[1]{ (uint) value };
      }
      else
      {
        this._sign = (short) -1;
        this._data = new uint[1]{ (uint) -value };
      }
    }

    [CLSCompliant(false)]
    public BigInteger(uint value)
    {
      if (value == 0U)
      {
        this._sign = (short) 0;
        this._data = (uint[]) null;
      }
      else
      {
        this._sign = (short) 1;
        this._data = new uint[1]{ value };
      }
    }

    public BigInteger(long value)
    {
      if (value == 0L)
      {
        this._sign = (short) 0;
        this._data = (uint[]) null;
      }
      else if (value > 0L)
      {
        this._sign = (short) 1;
        uint num1 = (uint) value;
        uint num2 = (uint) (value >> 32);
        this._data = new uint[num2 != 0U ? 2 : 1];
        this._data[0] = num1;
        if (num2 <= 0U)
          return;
        this._data[1] = num2;
      }
      else
      {
        this._sign = (short) -1;
        value = -value;
        uint num3 = (uint) value;
        uint num4 = (uint) ((ulong) value >> 32);
        this._data = new uint[num4 != 0U ? 2 : 1];
        this._data[0] = num3;
        if (num4 > 0U)
          this._data[1] = num4;
      }
    }

    [CLSCompliant(false)]
    public BigInteger(ulong value)
    {
      if (value == 0UL)
      {
        this._sign = (short) 0;
        this._data = (uint[]) null;
      }
      else
      {
        this._sign = (short) 1;
        uint num1 = (uint) value;
        uint num2 = (uint) (value >> 32);
        this._data = new uint[num2 != 0U ? 2 : 1];
        this._data[0] = num1;
        if (num2 > 0U)
          this._data[1] = num2;
      }
    }

    private static bool Negative(byte[] v) => ((uint) v[7] & 128U) > 0U;

    private static ushort Exponent(byte[] v) => (ushort) ((int) (ushort) ((uint) v[7] & (uint) sbyte.MaxValue) << 4 | (int) (ushort) ((uint) v[6] & 240U) >> 4);

    private static ulong Mantissa(byte[] v) => (ulong) (uint) ((int) v[0] | (int) v[1] << 8 | (int) v[2] << 16 | (int) v[3] << 24) | (ulong) (uint) ((int) v[4] | (int) v[5] << 8 | ((int) v[6] & 15) << 16) << 32;

    public BigInteger(double value)
    {
      byte[] v = !double.IsNaN(value) && !double.IsInfinity(value) ? BitConverter.GetBytes(value) : throw new OverflowException();
      ulong num1 = BigInteger.Mantissa(v);
      if (num1 == 0UL)
      {
        int num2 = (int) BigInteger.Exponent(v);
        if (num2 == 0)
        {
          this._sign = (short) 0;
          this._data = (uint[]) null;
        }
        else
        {
          BigInteger bigInteger = (BigInteger.Negative(v) ? BigInteger.MinusOne : BigInteger.One) << num2 - 1023;
          this._sign = bigInteger._sign;
          this._data = bigInteger._data;
        }
      }
      else
      {
        int num3 = (int) BigInteger.Exponent(v);
        BigInteger bigInteger1 = (BigInteger) (num1 | 4503599627370496UL);
        BigInteger bigInteger2 = num3 > 1075 ? bigInteger1 << num3 - 1075 : bigInteger1 >> 1075 - num3;
        this._sign = BigInteger.Negative(v) ? (short) -1 : (short) 1;
        this._data = bigInteger2._data;
      }
    }

    public BigInteger(float value)
      : this((double) value)
    {
    }

    public BigInteger(Decimal value)
    {
      int[] bits = Decimal.GetBits(Decimal.Truncate(value));
      int length = 3;
      while (length > 0 && bits[length - 1] == 0)
        --length;
      if (length == 0)
      {
        this._sign = (short) 0;
        this._data = (uint[]) null;
      }
      else
      {
        this._sign = (bits[3] & int.MinValue) != 0 ? (short) -1 : (short) 1;
        this._data = new uint[length];
        this._data[0] = (uint) bits[0];
        if (length > 1)
          this._data[1] = (uint) bits[1];
        if (length <= 2)
          return;
        this._data[2] = (uint) bits[2];
      }
    }

    [CLSCompliant(false)]
    public BigInteger(byte[] value)
    {
      int num1 = value != null ? value.Length : throw new ArgumentNullException(nameof (value));
      int num2;
      switch (num1)
      {
        case 0:
          num2 = 1;
          break;
        case 1:
          num2 = value[0] == (byte) 0 ? 1 : 0;
          break;
        default:
          num2 = 0;
          break;
      }
      if (num2 != 0)
      {
        this._sign = (short) 0;
        this._data = (uint[]) null;
      }
      else
      {
        this._sign = ((uint) value[num1 - 1] & 128U) <= 0U ? (short) 1 : (short) -1;
        if (this._sign == (short) 1)
        {
          while (value[num1 - 1] == (byte) 0)
          {
            if (--num1 == 0)
            {
              this._sign = (short) 0;
              this._data = (uint[]) null;
              return;
            }
          }
          int length;
          int num3 = length = num1 / 4;
          if ((num1 & 3) != 0)
            ++length;
          this._data = new uint[length];
          int num4 = 0;
          for (int index1 = 0; index1 < num3; ++index1)
          {
            uint[] data = this._data;
            int index2 = index1;
            byte[] numArray1 = value;
            int index3 = num4;
            int num5 = index3 + 1;
            int num6 = (int) numArray1[index3];
            byte[] numArray2 = value;
            int index4 = num5;
            int num7 = index4 + 1;
            int num8 = (int) numArray2[index4] << 8;
            int num9 = num6 | num8;
            byte[] numArray3 = value;
            int index5 = num7;
            int num10 = index5 + 1;
            int num11 = (int) numArray3[index5] << 16;
            int num12 = num9 | num11;
            byte[] numArray4 = value;
            int index6 = num10;
            num4 = index6 + 1;
            int num13 = (int) numArray4[index6] << 24;
            int num14 = num12 | num13;
            data[index2] = (uint) num14;
          }
          int num15 = num1 & 3;
          if (num15 <= 0)
            return;
          int index7 = this._data.Length - 1;
          for (int index8 = 0; index8 < num15; ++index8)
            this._data[index7] |= (uint) value[num4++] << index8 * 8;
        }
        else
        {
          int length;
          int num16 = length = num1 / 4;
          if ((num1 & 3) != 0)
            ++length;
          this._data = new uint[length];
          uint num17 = 1;
          int num18 = 0;
          for (int index9 = 0; index9 < num16; ++index9)
          {
            byte[] numArray5 = value;
            int index10 = num18;
            int num19 = index10 + 1;
            int num20 = (int) numArray5[index10];
            byte[] numArray6 = value;
            int index11 = num19;
            int num21 = index11 + 1;
            int num22 = (int) numArray6[index11] << 8;
            int num23 = num20 | num22;
            byte[] numArray7 = value;
            int index12 = num21;
            int num24 = index12 + 1;
            int num25 = (int) numArray7[index12] << 16;
            int num26 = num23 | num25;
            byte[] numArray8 = value;
            int index13 = num24;
            num18 = index13 + 1;
            int num27 = (int) numArray8[index13] << 24;
            ulong num28 = (ulong) (uint) (num26 | num27) - (ulong) num17;
            uint num29 = (uint) num28;
            num17 = (uint) (num28 >> 32) & 1U;
            this._data[index9] = ~num29;
          }
          int num30 = num1 & 3;
          if (num30 > 0)
          {
            uint num31 = 0;
            uint num32 = 0;
            for (int index = 0; index < num30; ++index)
            {
              num31 |= (uint) value[num18++] << index * 8;
              num32 = (uint) ((int) num32 << 8 | (int) byte.MaxValue);
            }
            ulong num33 = (ulong) (num31 - num17);
            uint num34 = (uint) num33;
            num17 = (uint) (num33 >> 32) & 1U;
            if ((~(int) num34 & (int) num32) == 0)
              Array.Resize<uint>(ref this._data, this._data.Length - 1);
            else
              this._data[this._data.Length - 1] = ~num34 & num32;
          }
          if (num17 > 0U)
            throw new Exception("non zero final carry");
        }
      }
    }

    public bool IsEven => this._sign == (short) 0 || ((int) this._data[0] & 1) == 0;

    public bool IsOne => this._sign == (short) 1 && this._data.Length == 1 && this._data[0] == 1U;

    private static int PopulationCount(uint x)
    {
      x -= x >> 1 & 1431655765U;
      x = (uint) (((int) x & 858993459) + ((int) (x >> 2) & 858993459));
      x = (uint) ((int) x + (int) (x >> 4) & 252645135);
      x += x >> 8;
      x += x >> 16;
      return (int) x & 63;
    }

    private static int PopulationCount(ulong x)
    {
      x -= x >> 1 & 6148914691236517205UL;
      x = (ulong) (((long) x & 3689348814741910323L) + ((long) (x >> 2) & 3689348814741910323L));
      x = (ulong) ((long) x + (long) (x >> 4) & 1085102592571150095L);
      return (int) (x * 72340172838076673UL >> 56);
    }

    private static int LeadingZeroCount(uint value)
    {
      value |= value >> 1;
      value |= value >> 2;
      value |= value >> 4;
      value |= value >> 8;
      value |= value >> 16;
      return 32 - BigInteger.PopulationCount(value);
    }

    private static int LeadingZeroCount(ulong value)
    {
      value |= value >> 1;
      value |= value >> 2;
      value |= value >> 4;
      value |= value >> 8;
      value |= value >> 16;
      value |= value >> 32;
      return 64 - BigInteger.PopulationCount(value);
    }

    private static double BuildDouble(int sign, ulong mantissa, int exponent)
    {
      if (sign == 0 || mantissa == 0UL)
        return 0.0;
      exponent += 1075;
      int num1 = BigInteger.LeadingZeroCount(mantissa) - 11;
      if (exponent - num1 > 2046)
        return sign > 0 ? double.PositiveInfinity : double.NegativeInfinity;
      if (num1 < 0)
      {
        mantissa >>= -num1;
        exponent += -num1;
      }
      else if (num1 >= exponent)
      {
        mantissa <<= exponent - 1;
        exponent = 0;
      }
      else
      {
        mantissa <<= num1;
        exponent -= num1;
      }
      mantissa &= 4503599627370495UL;
      if (((long) exponent & 2047L) != (long) exponent)
        return sign > 0 ? double.PositiveInfinity : double.NegativeInfinity;
      ulong num2 = mantissa | (ulong) exponent << 52;
      if (sign < 0)
        num2 |= 9223372036854775808UL;
      return BitConverter.Int64BitsToDouble((long) num2);
    }

    public bool IsPowerOfTwo
    {
      get
      {
        bool isPowerOfTwo = false;
        if (this._sign != (short) 1)
          return false;
        foreach (uint x in this._data)
        {
          int num = BigInteger.PopulationCount(x);
          if (num > 0)
          {
            if (num > 1 | isPowerOfTwo)
              return false;
            isPowerOfTwo = true;
          }
        }
        return isPowerOfTwo;
      }
    }

    public bool IsZero => this._sign == (short) 0;

    public int Sign => (int) this._sign;

    public static BigInteger MinusOne => BigInteger.MinusOneSingleton;

    public static BigInteger One => BigInteger.OneSingleton;

    public static BigInteger Zero => BigInteger.ZeroSingleton;

    public static explicit operator int(BigInteger value)
    {
      if (value._data == null)
        return 0;
      if (value._data.Length > 1)
        throw new OverflowException();
      uint num = value._data[0];
      if (value._sign == (short) 1)
        return num <= (uint) int.MaxValue ? (int) num : throw new OverflowException();
      if (value._sign != (short) -1)
        return 0;
      if (num > 2147483648U)
        throw new OverflowException();
      return -(int) num;
    }

    [CLSCompliant(false)]
    public static explicit operator uint(BigInteger value)
    {
      if (value._data == null)
        return 0;
      if (value._data.Length > 1 || value._sign == (short) -1)
        throw new OverflowException();
      return value._data[0];
    }

    public static explicit operator short(BigInteger value)
    {
      int num = (int) value;
      return num >= (int) short.MinValue && num <= (int) short.MaxValue ? (short) num : throw new OverflowException();
    }

    [CLSCompliant(false)]
    public static explicit operator ushort(BigInteger value)
    {
      uint num = (uint) value;
      return num <= (uint) ushort.MaxValue ? (ushort) num : throw new OverflowException();
    }

    public static explicit operator byte(BigInteger value)
    {
      uint num = (uint) value;
      return num <= (uint) byte.MaxValue ? (byte) num : throw new OverflowException();
    }

    [CLSCompliant(false)]
    public static explicit operator sbyte(BigInteger value)
    {
      int num = (int) value;
      return num >= (int) sbyte.MinValue && num <= (int) sbyte.MaxValue ? (sbyte) num : throw new OverflowException();
    }

    public static explicit operator long(BigInteger value)
    {
      if (value._data == null)
        return 0;
      if (value._data.Length > 2)
        throw new OverflowException();
      uint num1 = value._data[0];
      if (value._data.Length == 1)
        return value._sign == (short) 1 ? (long) num1 : (long) -num1;
      uint num2 = value._data[1];
      if (value._sign == (short) 1)
      {
        if (num2 >= 2147483648U)
          throw new OverflowException();
        return (long) num2 << 32 | (long) num1;
      }
      long num3 = -((long) num2 << 32 | (long) num1);
      return num3 <= 0L ? num3 : throw new OverflowException();
    }

    [CLSCompliant(false)]
    public static explicit operator ulong(BigInteger value)
    {
      if (value._data == null)
        return 0;
      if (value._data.Length > 2 || value._sign == (short) -1)
        throw new OverflowException();
      uint num = value._data[0];
      return value._data.Length == 1 ? (ulong) num : (ulong) value._data[1] << 32 | (ulong) num;
    }

    public static explicit operator double(BigInteger value)
    {
      if (value._data == null)
        return 0.0;
      switch (value._data.Length)
      {
        case 1:
          return BigInteger.BuildDouble((int) value._sign, (ulong) value._data[0], 0);
        case 2:
          return BigInteger.BuildDouble((int) value._sign, (ulong) value._data[1] << 32 | (ulong) value._data[0], 0);
        default:
          int index = value._data.Length - 1;
          uint num1 = value._data[index];
          ulong num2 = (ulong) num1 << 32 | (ulong) value._data[index - 1];
          int num3 = BigInteger.LeadingZeroCount(num1) - 11;
          ulong mantissa = num3 <= 0 ? num2 >> -num3 : num2 << num3 | (ulong) (value._data[index - 2] >> 32 - num3);
          return BigInteger.BuildDouble((int) value._sign, mantissa, (value._data.Length - 2) * 32 - num3);
      }
    }

    public static explicit operator float(BigInteger value) => (float) (double) value;

    public static explicit operator Decimal(BigInteger value)
    {
      if (value._data == null)
        return 0M;
      uint[] data = value._data;
      if (data.Length > 3)
        throw new OverflowException();
      int lo = 0;
      int mid = 0;
      int hi = 0;
      if (data.Length > 2)
        hi = (int) data[2];
      if (data.Length > 1)
        mid = (int) data[1];
      if (data.Length != 0)
        lo = (int) data[0];
      return new Decimal(lo, mid, hi, value._sign < (short) 0, (byte) 0);
    }

    public static implicit operator BigInteger(int value) => new BigInteger(value);

    [CLSCompliant(false)]
    public static implicit operator BigInteger(uint value) => new BigInteger(value);

    public static implicit operator BigInteger(short value) => new BigInteger((int) value);

    [CLSCompliant(false)]
    public static implicit operator BigInteger(ushort value) => new BigInteger((int) value);

    public static implicit operator BigInteger(byte value) => new BigInteger((int) value);

    [CLSCompliant(false)]
    public static implicit operator BigInteger(sbyte value) => new BigInteger((int) value);

    public static implicit operator BigInteger(long value) => new BigInteger(value);

    [CLSCompliant(false)]
    public static implicit operator BigInteger(ulong value) => new BigInteger(value);

    public static explicit operator BigInteger(double value) => new BigInteger(value);

    public static explicit operator BigInteger(float value) => new BigInteger(value);

    public static explicit operator BigInteger(Decimal value) => new BigInteger(value);

    public static BigInteger operator +(BigInteger left, BigInteger right)
    {
      if (left._sign == (short) 0)
        return right;
      if (right._sign == (short) 0)
        return left;
      if ((int) left._sign == (int) right._sign)
        return new BigInteger(left._sign, BigInteger.CoreAdd(left._data, right._data));
      int num = BigInteger.CoreCompare(left._data, right._data);
      if (num == 0)
        return BigInteger.Zero;
      return num > 0 ? new BigInteger(left._sign, BigInteger.CoreSub(left._data, right._data)) : new BigInteger(right._sign, BigInteger.CoreSub(right._data, left._data));
    }

    public static BigInteger operator -(BigInteger left, BigInteger right)
    {
      if (right._sign == (short) 0)
        return left;
      if (left._sign == (short) 0)
        return new BigInteger(-right._sign, right._data);
      if ((int) left._sign != (int) right._sign)
        return new BigInteger(left._sign, BigInteger.CoreAdd(left._data, right._data));
      int num = BigInteger.CoreCompare(left._data, right._data);
      if (num == 0)
        return BigInteger.Zero;
      return num > 0 ? new BigInteger(left._sign, BigInteger.CoreSub(left._data, right._data)) : new BigInteger(-right._sign, BigInteger.CoreSub(right._data, left._data));
    }

    public static BigInteger operator *(BigInteger left, BigInteger right)
    {
      if (left._sign == (short) 0 || right._sign == (short) 0)
        return BigInteger.Zero;
      if (left._data[0] == 1U && left._data.Length == 1)
        return left._sign == (short) 1 ? right : new BigInteger(-right._sign, right._data);
      if (right._data[0] == 1U && right._data.Length == 1)
        return right._sign == (short) 1 ? left : new BigInteger(-left._sign, left._data);
      uint[] data1 = left._data;
      uint[] data2 = right._data;
      uint[] array = new uint[data1.Length + data2.Length];
      for (int index1 = 0; index1 < data1.Length; ++index1)
      {
        uint num1 = data1[index1];
        int index2 = index1;
        ulong num2 = 0;
        for (int index3 = 0; index3 < data2.Length; ++index3)
        {
          ulong num3 = num2 + (ulong) num1 * (ulong) data2[index3] + (ulong) array[index2];
          array[index2++] = (uint) num3;
          num2 = num3 >> 32;
        }
        ulong num4;
        for (; num2 > 0UL; num2 = num4 >> 32)
        {
          num4 = num2 + (ulong) array[index2];
          array[index2++] = (uint) num4;
        }
      }
      int index = array.Length - 1;
      while (index >= 0 && array[index] == 0U)
        --index;
      if (index < array.Length - 1)
        Array.Resize<uint>(ref array, index + 1);
      return new BigInteger((short) ((int) left._sign * (int) right._sign), array);
    }

    public static BigInteger operator /(BigInteger dividend, BigInteger divisor)
    {
      if (divisor._sign == (short) 0)
        throw new DivideByZeroException();
      if (dividend._sign == (short) 0)
        return dividend;
      uint[] q;
      BigInteger.DivModUnsigned(dividend._data, divisor._data, out q, out uint[] _);
      int index = q.Length - 1;
      while (index >= 0 && q[index] == 0U)
        --index;
      if (index == -1)
        return BigInteger.Zero;
      if (index < q.Length - 1)
        Array.Resize<uint>(ref q, index + 1);
      return new BigInteger((short) ((int) dividend._sign * (int) divisor._sign), q);
    }

    public static BigInteger operator %(BigInteger dividend, BigInteger divisor)
    {
      if (divisor._sign == (short) 0)
        throw new DivideByZeroException();
      if (dividend._sign == (short) 0)
        return dividend;
      uint[] r;
      BigInteger.DivModUnsigned(dividend._data, divisor._data, out uint[] _, out r);
      int index = r.Length - 1;
      while (index >= 0 && r[index] == 0U)
        --index;
      if (index == -1)
        return BigInteger.Zero;
      if (index < r.Length - 1)
        Array.Resize<uint>(ref r, index + 1);
      return new BigInteger(dividend._sign, r);
    }

    public static BigInteger operator -(BigInteger value) => value._data == null ? value : new BigInteger(-value._sign, value._data);

    public static BigInteger operator +(BigInteger value) => value;

    public static BigInteger operator ++(BigInteger value)
    {
      if (value._data == null)
        return BigInteger.One;
      short sign = value._sign;
      uint[] data1 = value._data;
      if (data1.Length == 1)
      {
        if (sign == (short) -1 && data1[0] == 1U)
          return BigInteger.Zero;
        if (sign == (short) 0)
          return BigInteger.One;
      }
      uint[] data2 = sign == (short) -1 ? BigInteger.CoreSub(data1, 1U) : BigInteger.CoreAdd(data1, 1U);
      return new BigInteger(sign, data2);
    }

    public static BigInteger operator --(BigInteger value)
    {
      if (value._data == null)
        return BigInteger.MinusOne;
      short sign = value._sign;
      uint[] data1 = value._data;
      if (data1.Length == 1)
      {
        if (sign == (short) 1 && data1[0] == 1U)
          return BigInteger.Zero;
        if (sign == (short) 0)
          return BigInteger.MinusOne;
      }
      uint[] data2 = sign == (short) -1 ? BigInteger.CoreAdd(data1, 1U) : BigInteger.CoreSub(data1, 1U);
      return new BigInteger(sign, data2);
    }

    public static BigInteger operator &(BigInteger left, BigInteger right)
    {
      if (left._sign == (short) 0)
        return left;
      if (right._sign == (short) 0)
        return right;
      uint[] data1 = left._data;
      uint[] data2 = right._data;
      int sign1 = (int) left._sign;
      int sign2 = (int) right._sign;
      bool flag = sign1 == sign2 && sign1 == -1;
      uint[] array = new uint[Math.Max(data1.Length, data2.Length)];
      ulong num1 = 1;
      ulong num2 = 1;
      ulong num3 = 1;
      for (int index = 0; index < array.Length; ++index)
      {
        uint num4 = 0;
        if (index < data1.Length)
          num4 = data1[index];
        if (sign1 == -1)
        {
          ulong num5 = (ulong) ~num4 + num1;
          num4 = (uint) num5;
          num1 = (ulong) (uint) (num5 >> 32);
        }
        uint num6 = 0;
        if (index < data2.Length)
          num6 = data2[index];
        if (sign2 == -1)
        {
          ulong num7 = (ulong) ~num6 + num2;
          num6 = (uint) num7;
          num2 = (ulong) (uint) (num7 >> 32);
        }
        uint num8 = num4 & num6;
        if (flag)
        {
          ulong num9 = (ulong) num8 - num3;
          num8 = ~(uint) num9;
          num3 = (ulong) ((uint) (num9 >> 32) & 1U);
        }
        array[index] = num8;
      }
      int index1 = array.Length - 1;
      while (index1 >= 0 && array[index1] == 0U)
        --index1;
      if (index1 == -1)
        return BigInteger.Zero;
      if (index1 < array.Length - 1)
        Array.Resize<uint>(ref array, index1 + 1);
      return new BigInteger(flag ? (short) -1 : (short) 1, array);
    }

    public static BigInteger operator |(BigInteger left, BigInteger right)
    {
      if (left._sign == (short) 0)
        return right;
      if (right._sign == (short) 0)
        return left;
      uint[] data1 = left._data;
      uint[] data2 = right._data;
      int sign1 = (int) left._sign;
      int sign2 = (int) right._sign;
      bool flag = sign1 == -1 || sign2 == -1;
      uint[] array = new uint[Math.Max(data1.Length, data2.Length)];
      ulong num1 = 1;
      ulong num2 = 1;
      ulong num3 = 1;
      for (int index = 0; index < array.Length; ++index)
      {
        uint num4 = 0;
        if (index < data1.Length)
          num4 = data1[index];
        if (sign1 == -1)
        {
          ulong num5 = (ulong) ~num4 + num1;
          num4 = (uint) num5;
          num1 = (ulong) (uint) (num5 >> 32);
        }
        uint num6 = 0;
        if (index < data2.Length)
          num6 = data2[index];
        if (sign2 == -1)
        {
          ulong num7 = (ulong) ~num6 + num2;
          num6 = (uint) num7;
          num2 = (ulong) (uint) (num7 >> 32);
        }
        uint num8 = num4 | num6;
        if (flag)
        {
          ulong num9 = (ulong) num8 - num3;
          num8 = ~(uint) num9;
          num3 = (ulong) ((uint) (num9 >> 32) & 1U);
        }
        array[index] = num8;
      }
      int index1 = array.Length - 1;
      while (index1 >= 0 && array[index1] == 0U)
        --index1;
      if (index1 == -1)
        return BigInteger.Zero;
      if (index1 < array.Length - 1)
        Array.Resize<uint>(ref array, index1 + 1);
      return new BigInteger(flag ? (short) -1 : (short) 1, array);
    }

    public static BigInteger operator ^(BigInteger left, BigInteger right)
    {
      if (left._sign == (short) 0)
        return right;
      if (right._sign == (short) 0)
        return left;
      uint[] data1 = left._data;
      uint[] data2 = right._data;
      int sign1 = (int) left._sign;
      int sign2 = (int) right._sign;
      bool flag = sign1 == -1 ^ sign2 == -1;
      uint[] array = new uint[Math.Max(data1.Length, data2.Length)];
      ulong num1 = 1;
      ulong num2 = 1;
      ulong num3 = 1;
      for (int index = 0; index < array.Length; ++index)
      {
        uint num4 = 0;
        if (index < data1.Length)
          num4 = data1[index];
        if (sign1 == -1)
        {
          ulong num5 = (ulong) ~num4 + num1;
          num4 = (uint) num5;
          num1 = (ulong) (uint) (num5 >> 32);
        }
        uint num6 = 0;
        if (index < data2.Length)
          num6 = data2[index];
        if (sign2 == -1)
        {
          ulong num7 = (ulong) ~num6 + num2;
          num6 = (uint) num7;
          num2 = (ulong) (uint) (num7 >> 32);
        }
        uint num8 = num4 ^ num6;
        if (flag)
        {
          ulong num9 = (ulong) num8 - num3;
          num8 = ~(uint) num9;
          num3 = (ulong) ((uint) (num9 >> 32) & 1U);
        }
        array[index] = num8;
      }
      int index1 = array.Length - 1;
      while (index1 >= 0 && array[index1] == 0U)
        --index1;
      if (index1 == -1)
        return BigInteger.Zero;
      if (index1 < array.Length - 1)
        Array.Resize<uint>(ref array, index1 + 1);
      return new BigInteger(flag ? (short) -1 : (short) 1, array);
    }

    public static BigInteger operator ~(BigInteger value)
    {
      if (value._data == null)
        return BigInteger.MinusOne;
      uint[] data = value._data;
      int sign = (int) value._sign;
      bool flag = sign == 1;
      uint[] array = new uint[data.Length];
      ulong num1 = 1;
      ulong num2 = 1;
      for (int index = 0; index < array.Length; ++index)
      {
        uint num3 = data[index];
        if (sign == -1)
        {
          ulong num4 = (ulong) ~num3 + num1;
          num3 = (uint) num4;
          num1 = (ulong) (uint) (num4 >> 32);
        }
        uint num5 = ~num3;
        if (flag)
        {
          ulong num6 = (ulong) num5 - num2;
          num5 = ~(uint) num6;
          num2 = (ulong) ((uint) (num6 >> 32) & 1U);
        }
        array[index] = num5;
      }
      int index1 = array.Length - 1;
      while (index1 >= 0 && array[index1] == 0U)
        --index1;
      if (index1 == -1)
        return BigInteger.Zero;
      if (index1 < array.Length - 1)
        Array.Resize<uint>(ref array, index1 + 1);
      return new BigInteger(flag ? (short) -1 : (short) 1, array);
    }

    private static int BitScanBackward(uint word)
    {
      for (int index = 31; index >= 0; --index)
      {
        uint num = (uint) (1 << index);
        if (((int) word & (int) num) == (int) num)
          return index;
      }
      return 0;
    }

    public static BigInteger operator <<(BigInteger value, int shift)
    {
      if (shift == 0 || value._data == null)
        return value;
      if (shift < 0)
        return value >> -shift;
      uint[] data1 = value._data;
      int sign = (int) value._sign;
      int num1 = BigInteger.BitScanBackward(data1[data1.Length - 1]);
      int num2 = shift - (31 - num1);
      int num3 = (num2 >> 5) + ((num2 & 31) != 0 ? 1 : 0);
      uint[] data2 = new uint[data1.Length + num3];
      int num4 = shift >> 5;
      int num5 = shift & 31;
      int num6 = 32 - num5;
      if (num6 == 32)
      {
        for (int index = 0; index < data1.Length; ++index)
        {
          uint num7 = data1[index];
          data2[index + num4] |= num7 << num5;
        }
      }
      else
      {
        for (int index = 0; index < data1.Length; ++index)
        {
          uint num8 = data1[index];
          data2[index + num4] |= num8 << num5;
          if (index + num4 + 1 < data2.Length)
            data2[index + num4 + 1] = num8 >> num6;
        }
      }
      return new BigInteger((short) sign, data2);
    }

    public static BigInteger operator >>(BigInteger value, int shift)
    {
      if (shift == 0 || value._sign == (short) 0)
        return value;
      if (shift < 0)
        return value << -shift;
      uint[] data1 = value._data;
      int sign = (int) value._sign;
      int num1 = BigInteger.BitScanBackward(data1[data1.Length - 1]);
      int index1 = shift >> 5;
      int num2 = shift & 31;
      int num3 = index1;
      if (num2 > num1)
        ++num3;
      int length = data1.Length - num3;
      if (length <= 0)
        return sign == 1 ? BigInteger.Zero : BigInteger.MinusOne;
      uint[] data2 = new uint[length];
      int num4 = 32 - num2;
      if (num4 == 32)
      {
        for (int index2 = data1.Length - 1; index2 >= index1; --index2)
        {
          uint num5 = data1[index2];
          if (index2 - index1 < data2.Length)
            data2[index2 - index1] |= num5 >> num2;
        }
      }
      else
      {
        for (int index3 = data1.Length - 1; index3 >= index1; --index3)
        {
          uint num6 = data1[index3];
          if (index3 - index1 < data2.Length)
            data2[index3 - index1] |= num6 >> num2;
          if (index3 - index1 - 1 >= 0)
            data2[index3 - index1 - 1] = num6 << num4;
        }
      }
      if (sign == -1)
      {
        for (int index4 = 0; index4 < index1; ++index4)
        {
          if (data1[index4] > 0U)
            return BigInteger.op_Decrement(new BigInteger((short) sign, data2));
        }
        if (num2 > 0 && data1[index1] << num4 > 0U)
          return BigInteger.op_Decrement(new BigInteger((short) sign, data2));
      }
      return new BigInteger((short) sign, data2);
    }

    public static bool operator <(BigInteger left, BigInteger right) => BigInteger.Compare(left, right) < 0;

    public static bool operator <(BigInteger left, long right) => left.CompareTo(right) < 0;

    public static bool operator <(long left, BigInteger right) => right.CompareTo(left) > 0;

    [CLSCompliant(false)]
    public static bool operator <(BigInteger left, ulong right) => left.CompareTo(right) < 0;

    [CLSCompliant(false)]
    public static bool operator <(ulong left, BigInteger right) => right.CompareTo(left) > 0;

    public static bool operator <=(BigInteger left, BigInteger right) => BigInteger.Compare(left, right) <= 0;

    public static bool operator <=(BigInteger left, long right) => left.CompareTo(right) <= 0;

    public static bool operator <=(long left, BigInteger right) => right.CompareTo(left) >= 0;

    [CLSCompliant(false)]
    public static bool operator <=(BigInteger left, ulong right) => left.CompareTo(right) <= 0;

    [CLSCompliant(false)]
    public static bool operator <=(ulong left, BigInteger right) => right.CompareTo(left) >= 0;

    public static bool operator >(BigInteger left, BigInteger right) => BigInteger.Compare(left, right) > 0;

    public static bool operator >(BigInteger left, long right) => left.CompareTo(right) > 0;

    public static bool operator >(long left, BigInteger right) => right.CompareTo(left) < 0;

    [CLSCompliant(false)]
    public static bool operator >(BigInteger left, ulong right) => left.CompareTo(right) > 0;

    [CLSCompliant(false)]
    public static bool operator >(ulong left, BigInteger right) => right.CompareTo(left) < 0;

    public static bool operator >=(BigInteger left, BigInteger right) => BigInteger.Compare(left, right) >= 0;

    public static bool operator >=(BigInteger left, long right) => left.CompareTo(right) >= 0;

    public static bool operator >=(long left, BigInteger right) => right.CompareTo(left) <= 0;

    [CLSCompliant(false)]
    public static bool operator >=(BigInteger left, ulong right) => left.CompareTo(right) >= 0;

    [CLSCompliant(false)]
    public static bool operator >=(ulong left, BigInteger right) => right.CompareTo(left) <= 0;

    public static bool operator ==(BigInteger left, BigInteger right) => BigInteger.Compare(left, right) == 0;

    public static bool operator ==(BigInteger left, long right) => left.CompareTo(right) == 0;

    public static bool operator ==(long left, BigInteger right) => right.CompareTo(left) == 0;

    [CLSCompliant(false)]
    public static bool operator ==(BigInteger left, ulong right) => left.CompareTo(right) == 0;

    [CLSCompliant(false)]
    public static bool operator ==(ulong left, BigInteger right) => right.CompareTo(left) == 0;

    public static bool operator !=(BigInteger left, BigInteger right) => BigInteger.Compare(left, right) != 0;

    public static bool operator !=(BigInteger left, long right) => left.CompareTo(right) != 0;

    public static bool operator !=(long left, BigInteger right) => right.CompareTo(left) != 0;

    [CLSCompliant(false)]
    public static bool operator !=(BigInteger left, ulong right) => left.CompareTo(right) != 0;

    [CLSCompliant(false)]
    public static bool operator !=(ulong left, BigInteger right) => right.CompareTo(left) != 0;

    public override bool Equals(object obj) => obj is BigInteger other && this.Equals(other);

    public bool Equals(BigInteger other)
    {
      if ((int) this._sign != (int) other._sign)
        return false;
      int num1 = this._data != null ? this._data.Length : 0;
      int num2 = other._data != null ? other._data.Length : 0;
      if (num1 != num2)
        return false;
      for (int index = 0; index < num1; ++index)
      {
        if ((int) this._data[index] != (int) other._data[index])
          return false;
      }
      return true;
    }

    public bool Equals(long other) => this.CompareTo(other) == 0;

    public override string ToString() => this.ToString(10U, (IFormatProvider) null);

    private string ToStringWithPadding(string format, uint radix, IFormatProvider provider)
    {
      if (format.Length <= 1)
        return this.ToString(radix, provider);
      int int32 = Convert.ToInt32(format.Substring(1), (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
      string stringWithPadding = this.ToString(radix, provider);
      if (stringWithPadding.Length >= int32)
        return stringWithPadding;
      string str = new string('0', int32 - stringWithPadding.Length);
      return stringWithPadding[0] != '-' ? str + stringWithPadding : "-" + str + stringWithPadding.Substring(1);
    }

    public string ToString(string format) => this.ToString(format, (IFormatProvider) null);

    public string ToString(IFormatProvider provider) => this.ToString((string) null, provider);

    public string ToString(string format, IFormatProvider provider)
    {
      if (string.IsNullOrEmpty(format))
        return this.ToString(10U, provider);
      switch (format[0])
      {
        case 'D':
        case 'G':
        case 'R':
        case 'd':
        case 'g':
        case 'r':
          return this.ToStringWithPadding(format, 10U, provider);
        case 'X':
        case 'x':
          return this.ToStringWithPadding(format, 16U, (IFormatProvider) null);
        default:
          throw new FormatException(string.Format("format '{0}' not implemented", (object) format));
      }
    }

    private static uint[] MakeTwoComplement(uint[] v)
    {
      uint[] numArray = new uint[v.Length];
      ulong num1 = 1;
      for (int index = 0; index < v.Length; ++index)
      {
        ulong num2 = (ulong) ~v[index] + num1;
        uint num3 = (uint) num2;
        num1 = (ulong) (uint) (num2 >> 32);
        numArray[index] = num3;
      }
      uint word = numArray[numArray.Length - 1];
      int num4 = BigInteger.FirstNonFfByte(word);
      uint num5 = (uint) byte.MaxValue;
      for (int index = 1; index < num4; ++index)
        num5 = (uint) ((int) num5 << 8 | (int) byte.MaxValue);
      numArray[numArray.Length - 1] = word & num5;
      return numArray;
    }

    private string ToString(uint radix, IFormatProvider provider)
    {
      if ((long) "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length < (long) radix)
        throw new ArgumentException("charSet length less than radix", "characterSet");
      if (radix == 1U)
        throw new ArgumentException("There is no such thing as radix one notation", nameof (radix));
      if (this._sign == (short) 0)
        return "0";
      if (this._data.Length == 1 && this._data[0] == 1U)
        return this._sign == (short) 1 ? "1" : "-1";
      List<char> charList = new List<char>(1 + this._data.Length * 3 / 10);
      BigInteger dividend;
      if (this._sign == (short) 1)
      {
        dividend = this;
      }
      else
      {
        uint[] numArray = this._data;
        if (radix > 10U)
          numArray = BigInteger.MakeTwoComplement(numArray);
        dividend = new BigInteger((short) 1, numArray);
      }
      while (dividend != 0L)
      {
        BigInteger remainder;
        dividend = BigInteger.DivRem(dividend, (BigInteger) radix, out remainder);
        charList.Add("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[(int) remainder]);
      }
      if (this._sign == (short) -1 && radix == 10U)
      {
        NumberFormatInfo numberFormatInfo = (NumberFormatInfo) null;
        if (provider != null)
          numberFormatInfo = provider.GetFormat(typeof (NumberFormatInfo)) as NumberFormatInfo;
        if (numberFormatInfo != null)
        {
          string negativeSign = numberFormatInfo.NegativeSign;
          for (int index = negativeSign.Length - 1; index >= 0; --index)
            charList.Add(negativeSign[index]);
        }
        else
          charList.Add('-');
      }
      char ch = charList[charList.Count - 1];
      if (this._sign == (short) 1 && radix > 10U && (ch < '0' || ch > '9'))
        charList.Add('0');
      charList.Reverse();
      return new string(charList.ToArray());
    }

    public static BigInteger Parse(string value)
    {
      BigInteger result;
      Exception exc;
      if (!BigInteger.Parse(value, false, out result, out exc))
        throw exc;
      return result;
    }

    public static BigInteger Parse(string value, NumberStyles style) => BigInteger.Parse(value, style, (IFormatProvider) null);

    public static BigInteger Parse(string value, IFormatProvider provider) => BigInteger.Parse(value, NumberStyles.Integer, provider);

    public static BigInteger Parse(
      string value,
      NumberStyles style,
      IFormatProvider provider)
    {
      BigInteger result;
      Exception exc;
      if (!BigInteger.Parse(value, style, provider, false, out result, out exc))
        throw exc;
      return result;
    }

    public static bool TryParse(string value, out BigInteger result) => BigInteger.Parse(value, true, out result, out Exception _);

    public static bool TryParse(
      string value,
      NumberStyles style,
      IFormatProvider provider,
      out BigInteger result)
    {
      if (BigInteger.Parse(value, style, provider, true, out result, out Exception _))
        return true;
      result = BigInteger.Zero;
      return false;
    }

    private static bool Parse(
      string value,
      NumberStyles style,
      IFormatProvider fp,
      bool tryParse,
      out BigInteger result,
      out Exception exc)
    {
      result = BigInteger.Zero;
      exc = (Exception) null;
      switch (value)
      {
        case "":
          if (!tryParse)
            exc = BigInteger.GetFormatException();
          return false;
        case null:
          if (!tryParse)
            exc = (Exception) new ArgumentNullException(nameof (value));
          return false;
        default:
          NumberFormatInfo nfi = (NumberFormatInfo) null;
          if (fp != null)
          {
            Type formatType = typeof (NumberFormatInfo);
            nfi = (NumberFormatInfo) fp.GetFormat(formatType);
          }
          if (nfi == null)
            nfi = NumberFormatInfo.CurrentInfo;
          if (!BigInteger.CheckStyle(style, tryParse, ref exc))
            return false;
          bool flag1 = (style & NumberStyles.AllowCurrencySymbol) != 0;
          bool allowHex = (style & NumberStyles.AllowHexSpecifier) != 0;
          bool flag2 = (style & NumberStyles.AllowThousands) != 0;
          bool flag3 = (style & NumberStyles.AllowDecimalPoint) != 0;
          bool flag4 = (style & NumberStyles.AllowParentheses) != 0;
          bool flag5 = (style & NumberStyles.AllowTrailingSign) != 0;
          bool flag6 = (style & NumberStyles.AllowLeadingSign) != 0;
          bool flag7 = (style & NumberStyles.AllowTrailingWhite) != 0;
          bool flag8 = (style & NumberStyles.AllowLeadingWhite) != 0;
          bool flag9 = (style & NumberStyles.AllowExponent) != 0;
          int pos = 0;
          if (flag8 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
            return false;
          bool flag10 = false;
          bool negative = false;
          bool foundSign = false;
          bool foundCurrency = false;
          if (flag4 && value[pos] == '(')
          {
            flag10 = true;
            foundSign = true;
            negative = true;
            ++pos;
            if (flag8 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
              return false;
            if (value.Substring(pos, nfi.NegativeSign.Length) == nfi.NegativeSign)
            {
              if (!tryParse)
                exc = BigInteger.GetFormatException();
              return false;
            }
            if (value.Substring(pos, nfi.PositiveSign.Length) == nfi.PositiveSign)
            {
              if (!tryParse)
                exc = BigInteger.GetFormatException();
              return false;
            }
          }
          if (flag6 && !foundSign)
          {
            BigInteger.FindSign(ref pos, value, nfi, ref foundSign, ref negative);
            if (foundSign)
            {
              if (flag8 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
                return false;
              if (flag1)
              {
                BigInteger.FindCurrency(ref pos, value, nfi, ref foundCurrency);
                if (foundCurrency & flag8 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
                  return false;
              }
            }
          }
          if (flag1 && !foundCurrency)
          {
            BigInteger.FindCurrency(ref pos, value, nfi, ref foundCurrency);
            if (foundCurrency)
            {
              if (flag8 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
                return false;
              if (foundCurrency && !foundSign & flag6)
              {
                BigInteger.FindSign(ref pos, value, nfi, ref foundSign, ref negative);
                if (foundSign & flag8 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
                  return false;
              }
            }
          }
          BigInteger dividend = BigInteger.Zero;
          int exponent1 = 0;
          int num1 = -1;
          bool flag11 = true;
          while (pos < value.Length)
          {
            if (!BigInteger.ValidDigit(value[pos], allowHex))
            {
              if (!flag2 || !BigInteger.FindOther(ref pos, value, nfi.NumberGroupSeparator) && !BigInteger.FindOther(ref pos, value, nfi.CurrencyGroupSeparator))
              {
                if (flag3 && num1 < 0 && (BigInteger.FindOther(ref pos, value, nfi.NumberDecimalSeparator) || BigInteger.FindOther(ref pos, value, nfi.CurrencyDecimalSeparator)))
                  num1 = exponent1;
                else
                  break;
              }
            }
            else
            {
              ++exponent1;
              if (allowHex)
              {
                char c = value[pos++];
                byte num2 = !char.IsDigit(c) ? (!char.IsLower(c) ? (byte) ((int) c - 65 + 10) : (byte) ((int) c - 97 + 10)) : (byte) ((uint) c - 48U);
                if (flag11 && num2 >= (byte) 8)
                  negative = true;
                dividend = dividend * (BigInteger) 16 + (BigInteger) num2;
                flag11 = false;
              }
              else
                dividend = dividend * (BigInteger) 10 + (BigInteger) (byte) ((uint) value[pos++] - 48U);
            }
          }
          if (exponent1 == 0)
          {
            if (!tryParse)
              exc = BigInteger.GetFormatException();
            return false;
          }
          if (allowHex & negative)
          {
            BigInteger bigInteger = BigInteger.Pow((BigInteger) 16, exponent1) - (BigInteger) 1;
            dividend = (dividend ^ bigInteger) + (BigInteger) 1;
          }
          int exponent2 = 0;
          if (flag9 && BigInteger.FindExponent(ref pos, value, ref exponent2, tryParse, ref exc) && exc != null)
            return false;
          if (flag5 && !foundSign)
          {
            BigInteger.FindSign(ref pos, value, nfi, ref foundSign, ref negative);
            if (foundSign && pos < value.Length && flag7 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
              return false;
          }
          if (flag1 && !foundCurrency)
          {
            if (flag7 && pos < value.Length && !BigInteger.JumpOverWhitespace(ref pos, value, false, tryParse, ref exc))
              return false;
            BigInteger.FindCurrency(ref pos, value, nfi, ref foundCurrency);
            if (foundCurrency && pos < value.Length)
            {
              if (flag7 && !BigInteger.JumpOverWhitespace(ref pos, value, true, tryParse, ref exc))
                return false;
              if (!foundSign & flag5)
                BigInteger.FindSign(ref pos, value, nfi, ref foundSign, ref negative);
            }
          }
          if (flag7 && pos < value.Length && !BigInteger.JumpOverWhitespace(ref pos, value, false, tryParse, ref exc))
            return false;
          if (flag10)
          {
            if (pos >= value.Length || value[pos++] != ')')
            {
              if (!tryParse)
                exc = BigInteger.GetFormatException();
              return false;
            }
            if (flag7 && pos < value.Length && !BigInteger.JumpOverWhitespace(ref pos, value, false, tryParse, ref exc))
              return false;
          }
          if (pos < value.Length && value[pos] > char.MinValue)
          {
            if (!tryParse)
              exc = BigInteger.GetFormatException();
            return false;
          }
          if (num1 >= 0)
            exponent2 = exponent2 - exponent1 + num1;
          if (exponent2 < 0)
          {
            BigInteger remainder;
            dividend = BigInteger.DivRem(dividend, BigInteger.Pow((BigInteger) 10, -exponent2), out remainder);
            if (!remainder.IsZero)
            {
              if (!tryParse)
                exc = (Exception) new OverflowException("Value too large or too small. exp=" + exponent2.ToString() + " rem = " + remainder.ToString() + " pow = " + BigInteger.Pow((BigInteger) 10, -exponent2).ToString());
              return false;
            }
          }
          else if (exponent2 > 0)
            dividend = BigInteger.Pow((BigInteger) 10, exponent2) * dividend;
          result = dividend._sign != (short) 0 ? (!negative ? new BigInteger((short) 1, dividend._data) : new BigInteger((short) -1, dividend._data)) : dividend;
          return true;
      }
    }

    private static bool CheckStyle(NumberStyles style, bool tryParse, ref Exception exc)
    {
      if ((style & NumberStyles.AllowHexSpecifier) != 0)
      {
        NumberStyles numberStyles = style ^ NumberStyles.AllowHexSpecifier;
        if ((numberStyles & NumberStyles.AllowLeadingWhite) != 0)
          numberStyles ^= NumberStyles.AllowLeadingWhite;
        if ((numberStyles & NumberStyles.AllowTrailingWhite) != 0)
          numberStyles ^= NumberStyles.AllowTrailingWhite;
        if (numberStyles != 0)
        {
          if (!tryParse)
            exc = (Exception) new ArgumentException("With AllowHexSpecifier only AllowLeadingWhite and AllowTrailingWhite are permitted.");
          return false;
        }
      }
      else if ((uint) style > 511U)
      {
        if (!tryParse)
          exc = (Exception) new ArgumentException("Not a valid number style");
        return false;
      }
      return true;
    }

    private static bool JumpOverWhitespace(
      ref int pos,
      string s,
      bool reportError,
      bool tryParse,
      ref Exception exc)
    {
      while (pos < s.Length && char.IsWhiteSpace(s[pos]))
        ++pos;
      if (!reportError || pos < s.Length)
        return true;
      if (!tryParse)
        exc = BigInteger.GetFormatException();
      return false;
    }

    private static void FindSign(
      ref int pos,
      string s,
      NumberFormatInfo nfi,
      ref bool foundSign,
      ref bool negative)
    {
      if (pos + nfi.NegativeSign.Length <= s.Length && string.CompareOrdinal(s, pos, nfi.NegativeSign, 0, nfi.NegativeSign.Length) == 0)
      {
        negative = true;
        foundSign = true;
        pos += nfi.NegativeSign.Length;
      }
      else
      {
        if (pos + nfi.PositiveSign.Length > s.Length || string.CompareOrdinal(s, pos, nfi.PositiveSign, 0, nfi.PositiveSign.Length) != 0)
          return;
        negative = false;
        pos += nfi.PositiveSign.Length;
        foundSign = true;
      }
    }

    private static void FindCurrency(
      ref int pos,
      string s,
      NumberFormatInfo nfi,
      ref bool foundCurrency)
    {
      if (pos + nfi.CurrencySymbol.Length > s.Length || !(s.Substring(pos, nfi.CurrencySymbol.Length) == nfi.CurrencySymbol))
        return;
      foundCurrency = true;
      pos += nfi.CurrencySymbol.Length;
    }

    private static bool FindExponent(
      ref int pos,
      string s,
      ref int exponent,
      bool tryParse,
      ref Exception exc)
    {
      exponent = 0;
      if (pos >= s.Length || s[pos] != 'e' && s[pos] != 'E')
      {
        exc = (Exception) null;
        return false;
      }
      int index = pos + 1;
      if (index == s.Length)
      {
        exc = tryParse ? (Exception) null : BigInteger.GetFormatException();
        return true;
      }
      bool flag = false;
      if (s[index] == '-')
      {
        flag = true;
        if (++index == s.Length)
        {
          exc = tryParse ? (Exception) null : BigInteger.GetFormatException();
          return true;
        }
      }
      if (s[index] == '+' && ++index == s.Length)
      {
        exc = tryParse ? (Exception) null : BigInteger.GetFormatException();
        return true;
      }
      long num = 0;
      for (; index < s.Length; ++index)
      {
        if (!char.IsDigit(s[index]))
        {
          exc = tryParse ? (Exception) null : BigInteger.GetFormatException();
          return true;
        }
        num = checked (num * 10L - (long) ((int) s[index] - 48));
        if (num < (long) int.MinValue || num > (long) int.MaxValue)
        {
          exc = tryParse ? (Exception) null : (Exception) new OverflowException("Value too large or too small.");
          return true;
        }
      }
      if (!flag)
        num = -num;
      exc = (Exception) null;
      exponent = (int) num;
      pos = index;
      return true;
    }

    private static bool FindOther(ref int pos, string s, string other)
    {
      if (pos + other.Length > s.Length || !(s.Substring(pos, other.Length) == other))
        return false;
      pos += other.Length;
      return true;
    }

    private static bool ValidDigit(char e, bool allowHex) => allowHex ? char.IsDigit(e) || e >= 'A' && e <= 'F' || e >= 'a' && e <= 'f' : char.IsDigit(e);

    private static Exception GetFormatException() => (Exception) new FormatException("Input string was not in the correct format");

    private static bool ProcessTrailingWhitespace(
      bool tryParse,
      string s,
      int position,
      ref Exception exc)
    {
      int length = s.Length;
      for (int index = position; index < length; ++index)
      {
        char c = s[index];
        if (c != char.MinValue && !char.IsWhiteSpace(c))
        {
          if (!tryParse)
            exc = BigInteger.GetFormatException();
          return false;
        }
      }
      return true;
    }

    private static bool Parse(
      string value,
      bool tryParse,
      out BigInteger result,
      out Exception exc)
    {
      int num1 = 1;
      bool flag = false;
      result = BigInteger.Zero;
      exc = (Exception) null;
      if (value == null)
      {
        if (!tryParse)
          exc = (Exception) new ArgumentNullException(nameof (value));
        return false;
      }
      int length = value.Length;
      int num2 = 0;
      while (num2 < length && char.IsWhiteSpace(value[num2]))
        ++num2;
      if (num2 == length)
      {
        if (!tryParse)
          exc = BigInteger.GetFormatException();
        return false;
      }
      NumberFormatInfo currentInfo = NumberFormatInfo.CurrentInfo;
      string negativeSign = currentInfo.NegativeSign;
      string positiveSign = currentInfo.PositiveSign;
      if (string.CompareOrdinal(value, num2, positiveSign, 0, positiveSign.Length) == 0)
        num2 += positiveSign.Length;
      else if (string.CompareOrdinal(value, num2, negativeSign, 0, negativeSign.Length) == 0)
      {
        num1 = -1;
        num2 += negativeSign.Length;
      }
      BigInteger bigInteger = BigInteger.Zero;
      for (; num2 < length; ++num2)
      {
        char ch = value[num2];
        if (ch == char.MinValue)
          num2 = length;
        else if (ch >= '0' && ch <= '9')
        {
          byte num3 = (byte) ((uint) ch - 48U);
          bigInteger = bigInteger * (BigInteger) 10 + (BigInteger) num3;
          flag = true;
        }
        else if (!BigInteger.ProcessTrailingWhitespace(tryParse, value, num2, ref exc))
          return false;
      }
      if (!flag)
      {
        if (!tryParse)
          exc = BigInteger.GetFormatException();
        return false;
      }
      result = bigInteger._sign != (short) 0 ? (num1 != -1 ? new BigInteger((short) 1, bigInteger._data) : new BigInteger((short) -1, bigInteger._data)) : bigInteger;
      return true;
    }

    public static BigInteger Min(BigInteger left, BigInteger right)
    {
      int sign1 = (int) left._sign;
      int sign2 = (int) right._sign;
      if (sign1 < sign2)
        return left;
      if (sign2 < sign1)
        return right;
      int num = BigInteger.CoreCompare(left._data, right._data);
      if (sign1 == -1)
        num = -num;
      return num <= 0 ? left : right;
    }

    public static BigInteger Max(BigInteger left, BigInteger right)
    {
      int sign1 = (int) left._sign;
      int sign2 = (int) right._sign;
      if (sign1 > sign2)
        return left;
      if (sign2 > sign1)
        return right;
      int num = BigInteger.CoreCompare(left._data, right._data);
      if (sign1 == -1)
        num = -num;
      return num >= 0 ? left : right;
    }

    public static BigInteger Abs(BigInteger value) => new BigInteger(Math.Abs(value._sign), value._data);

    public static BigInteger DivRem(
      BigInteger dividend,
      BigInteger divisor,
      out BigInteger remainder)
    {
      if (divisor._sign == (short) 0)
        throw new DivideByZeroException();
      if (dividend._sign == (short) 0)
      {
        remainder = dividend;
        return dividend;
      }
      uint[] q;
      uint[] r;
      BigInteger.DivModUnsigned(dividend._data, divisor._data, out q, out r);
      int index1 = r.Length - 1;
      while (index1 >= 0 && r[index1] == 0U)
        --index1;
      if (index1 == -1)
      {
        remainder = BigInteger.Zero;
      }
      else
      {
        if (index1 < r.Length - 1)
          Array.Resize<uint>(ref r, index1 + 1);
        remainder = new BigInteger(dividend._sign, r);
      }
      int index2 = q.Length - 1;
      while (index2 >= 0 && q[index2] == 0U)
        --index2;
      if (index2 == -1)
        return BigInteger.Zero;
      if (index2 < q.Length - 1)
        Array.Resize<uint>(ref q, index2 + 1);
      return new BigInteger((short) ((int) dividend._sign * (int) divisor._sign), q);
    }

    public static BigInteger Pow(BigInteger value, int exponent)
    {
      if (exponent < 0)
        throw new ArgumentOutOfRangeException(nameof (exponent), "exp must be >= 0");
      if (exponent == 0)
        return BigInteger.One;
      if (exponent == 1)
        return value;
      BigInteger one = BigInteger.One;
      for (; exponent != 0; exponent >>= 1)
      {
        if ((exponent & 1) != 0)
          one *= value;
        if (exponent != 1)
          value *= value;
        else
          break;
      }
      return one;
    }

    public static BigInteger ModPow(
      BigInteger value,
      BigInteger exponent,
      BigInteger modulus)
    {
      if (exponent._sign == (short) -1)
        throw new ArgumentOutOfRangeException(nameof (exponent), "power must be >= 0");
      if (modulus._sign == (short) 0)
        throw new DivideByZeroException();
      BigInteger bigInteger = BigInteger.One % modulus;
      while (exponent._sign != (short) 0)
      {
        if (!exponent.IsEven)
          bigInteger = bigInteger * value % modulus;
        if (!exponent.IsOne)
        {
          value *= value;
          value %= modulus;
          exponent >>= 1;
        }
        else
          break;
      }
      return bigInteger;
    }

    public static BigInteger GreatestCommonDivisor(BigInteger left, BigInteger right)
    {
      if (left._sign != (short) 0 && left._data.Length == 1 && left._data[0] == 1U || right._sign != (short) 0 && right._data.Length == 1 && right._data[0] == 1U)
        return BigInteger.One;
      if (left.IsZero)
        return BigInteger.Abs(right);
      if (right.IsZero)
        return BigInteger.Abs(left);
      BigInteger bigInteger1 = new BigInteger((short) 1, left._data);
      BigInteger bigInteger2 = new BigInteger((short) 1, right._data);
      BigInteger bigInteger3 = bigInteger2;
      while (bigInteger1._data.Length > 1)
      {
        bigInteger3 = bigInteger1;
        bigInteger1 = bigInteger2 % bigInteger1;
        bigInteger2 = bigInteger3;
      }
      if (bigInteger1.IsZero)
        return bigInteger3;
      uint num1 = bigInteger1._data[0];
      uint num2 = (uint) (bigInteger2 % (BigInteger) num1);
      int num3 = 0;
      while ((((int) num2 | (int) num1) & 1) == 0)
      {
        num2 >>= 1;
        num1 >>= 1;
        ++num3;
      }
      while (num2 > 0U)
      {
        while (((int) num2 & 1) == 0)
          num2 >>= 1;
        while (((int) num1 & 1) == 0)
          num1 >>= 1;
        if (num2 >= num1)
          num2 = num2 - num1 >> 1;
        else
          num1 = num1 - num2 >> 1;
      }
      return (BigInteger) (num1 << num3);
    }

    public static double Log(BigInteger value, double baseValue)
    {
      if (value._sign == (short) -1 || baseValue == 1.0 || baseValue == -1.0 || baseValue == double.NegativeInfinity || double.IsNaN(baseValue))
        return double.NaN;
      if (baseValue == 0.0 || baseValue == double.PositiveInfinity)
        return value.IsOne ? 0.0 : double.NaN;
      if (value._data == null)
        return double.NegativeInfinity;
      int index1 = value._data.Length - 1;
      int num1 = -1;
      for (int index2 = 31; index2 >= 0; --index2)
      {
        if (((ulong) value._data[index1] & (ulong) (1 << index2)) > 0UL)
        {
          num1 = index2 + index1 * 32;
          break;
        }
      }
      long num2 = (long) num1;
      double d = 0.0;
      double num3 = 1.0;
      BigInteger one = BigInteger.One;
      long num4;
      for (num4 = num2; num4 > (long) int.MaxValue; num4 -= (long) int.MaxValue)
        one <<= int.MaxValue;
      BigInteger bigInteger = one << (int) num4;
      for (long index3 = num2; index3 >= 0L; --index3)
      {
        if ((value & bigInteger)._sign != (short) 0)
          d += num3;
        num3 *= 0.5;
        bigInteger >>= 1;
      }
      return (Math.Log(d) + Math.Log(2.0) * (double) num2) / Math.Log(baseValue);
    }

    public static double Log(BigInteger value) => BigInteger.Log(value, Math.E);

    public static double Log10(BigInteger value) => BigInteger.Log(value, 10.0);

    [CLSCompliant(false)]
    public bool Equals(ulong other) => this.CompareTo(other) == 0;

    public override int GetHashCode()
    {
      uint hashCode = (uint) ((ulong) this._sign * 16843009UL);
      if (this._data != null)
      {
        foreach (uint num in this._data)
          hashCode ^= num;
      }
      return (int) hashCode;
    }

    public static BigInteger Add(BigInteger left, BigInteger right) => left + right;

    public static BigInteger Subtract(BigInteger left, BigInteger right) => left - right;

    public static BigInteger Multiply(BigInteger left, BigInteger right) => left * right;

    public static BigInteger Divide(BigInteger dividend, BigInteger divisor) => dividend / divisor;

    public static BigInteger Remainder(BigInteger dividend, BigInteger divisor) => dividend % divisor;

    public static BigInteger Negate(BigInteger value) => -value;

    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      return !(obj is BigInteger right) ? -1 : BigInteger.Compare(this, right);
    }

    public int CompareTo(BigInteger other) => BigInteger.Compare(this, other);

    [CLSCompliant(false)]
    public int CompareTo(ulong other)
    {
      if (this._sign < (short) 0)
        return -1;
      if (this._sign == (short) 0)
        return other == 0UL ? 0 : -1;
      if (this._data.Length > 2)
        return 1;
      uint high = (uint) (other >> 32);
      return this.LongCompare((uint) other, high);
    }

    private int LongCompare(uint low, uint high)
    {
      uint num1 = 0;
      if (this._data.Length > 1)
        num1 = this._data[1];
      if (num1 > high)
        return 1;
      if (num1 < high)
        return -1;
      uint num2 = this._data[0];
      if (num2 > low)
        return 1;
      return num2 < low ? -1 : 0;
    }

    public int CompareTo(long other)
    {
      int sign = (int) this._sign;
      int num1 = Math.Sign(other);
      if (sign != num1)
        return sign > num1 ? 1 : -1;
      if (sign == 0)
        return 0;
      if (this._data.Length > 2)
        return (int) this._sign;
      if (other < 0L)
        other = -other;
      int num2 = this.LongCompare((uint) other, (uint) ((ulong) other >> 32));
      if (sign == -1)
        num2 = -num2;
      return num2;
    }

    public static int Compare(BigInteger left, BigInteger right)
    {
      int sign1 = (int) left._sign;
      int sign2 = (int) right._sign;
      if (sign1 != sign2)
        return sign1 > sign2 ? 1 : -1;
      int num = BigInteger.CoreCompare(left._data, right._data);
      if (sign1 < 0)
        num = -num;
      return num;
    }

    private static int TopByte(uint x) => (x & 4294901760U) > 0U ? ((x & 4278190080U) > 0U ? 4 : 3) : ((x & 65280U) > 0U ? 2 : 1);

    private static int FirstNonFfByte(uint word)
    {
      if (((int) word & -16777216) != -16777216)
        return 4;
      if (((int) word & 16711680) != 16711680)
        return 3;
      return ((int) word & 65280) != 65280 ? 2 : 1;
    }

    public byte[] ToByteArray()
    {
      if (this._sign == (short) 0)
        return new byte[1];
      int num1 = (this._data.Length - 1) * 4;
      bool flag1 = false;
      uint x = this._data[this._data.Length - 1];
      int num2;
      if (this._sign == (short) 1)
      {
        num2 = BigInteger.TopByte(x);
        uint num3 = (uint) (128 << (num2 - 1) * 8);
        if ((x & num3) > 0U)
          flag1 = true;
      }
      else
        num2 = BigInteger.TopByte(x);
      byte[] array = new byte[num1 + num2 + (flag1 ? 1 : 0)];
      if (this._sign == (short) 1)
      {
        int num4 = 0;
        int num5 = this._data.Length - 1;
        for (int index1 = 0; index1 < num5; ++index1)
        {
          uint num6 = this._data[index1];
          byte[] numArray1 = array;
          int index2 = num4;
          int num7 = index2 + 1;
          int num8 = (int) (byte) num6;
          numArray1[index2] = (byte) num8;
          byte[] numArray2 = array;
          int index3 = num7;
          int num9 = index3 + 1;
          int num10 = (int) (byte) (num6 >> 8);
          numArray2[index3] = (byte) num10;
          byte[] numArray3 = array;
          int index4 = num9;
          int num11 = index4 + 1;
          int num12 = (int) (byte) (num6 >> 16);
          numArray3[index4] = (byte) num12;
          byte[] numArray4 = array;
          int index5 = num11;
          num4 = index5 + 1;
          int num13 = (int) (byte) (num6 >> 24);
          numArray4[index5] = (byte) num13;
        }
        while (num2-- > 0)
        {
          array[num4++] = (byte) x;
          x >>= 8;
        }
      }
      else
      {
        int num14 = 0;
        int num15 = this._data.Length - 1;
        uint num16 = 1;
        for (int index6 = 0; index6 < num15; ++index6)
        {
          ulong num17 = (ulong) ~this._data[index6] + (ulong) num16;
          uint num18 = (uint) num17;
          num16 = (uint) (num17 >> 32);
          byte[] numArray5 = array;
          int index7 = num14;
          int num19 = index7 + 1;
          int num20 = (int) (byte) num18;
          numArray5[index7] = (byte) num20;
          byte[] numArray6 = array;
          int index8 = num19;
          int num21 = index8 + 1;
          int num22 = (int) (byte) (num18 >> 8);
          numArray6[index8] = (byte) num22;
          byte[] numArray7 = array;
          int index9 = num21;
          int num23 = index9 + 1;
          int num24 = (int) (byte) (num18 >> 16);
          numArray7[index9] = (byte) num24;
          byte[] numArray8 = array;
          int index10 = num23;
          num14 = index10 + 1;
          int num25 = (int) (byte) (num18 >> 24);
          numArray8[index10] = (byte) num25;
        }
        ulong num26 = (ulong) ~x + (ulong) num16;
        uint word = (uint) num26;
        int num27;
        if ((uint) (num26 >> 32) == 0U)
        {
          int num28 = BigInteger.FirstNonFfByte(word);
          bool flag2 = ((long) word & (long) (1 << num28 * 8 - 1)) == 0L;
          int num29 = num28 + (flag2 ? 1 : 0);
          if (num29 != num2)
            Array.Resize<byte>(ref array, num1 + num29);
          while (num28-- > 0)
          {
            array[num14++] = (byte) word;
            word >>= 8;
          }
          if (flag2)
          {
            byte[] numArray = array;
            int index = num14;
            num27 = index + 1;
            numArray[index] = byte.MaxValue;
          }
        }
        else
        {
          Array.Resize<byte>(ref array, num1 + 5);
          byte[] numArray9 = array;
          int index11 = num14;
          int num30 = index11 + 1;
          int num31 = (int) (byte) word;
          numArray9[index11] = (byte) num31;
          byte[] numArray10 = array;
          int index12 = num30;
          int num32 = index12 + 1;
          int num33 = (int) (byte) (word >> 8);
          numArray10[index12] = (byte) num33;
          byte[] numArray11 = array;
          int index13 = num32;
          int num34 = index13 + 1;
          int num35 = (int) (byte) (word >> 16);
          numArray11[index13] = (byte) num35;
          byte[] numArray12 = array;
          int index14 = num34;
          int num36 = index14 + 1;
          int num37 = (int) (byte) (word >> 24);
          numArray12[index14] = (byte) num37;
          byte[] numArray13 = array;
          int index15 = num36;
          num27 = index15 + 1;
          numArray13[index15] = byte.MaxValue;
        }
      }
      return array;
    }

    private static uint[] CoreAdd(uint[] a, uint[] b)
    {
      if (a.Length < b.Length)
      {
        uint[] numArray = a;
        a = b;
        b = numArray;
      }
      int length1 = a.Length;
      int length2 = b.Length;
      uint[] array = new uint[length1];
      ulong num1 = 0;
      int index;
      for (index = 0; index < length2; ++index)
      {
        ulong num2 = num1 + (ulong) a[index] + (ulong) b[index];
        array[index] = (uint) num2;
        num1 = num2 >> 32;
      }
      for (; index < length1; ++index)
      {
        ulong num3 = num1 + (ulong) a[index];
        array[index] = (uint) num3;
        num1 = num3 >> 32;
      }
      if (num1 > 0UL)
      {
        Array.Resize<uint>(ref array, length1 + 1);
        array[index] = (uint) num1;
      }
      return array;
    }

    private static uint[] CoreSub(uint[] a, uint[] b)
    {
      int length1 = a.Length;
      int length2 = b.Length;
      uint[] array = new uint[length1];
      ulong num1 = 0;
      int index1;
      for (index1 = 0; index1 < length2; ++index1)
      {
        ulong num2 = (ulong) a[index1] - (ulong) b[index1] - num1;
        array[index1] = (uint) num2;
        num1 = num2 >> 32 & 1UL;
      }
      for (; index1 < length1; ++index1)
      {
        ulong num3 = (ulong) a[index1] - num1;
        array[index1] = (uint) num3;
        num1 = num3 >> 32 & 1UL;
      }
      int index2 = length1 - 1;
      while (index2 >= 0 && array[index2] == 0U)
        --index2;
      if (index2 < length1 - 1)
        Array.Resize<uint>(ref array, index2 + 1);
      return array;
    }

    private static uint[] CoreAdd(uint[] a, uint b)
    {
      int length = a.Length;
      uint[] array = new uint[length];
      ulong num1 = (ulong) b;
      int index;
      for (index = 0; index < length; ++index)
      {
        ulong num2 = num1 + (ulong) a[index];
        array[index] = (uint) num2;
        num1 = num2 >> 32;
      }
      if (num1 > 0UL)
      {
        Array.Resize<uint>(ref array, length + 1);
        array[index] = (uint) num1;
      }
      return array;
    }

    private static uint[] CoreSub(uint[] a, uint b)
    {
      int length = a.Length;
      uint[] array = new uint[length];
      ulong num1 = (ulong) b;
      for (int index = 0; index < length; ++index)
      {
        ulong num2 = (ulong) a[index] - num1;
        array[index] = (uint) num2;
        num1 = num2 >> 32 & 1UL;
      }
      int index1 = length - 1;
      while (index1 >= 0 && array[index1] == 0U)
        --index1;
      if (index1 < length - 1)
        Array.Resize<uint>(ref array, index1 + 1);
      return array;
    }

    private static int CoreCompare(uint[] a, uint[] b)
    {
      int num1 = a != null ? a.Length : 0;
      int num2 = b != null ? b.Length : 0;
      if (num1 > num2)
        return 1;
      if (num2 > num1)
        return -1;
      for (int index = num1 - 1; index >= 0; --index)
      {
        uint num3 = a[index];
        uint num4 = b[index];
        if (num3 > num4)
          return 1;
        if (num3 < num4)
          return -1;
      }
      return 0;
    }

    private static int GetNormalizeShift(uint value)
    {
      int normalizeShift = 0;
      if (((int) value & -65536) == 0)
      {
        value <<= 16;
        normalizeShift += 16;
      }
      if (((int) value & -16777216) == 0)
      {
        value <<= 8;
        normalizeShift += 8;
      }
      if (((int) value & -268435456) == 0)
      {
        value <<= 4;
        normalizeShift += 4;
      }
      if (((int) value & -1073741824) == 0)
      {
        value <<= 2;
        normalizeShift += 2;
      }
      if (((int) value & int.MinValue) == 0)
      {
        value <<= 1;
        ++normalizeShift;
      }
      return normalizeShift;
    }

    private static void Normalize(uint[] u, int l, uint[] un, int shift)
    {
      uint num1 = 0;
      int index;
      if (shift > 0)
      {
        int num2 = 32 - shift;
        for (index = 0; index < l; ++index)
        {
          uint num3 = u[index];
          un[index] = num3 << shift | num1;
          num1 = num3 >> num2;
        }
      }
      else
      {
        for (index = 0; index < l; ++index)
          un[index] = u[index];
      }
      while (index < un.Length)
        un[index++] = 0U;
      if (num1 <= 0U)
        return;
      un[l] = num1;
    }

    private static void Unnormalize(uint[] un, out uint[] r, int shift)
    {
      int length = un.Length;
      r = new uint[length];
      if (shift > 0)
      {
        int num1 = 32 - shift;
        uint num2 = 0;
        for (int index = length - 1; index >= 0; --index)
        {
          uint num3 = un[index];
          r[index] = num3 >> shift | num2;
          num2 = num3 << num1;
        }
      }
      else
      {
        for (int index = 0; index < length; ++index)
          r[index] = un[index];
      }
    }

    private static void DivModUnsigned(uint[] u, uint[] v, out uint[] q, out uint[] r)
    {
      int length1 = u.Length;
      int length2 = v.Length;
      if (length2 <= 1)
      {
        ulong num1 = 0;
        uint num2 = v[0];
        q = new uint[length1];
        r = new uint[1];
        for (int index = length1 - 1; index >= 0; --index)
        {
          ulong num3 = num1 * 4294967296UL + (ulong) u[index];
          ulong num4 = num3 / (ulong) num2;
          num1 = num3 - num4 * (ulong) num2;
          q[index] = (uint) num4;
        }
        r[0] = (uint) num1;
      }
      else if (length1 >= length2)
      {
        int normalizeShift = BigInteger.GetNormalizeShift(v[length2 - 1]);
        uint[] un1 = new uint[length1 + 1];
        uint[] un2 = new uint[length2];
        BigInteger.Normalize(u, length1, un1, normalizeShift);
        BigInteger.Normalize(v, length2, un2, normalizeShift);
        q = new uint[length1 - length2 + 1];
        r = (uint[]) null;
        for (int index1 = length1 - length2; index1 >= 0; --index1)
        {
          ulong num5 = 4294967296UL * (ulong) un1[index1 + length2] + (ulong) un1[index1 + length2 - 1];
          ulong num6 = num5 / (ulong) un2[length2 - 1];
          ulong num7 = num5 - num6 * (ulong) un2[length2 - 1];
          while (num6 >= 4294967296UL || num6 * (ulong) un2[length2 - 2] > num7 * 4294967296UL + (ulong) un1[index1 + length2 - 2])
          {
            --num6;
            num7 += (ulong) un2[length2 - 1];
            if (num7 >= 4294967296UL)
              break;
          }
          long num8 = 0;
          for (int index2 = 0; index2 < length2; ++index2)
          {
            ulong num9 = (ulong) un2[index2] * num6;
            long num10 = (long) un1[index2 + index1] - (long) (uint) num9 - num8;
            un1[index2 + index1] = (uint) num10;
            num8 = (long) (num9 >> 32) - (num10 >> 32);
          }
          long num11 = (long) un1[index1 + length2] - num8;
          un1[index1 + length2] = (uint) num11;
          q[index1] = (uint) num6;
          if (num11 < 0L)
          {
            --q[index1];
            ulong num12 = 0;
            for (int index3 = 0; index3 < length2; ++index3)
            {
              ulong num13 = (ulong) un2[index3] + (ulong) un1[index1 + index3] + num12;
              un1[index1 + index3] = (uint) num13;
              num12 = num13 >> 32;
            }
            ulong num14 = num12 + (ulong) un1[index1 + length2];
            un1[index1 + length2] = (uint) num14;
          }
        }
        BigInteger.Unnormalize(un1, out r, normalizeShift);
      }
      else
      {
        q = new uint[1];
        r = u;
      }
    }
  }
}
