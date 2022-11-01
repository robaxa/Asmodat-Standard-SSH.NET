// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.DerData
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Collections.Generic;

namespace Renci.SshNet.Common
{
  public class DerData
  {
    private const byte Constructed = 32;
    private const byte Boolean = 1;
    private const byte Integer = 2;
    private const byte Octetstring = 4;
    private const byte Null = 5;
    private const byte Objectidentifier = 6;
    private const byte Sequence = 16;
    private readonly List<byte> _data;
    private int _readerIndex;
    private readonly int _lastIndex;

    public bool IsEndOfData => this._readerIndex >= this._lastIndex;

    public DerData() => this._data = new List<byte>();

    public DerData(byte[] data)
    {
      this._data = new List<byte>((IEnumerable<byte>) data);
      int num = (int) this.ReadByte();
      this._lastIndex = this._readerIndex + this.ReadLength();
    }

    public byte[] Encode()
    {
      this._data.InsertRange(0, DerData.GetLength(this._data.Count));
      this._data.Insert(0, (byte) 48);
      return this._data.ToArray();
    }

    public BigInteger ReadBigInteger()
    {
      if (this.ReadByte() != (byte) 2)
        throw new InvalidOperationException("Invalid data type, INTEGER(02) is expected.");
      return new BigInteger(this.ReadBytes(this.ReadLength()).Reverse<byte>());
    }

    public int ReadInteger()
    {
      if (this.ReadByte() != (byte) 2)
        throw new InvalidOperationException("Invalid data type, INTEGER(02) is expected.");
      int length = this.ReadLength();
      byte[] numArray = this.ReadBytes(length);
      if (length > 4)
        throw new InvalidOperationException("Integer type cannot occupy more then 4 bytes");
      int num1 = 0;
      int num2 = (length - 1) * 8;
      for (int index = 0; index < length; ++index)
      {
        num1 |= (int) numArray[index] << num2;
        num2 -= 8;
      }
      return num1;
    }

    public void Write(bool data)
    {
      this._data.Add((byte) 1);
      this._data.Add((byte) 1);
      this._data.Add(data ? (byte) 1 : (byte) 0);
    }

    public void Write(uint data)
    {
      byte[] bigEndian = Pack.UInt32ToBigEndian(data);
      this._data.Add((byte) 2);
      this.WriteBytes(DerData.GetLength(bigEndian.Length));
      this.WriteBytes((IEnumerable<byte>) bigEndian);
    }

    public void Write(BigInteger data)
    {
      byte[] data1 = data.ToByteArray().Reverse<byte>();
      this._data.Add((byte) 2);
      this.WriteBytes(DerData.GetLength(data1.Length));
      this.WriteBytes((IEnumerable<byte>) data1);
    }

    public void Write(byte[] data)
    {
      this._data.Add((byte) 4);
      this.WriteBytes(DerData.GetLength(data.Length));
      this.WriteBytes((IEnumerable<byte>) data);
    }

    public void Write(ObjectIdentifier identifier)
    {
      ulong[] dst = new ulong[identifier.Identifiers.Length - 1];
      dst[0] = identifier.Identifiers[0] * 40UL + identifier.Identifiers[1];
      Buffer.BlockCopy((Array) identifier.Identifiers, 16, (Array) dst, 8, (identifier.Identifiers.Length - 2) * 8);
      List<byte> data = new List<byte>();
      foreach (ulong num1 in dst)
      {
        byte[] numArray = new byte[8];
        int index1 = numArray.Length - 1;
        byte num2 = (byte) (num1 & (ulong) sbyte.MaxValue);
        do
        {
          numArray[index1] = num2;
          if (index1 < numArray.Length - 1)
            numArray[index1] |= (byte) 128;
          num1 >>= 7;
          num2 = (byte) (num1 & (ulong) sbyte.MaxValue);
          --index1;
        }
        while (num2 > (byte) 0);
        for (int index2 = index1 + 1; index2 < numArray.Length; ++index2)
          data.Add(numArray[index2]);
      }
      this._data.Add((byte) 6);
      this.WriteBytes(DerData.GetLength(data.Count));
      this.WriteBytes((IEnumerable<byte>) data);
    }

    public void WriteNull()
    {
      this._data.Add((byte) 5);
      this._data.Add((byte) 0);
    }

    public void Write(DerData data) => this._data.AddRange((IEnumerable<byte>) data.Encode());

    private static IEnumerable<byte> GetLength(int length)
    {
      if (length > (int) sbyte.MaxValue)
      {
        int length1 = 1;
        int num1 = length;
        while ((num1 >>= 8) != 0)
          ++length1;
        byte[] length2 = new byte[length1];
        length2[0] = (byte) (length1 | 128);
        int num2 = (length1 - 1) * 8;
        int index = 1;
        while (num2 >= 0)
        {
          length2[index] = (byte) (length >> num2);
          num2 -= 8;
          ++index;
        }
        return (IEnumerable<byte>) length2;
      }
      return (IEnumerable<byte>) new byte[1]
      {
        (byte) length
      };
    }

    private int ReadLength()
    {
      int num1 = (int) this.ReadByte();
      if (num1 == 128)
        throw new NotSupportedException("Indefinite-length encoding is not supported.");
      if (num1 > (int) sbyte.MaxValue)
      {
        int num2 = num1 & (int) sbyte.MaxValue;
        if (num2 > 4)
          throw new InvalidOperationException(string.Format("DER length is '{0}' and cannot be more than 4 bytes.", (object) num2));
        num1 = 0;
        for (int index = 0; index < num2; ++index)
        {
          int num3 = (int) this.ReadByte();
          num1 = (num1 << 8) + num3;
        }
        if (num1 < 0)
          throw new InvalidOperationException("Corrupted data - negative length found");
      }
      return num1;
    }

    private void WriteBytes(IEnumerable<byte> data) => this._data.AddRange(data);

    private byte ReadByte()
    {
      if (this._readerIndex > this._data.Count)
        throw new InvalidOperationException("Read out of boundaries.");
      return this._data[this._readerIndex++];
    }

    private byte[] ReadBytes(int length)
    {
      if (this._readerIndex + length > this._data.Count)
        throw new InvalidOperationException("Read out of boundaries.");
      byte[] array = new byte[length];
      this._data.CopyTo(this._readerIndex, array, 0, length);
      this._readerIndex += length;
      return array;
    }
  }
}
