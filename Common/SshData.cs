// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Common.SshData
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using System;
using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Common
{
  public abstract class SshData
  {
    internal const int DefaultCapacity = 64;
    internal static readonly Encoding Ascii = Encoding.ASCII;
    internal static readonly Encoding Utf8 = Encoding.UTF8;
    private SshDataStream _stream;

    protected SshDataStream DataStream => this._stream;

    protected bool IsEndOfData => this._stream.Position >= this._stream.Length;

    protected virtual int BufferCapacity => 0;

    public byte[] GetBytes()
    {
      int bufferCapacity = this.BufferCapacity;
      SshDataStream stream = new SshDataStream(bufferCapacity != -1 ? bufferCapacity : 64);
      this.WriteBytes(stream);
      return stream.ToArray();
    }

    protected virtual void WriteBytes(SshDataStream stream)
    {
      this._stream = stream;
      this.SaveData();
    }

    public void Load(byte[] data)
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      this.LoadInternal(data, 0, data.Length);
    }

    public void Load(byte[] data, int offset, int count)
    {
      if (data == null)
        throw new ArgumentNullException(nameof (data));
      this.LoadInternal(data, offset, count);
    }

    private void LoadInternal(byte[] value, int offset, int count)
    {
      this._stream = new SshDataStream(value, offset, count);
      this.LoadData();
    }

    protected abstract void LoadData();

    protected abstract void SaveData();

    protected byte[] ReadBytes()
    {
      int count = (int) (this._stream.Length - this._stream.Position);
      byte[] buffer = new byte[count];
      this._stream.Read(buffer, 0, count);
      return buffer;
    }

    protected byte[] ReadBytes(int length)
    {
      byte[] buffer = new byte[length];
      if (this._stream.Read(buffer, 0, length) < length)
        throw new ArgumentOutOfRangeException(nameof (length));
      return buffer;
    }

    protected byte ReadByte()
    {
      int num = this._stream.ReadByte();
      return num != -1 ? (byte) num : throw new InvalidOperationException("Attempt to read past the end of the SSH data stream.");
    }

    protected bool ReadBoolean() => this.ReadByte() > (byte) 0;

    protected ushort ReadUInt16() => Pack.BigEndianToUInt16(this.ReadBytes(2));

    protected uint ReadUInt32() => Pack.BigEndianToUInt32(this.ReadBytes(4));

    protected ulong ReadUInt64() => Pack.BigEndianToUInt64(this.ReadBytes(8));

    protected string ReadString(Encoding encoding) => this._stream.ReadString(encoding);

    protected byte[] ReadBinary() => this._stream.ReadBinary();

    protected string[] ReadNamesList() => this.ReadString(SshData.Ascii).Split(',');

    protected IDictionary<string, string> ReadExtensionPair()
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      while (!this.IsEndOfData)
      {
        string key = this.ReadString(SshData.Ascii);
        string str = this.ReadString(SshData.Ascii);
        dictionary.Add(key, str);
      }
      return (IDictionary<string, string>) dictionary;
    }

    protected void Write(byte[] data) => this._stream.Write(data);

    protected void Write(byte[] buffer, int offset, int count) => this._stream.Write(buffer, offset, count);

    protected void Write(byte data) => this._stream.WriteByte(data);

    protected void Write(bool data) => this.Write(data ? (byte) 1 : (byte) 0);

    protected void Write(uint data) => this._stream.Write(data);

    protected void Write(ulong data) => this._stream.Write(data);

    protected void Write(string data) => this.Write(data, SshData.Utf8);

    protected void Write(string data, Encoding encoding) => this._stream.Write(data, encoding);

    protected void WriteBinaryString(byte[] buffer) => this._stream.WriteBinary(buffer);

    protected void WriteBinary(byte[] buffer, int offset, int count) => this._stream.WriteBinary(buffer, offset, count);

    protected void Write(BigInteger data) => this._stream.Write(data);

    protected void Write(string[] data) => this.Write(string.Join(",", data), SshData.Ascii);

    protected void Write(IDictionary<string, string> data)
    {
      foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>) data)
      {
        this.Write(keyValuePair.Key, SshData.Ascii);
        this.Write(keyValuePair.Value, SshData.Ascii);
      }
    }
  }
}
