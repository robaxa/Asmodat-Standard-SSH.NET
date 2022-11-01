// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.Extensions
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace Renci.SshNet.Common
{
  internal static class Extensions
  {
    public static bool IsNullOrWhiteSpace(this string value)
    {
      if (string.IsNullOrEmpty(value))
        return true;
      for (int index = 0; index < value.Length; ++index)
      {
        if (!char.IsWhiteSpace(value[index]))
          return false;
      }
      return true;
    }

    internal static byte[] ToArray(this ServiceName serviceName)
    {
      switch (serviceName)
      {
        case ServiceName.UserAuthentication:
          return SshData.Ascii.GetBytes("ssh-userauth");
        case ServiceName.Connection:
          return SshData.Ascii.GetBytes("ssh-connection");
        default:
          throw new NotSupportedException(string.Format("Service name '{0}' is not supported.", (object) serviceName));
      }
    }

    internal static ServiceName ToServiceName(this byte[] data)
    {
      string str1 = SshData.Ascii.GetString(data, 0, data.Length);
      string str2 = str1;
      if (str2 == "ssh-userauth")
        return ServiceName.UserAuthentication;
      if (str2 == "ssh-connection")
        return ServiceName.Connection;
      throw new NotSupportedException(string.Format("Service name '{0}' is not supported.", (object) str1));
    }

    internal static BigInteger ToBigInteger(this byte[] data)
    {
      byte[] numArray = new byte[data.Length];
      Buffer.BlockCopy((Array) data, 0, (Array) numArray, 0, data.Length);
      return new BigInteger(numArray.Reverse<byte>());
    }

    internal static T[] Reverse<T>(this T[] array)
    {
      Array.Reverse<T>(array);
      return array;
    }

    internal static void DebugPrint(this IEnumerable<byte> bytes)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (byte num in bytes)
        stringBuilder.AppendFormat((IFormatProvider) CultureInfo.CurrentCulture, "0x{0:x2}, ", (object) num);
      Debug.WriteLine(stringBuilder.ToString());
    }

    internal static T CreateInstance<T>(this Type type) where T : class => type == (Type) null ? default (T) : Activator.CreateInstance(type) as T;

    internal static void ValidatePort(this uint value, string argument)
    {
      if (value > (uint) ushort.MaxValue)
        throw new ArgumentOutOfRangeException(argument, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.", (object) (int) ushort.MaxValue));
    }

    internal static void ValidatePort(this int value, string argument)
    {
      if (value < 0)
        throw new ArgumentOutOfRangeException(argument, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Specified value cannot be less than {0}.", (object) 0));
      if (value > (int) ushort.MaxValue)
        throw new ArgumentOutOfRangeException(argument, string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.", (object) (int) ushort.MaxValue));
    }

    public static byte[] Take(this byte[] value, int offset, int count)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (count == 0)
        return Array<byte>.Empty;
      if (offset == 0 && value.Length == count)
        return value;
      byte[] dst = new byte[count];
      Buffer.BlockCopy((Array) value, offset, (Array) dst, 0, count);
      return dst;
    }

    public static byte[] Take(this byte[] value, int count)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      if (count == 0)
        return Array<byte>.Empty;
      if (value.Length == count)
        return value;
      byte[] dst = new byte[count];
      Buffer.BlockCopy((Array) value, 0, (Array) dst, 0, count);
      return dst;
    }

    public static bool IsEqualTo(this byte[] left, byte[] right)
    {
      if (left == null)
        throw new ArgumentNullException(nameof (left));
      if (right == null)
        throw new ArgumentNullException(nameof (right));
      if (left == right)
        return true;
      if (left.Length != right.Length)
        return false;
      for (int index = 0; index < left.Length; ++index)
      {
        if ((int) left[index] != (int) right[index])
          return false;
      }
      return true;
    }

    public static byte[] TrimLeadingZeros(this byte[] value)
    {
      if (value == null)
        throw new ArgumentNullException(nameof (value));
      for (int srcOffset = 0; srcOffset < value.Length; ++srcOffset)
      {
        if (value[srcOffset] != (byte) 0)
        {
          if (srcOffset == 0)
            return value;
          int count = value.Length - srcOffset;
          byte[] dst = new byte[count];
          Buffer.BlockCopy((Array) value, srcOffset, (Array) dst, 0, count);
          return dst;
        }
      }
      return value;
    }

    public static byte[] Concat(this byte[] first, byte[] second)
    {
      if (first == null || first.Length == 0)
        return second;
      if (second == null || second.Length == 0)
        return first;
      byte[] dst = new byte[first.Length + second.Length];
      Buffer.BlockCopy((Array) first, 0, (Array) dst, 0, first.Length);
      Buffer.BlockCopy((Array) second, 0, (Array) dst, first.Length, second.Length);
      return dst;
    }

    internal static bool CanRead(this Socket socket) => SocketAbstraction.CanRead(socket);

    internal static bool CanWrite(this Socket socket) => SocketAbstraction.CanWrite(socket);

    internal static bool IsConnected(this Socket socket) => socket != null && socket.Connected;
  }
}
