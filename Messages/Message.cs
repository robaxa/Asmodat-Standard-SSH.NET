// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Messages.Message
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Renci.SshNet.Messages
{
  public abstract class Message : SshData
  {
    protected override int BufferCapacity => 1;

    protected override void WriteBytes(SshDataStream stream)
    {
      using (IEnumerator<MessageAttribute> enumerator = this.GetType().GetCustomAttributes<MessageAttribute>(true).GetEnumerator())
      {
        MessageAttribute messageAttribute = enumerator.MoveNext() ? enumerator.Current : throw new SshException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Type '{0}' is not a valid message type.", (object) this.GetType().AssemblyQualifiedName));
        stream.WriteByte(messageAttribute.Number);
        base.WriteBytes(stream);
      }
    }

    internal byte[] GetPacket(byte paddingMultiplier, Compressor compressor)
    {
      int bufferCapacity = this.BufferCapacity;
      SshDataStream stream1;
      if (bufferCapacity == -1 || compressor != null)
      {
        stream1 = new SshDataStream(64);
        stream1.Seek(9L, SeekOrigin.Begin);
        if (compressor != null)
        {
          SshDataStream stream2 = new SshDataStream(bufferCapacity != -1 ? bufferCapacity : 64);
          this.WriteBytes(stream2);
          byte[] buffer = compressor.Compress(stream2.ToArray());
          stream1.Write(buffer, 0, buffer.Length);
        }
        else
          this.WriteBytes(stream1);
        int messageLength = (int) stream1.Length - 9;
        int packetLength = messageLength + 4 + 1;
        byte paddingLength = Message.GetPaddingLength(paddingMultiplier, (long) packetLength);
        byte[] numArray = new byte[(int) paddingLength];
        CryptoAbstraction.GenerateRandom(numArray);
        stream1.Write(numArray, 0, (int) paddingLength);
        uint packetDataLength = Message.GetPacketDataLength(messageLength, paddingLength);
        stream1.Seek(4L, SeekOrigin.Begin);
        stream1.Write(packetDataLength);
        stream1.WriteByte(paddingLength);
      }
      else
      {
        int packetLength = bufferCapacity + 4 + 1;
        byte paddingLength = Message.GetPaddingLength(paddingMultiplier, (long) packetLength);
        uint packetDataLength = Message.GetPacketDataLength(bufferCapacity, paddingLength);
        stream1 = new SshDataStream(packetLength + (int) paddingLength + 4);
        stream1.Seek(4L, SeekOrigin.Begin);
        stream1.Write(packetDataLength);
        stream1.WriteByte(paddingLength);
        this.WriteBytes(stream1);
        byte[] numArray = new byte[(int) paddingLength];
        CryptoAbstraction.GenerateRandom(numArray);
        stream1.Write(numArray, 0, (int) paddingLength);
      }
      return stream1.ToArray();
    }

    private static uint GetPacketDataLength(int messageLength, byte paddingLength) => (uint) (messageLength + (int) paddingLength + 1);

    private static byte GetPaddingLength(byte paddingMultiplier, long packetLength)
    {
      byte paddingLength = (byte) ((ulong) -packetLength & (ulong) ((int) paddingMultiplier - 1));
      if ((int) paddingLength < (int) paddingMultiplier)
        paddingLength += paddingMultiplier;
      return paddingLength;
    }

    public override string ToString()
    {
      using (IEnumerator<MessageAttribute> enumerator = this.GetType().GetCustomAttributes<MessageAttribute>(true).GetEnumerator())
        return !enumerator.MoveNext() ? string.Format((IFormatProvider) CultureInfo.CurrentCulture, "'{0}' without Message attribute.", (object) this.GetType().FullName) : enumerator.Current.Name;
    }

    internal abstract void Process(Session session);
  }
}
