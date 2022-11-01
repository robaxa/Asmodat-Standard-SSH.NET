// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.Pack
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

namespace Renci.SshNet.Common
{
  internal static class Pack
  {
    internal static ushort LittleEndianToUInt16(byte[] buffer) => (ushort) ((uint) (ushort) buffer[0] | (uint) (ushort) ((uint) buffer[1] << 8));

    internal static uint LittleEndianToUInt32(byte[] buffer, int offset) => (uint) buffer[offset] | (uint) buffer[offset + 1] << 8 | (uint) buffer[offset + 2] << 16 | (uint) buffer[offset + 3] << 24;

    internal static uint LittleEndianToUInt32(byte[] buffer) => (uint) buffer[0] | (uint) buffer[1] << 8 | (uint) buffer[2] << 16 | (uint) buffer[3] << 24;

    internal static ulong LittleEndianToUInt64(byte[] buffer) => (ulong) buffer[0] | (ulong) buffer[1] << 8 | (ulong) buffer[2] << 16 | (ulong) buffer[3] << 24 | (ulong) buffer[4] << 32 | (ulong) buffer[5] << 40 | (ulong) buffer[6] << 48 | (ulong) buffer[7] << 56;

    internal static byte[] UInt16ToLittleEndian(ushort value)
    {
      byte[] buffer = new byte[2];
      Pack.UInt16ToLittleEndian(value, buffer);
      return buffer;
    }

    internal static void UInt16ToLittleEndian(ushort value, byte[] buffer)
    {
      buffer[0] = (byte) ((uint) value & (uint) byte.MaxValue);
      buffer[1] = (byte) (((int) value & 65280) >> 8);
    }

    internal static byte[] UInt32ToLittleEndian(uint value)
    {
      byte[] buffer = new byte[4];
      Pack.UInt32ToLittleEndian(value, buffer);
      return buffer;
    }

    internal static void UInt32ToLittleEndian(uint value, byte[] buffer)
    {
      buffer[0] = (byte) (value & (uint) byte.MaxValue);
      buffer[1] = (byte) ((value & 65280U) >> 8);
      buffer[2] = (byte) ((value & 16711680U) >> 16);
      buffer[3] = (byte) ((value & 4278190080U) >> 24);
    }

    internal static void UInt32ToLittleEndian(uint value, byte[] buffer, int offset)
    {
      buffer[offset] = (byte) (value & (uint) byte.MaxValue);
      buffer[offset + 1] = (byte) ((value & 65280U) >> 8);
      buffer[offset + 2] = (byte) ((value & 16711680U) >> 16);
      buffer[offset + 3] = (byte) ((value & 4278190080U) >> 24);
    }

    internal static byte[] UInt64ToLittleEndian(ulong value)
    {
      byte[] buffer = new byte[8];
      Pack.UInt64ToLittleEndian(value, buffer);
      return buffer;
    }

    internal static void UInt64ToLittleEndian(ulong value, byte[] buffer)
    {
      buffer[0] = (byte) (value & (ulong) byte.MaxValue);
      buffer[1] = (byte) ((value & 65280UL) >> 8);
      buffer[2] = (byte) ((value & 16711680UL) >> 16);
      buffer[3] = (byte) ((value & 4278190080UL) >> 24);
      buffer[4] = (byte) ((value & 1095216660480UL) >> 32);
      buffer[5] = (byte) ((value & 280375465082880UL) >> 40);
      buffer[6] = (byte) ((value & 71776119061217280UL) >> 48);
      buffer[7] = (byte) ((value & 18374686479671623680UL) >> 56);
    }

    internal static byte[] UInt16ToBigEndian(ushort value)
    {
      byte[] buffer = new byte[2];
      Pack.UInt16ToBigEndian(value, buffer);
      return buffer;
    }

    internal static void UInt16ToBigEndian(ushort value, byte[] buffer)
    {
      buffer[0] = (byte) ((uint) value >> 8);
      buffer[1] = (byte) ((uint) value & (uint) byte.MaxValue);
    }

    internal static void UInt16ToBigEndian(ushort value, byte[] buffer, int offset)
    {
      buffer[offset] = (byte) ((uint) value >> 8);
      buffer[offset + 1] = (byte) ((uint) value & (uint) byte.MaxValue);
    }

    internal static void UInt32ToBigEndian(uint value, byte[] buffer)
    {
      buffer[0] = (byte) ((value & 4278190080U) >> 24);
      buffer[1] = (byte) ((value & 16711680U) >> 16);
      buffer[2] = (byte) ((value & 65280U) >> 8);
      buffer[3] = (byte) (value & (uint) byte.MaxValue);
    }

    internal static void UInt32ToBigEndian(uint value, byte[] buffer, int offset)
    {
      buffer[offset++] = (byte) ((value & 4278190080U) >> 24);
      buffer[offset++] = (byte) ((value & 16711680U) >> 16);
      buffer[offset++] = (byte) ((value & 65280U) >> 8);
      buffer[offset] = (byte) (value & (uint) byte.MaxValue);
    }

    internal static byte[] UInt32ToBigEndian(uint value)
    {
      byte[] buffer = new byte[4];
      Pack.UInt32ToBigEndian(value, buffer);
      return buffer;
    }

    internal static byte[] UInt64ToBigEndian(ulong value) => new byte[8]
    {
      (byte) ((value & 18374686479671623680UL) >> 56),
      (byte) ((value & 71776119061217280UL) >> 48),
      (byte) ((value & 280375465082880UL) >> 40),
      (byte) ((value & 1095216660480UL) >> 32),
      (byte) ((value & 4278190080UL) >> 24),
      (byte) ((value & 16711680UL) >> 16),
      (byte) ((value & 65280UL) >> 8),
      (byte) (value & (ulong) byte.MaxValue)
    };

    internal static ushort BigEndianToUInt16(byte[] buffer) => (ushort) ((uint) buffer[0] << 8 | (uint) buffer[1]);

    internal static uint BigEndianToUInt32(byte[] buffer, int offset) => (uint) ((int) buffer[offset] << 24 | (int) buffer[offset + 1] << 16 | (int) buffer[offset + 2] << 8) | (uint) buffer[offset + 3];

    internal static uint BigEndianToUInt32(byte[] buffer) => (uint) ((int) buffer[0] << 24 | (int) buffer[1] << 16 | (int) buffer[2] << 8) | (uint) buffer[3];

    internal static ulong BigEndianToUInt64(byte[] buffer) => (ulong) ((long) buffer[0] << 56 | (long) buffer[1] << 48 | (long) buffer[2] << 40 | (long) buffer[3] << 32 | (long) buffer[4] << 24 | (long) buffer[5] << 16 | (long) buffer[6] << 8) | (ulong) buffer[7];
  }
}
