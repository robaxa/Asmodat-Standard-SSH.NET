// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.GroupExchangeHashData
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System;

namespace Renci.SshNet.Security
{
  internal class GroupExchangeHashData : SshData
  {
    private byte[] _serverVersion;
    private byte[] _clientVersion;
    private byte[] _prime;
    private byte[] _subGroup;
    private byte[] _clientExchangeValue;
    private byte[] _serverExchangeValue;
    private byte[] _sharedKey;

    public string ServerVersion
    {
      private get => SshData.Utf8.GetString(this._serverVersion, 0, this._serverVersion.Length);
      set => this._serverVersion = SshData.Utf8.GetBytes(value);
    }

    public string ClientVersion
    {
      private get => SshData.Utf8.GetString(this._clientVersion, 0, this._clientVersion.Length);
      set => this._clientVersion = SshData.Utf8.GetBytes(value);
    }

    public byte[] ClientPayload { get; set; }

    public byte[] ServerPayload { get; set; }

    public byte[] HostKey { get; set; }

    public uint MinimumGroupSize { get; set; }

    public uint PreferredGroupSize { get; set; }

    public uint MaximumGroupSize { get; set; }

    public BigInteger Prime
    {
      private get => this._prime.ToBigInteger();
      set => this._prime = value.ToByteArray().Reverse<byte>();
    }

    public BigInteger SubGroup
    {
      private get => this._subGroup.ToBigInteger();
      set => this._subGroup = value.ToByteArray().Reverse<byte>();
    }

    public BigInteger ClientExchangeValue
    {
      private get => this._clientExchangeValue.ToBigInteger();
      set => this._clientExchangeValue = value.ToByteArray().Reverse<byte>();
    }

    public BigInteger ServerExchangeValue
    {
      private get => this._serverExchangeValue.ToBigInteger();
      set => this._serverExchangeValue = value.ToByteArray().Reverse<byte>();
    }

    public BigInteger SharedKey
    {
      private get => this._sharedKey.ToBigInteger();
      set => this._sharedKey = value.ToByteArray().Reverse<byte>();
    }

    protected override int BufferCapacity => base.BufferCapacity + 4 + this._clientVersion.Length + 4 + this._serverVersion.Length + 4 + this.ClientPayload.Length + 4 + this.ServerPayload.Length + 4 + this.HostKey.Length + 4 + 4 + 4 + 4 + this._prime.Length + 4 + this._subGroup.Length + 4 + this._clientExchangeValue.Length + 4 + this._serverExchangeValue.Length + 4 + this._sharedKey.Length;

    protected override void LoadData() => throw new NotImplementedException();

    protected override void SaveData()
    {
      this.WriteBinaryString(this._clientVersion);
      this.WriteBinaryString(this._serverVersion);
      this.WriteBinaryString(this.ClientPayload);
      this.WriteBinaryString(this.ServerPayload);
      this.WriteBinaryString(this.HostKey);
      this.Write(this.MinimumGroupSize);
      this.Write(this.PreferredGroupSize);
      this.Write(this.MaximumGroupSize);
      this.WriteBinaryString(this._prime);
      this.WriteBinaryString(this._subGroup);
      this.WriteBinaryString(this._clientExchangeValue);
      this.WriteBinaryString(this._serverExchangeValue);
      this.WriteBinaryString(this._sharedKey);
    }
  }
}
