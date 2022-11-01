// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.SshDataStream
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Renci.SshNet.Common
{
  public class SshDataStream : MemoryStream
  {
    public SshDataStream(int capacity)
      : base(capacity)
    {
    }

    public SshDataStream(byte[] buffer)
      : base(buffer)
    {
    }

    public SshDataStream(byte[] buffer, int offset, int count)
      : base(buffer, offset, count)
    {
    }

    public bool IsEndOfData => this.Position >= this.Length;

    public void Write(uint value)
    {
      byte[] bigEndian = Pack.UInt32ToBigEndian(value);
      this.Write(bigEndian, 0, bigEndian.Length);
    }

    public void Write(ulong value)
    {
      byte[] bigEndian = Pack.UInt64ToBigEndian(value);
      this.Write(bigEndian, 0, bigEndian.Length);
    }

    public void Write(BigInteger data)
    {
      byte[] buffer = data.ToByteArray().Reverse<byte>();
      this.WriteBinary(buffer, 0, buffer.Length);
    }

    public void Write(byte[] data)
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      this.Write(data, 0, data.Length);
    }

    public byte[] ReadBinary()
    {
      uint length = this.ReadUInt32();
      return length <= (uint) int.MaxValue ? this.ReadBytes((int) length) : throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Data longer than {0} is not supported.", (object) int.MaxValue));
    }

    public void WriteBinary(byte[] buffer)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      this.WriteBinary(buffer, 0, buffer.Length);
    }

    public void WriteBinary(byte[] buffer, int offset, int count)
    {
      this.Write((uint) count);
      this.Write(buffer, offset, count);
    }

    public void Write(string s, Encoding encoding)
    {
      byte[] buffer = encoding != null ? encoding.GetBytes(s) : throw new ArgumentNullException(nameof (encoding));
      this.WriteBinary(buffer, 0, buffer.Length);
    }

    public BigInteger ReadBigInt() => new BigInteger(this.ReadBytes((int) this.ReadUInt32()).Reverse<byte>());

    public uint ReadUInt32() => Pack.BigEndianToUInt32(this.ReadBytes(4));

    public ulong ReadUInt64() => Pack.BigEndianToUInt64(this.ReadBytes(8));

    public string ReadString(Encoding encoding)
    {
      uint length = this.ReadUInt32();
      byte[] bytes = length <= (uint) int.MaxValue ? this.ReadBytes((int) length) : throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", (object) int.MaxValue));
      return encoding.GetString(bytes, 0, bytes.Length);
    }

    private byte[] ReadBytes(int length)
    {
      byte[] buffer = new byte[length];
      int num = this.Read(buffer, 0, length);
      if (num < length)
        throw new ArgumentOutOfRangeException(nameof (length), string.Format((IFormatProvider) CultureInfo.InvariantCulture, "The requested length ({0}) is greater than the actual number of bytes read ({1}).", (object) length, (object) num));
      return buffer;
    }

    public override byte[] ToArray() => (long) this.Capacity == this.Length ? this.GetBuffer() : base.ToArray();
  }
}
