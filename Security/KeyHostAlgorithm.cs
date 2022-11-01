// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Security.KeyHostAlgorithm
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using System.Collections.Generic;

namespace Renci.SshNet.Security
{
  public class KeyHostAlgorithm : HostAlgorithm
  {
    public Key Key { get; private set; }

    public override byte[] Data => new KeyHostAlgorithm.SshKeyData(this.Name, this.Key.Public).GetBytes();

    public KeyHostAlgorithm(string name, Key key)
      : base(name)
    {
      this.Key = key;
    }

    public KeyHostAlgorithm(string name, Key key, byte[] data)
      : base(name)
    {
      this.Key = key;
      KeyHostAlgorithm.SshKeyData sshKeyData = new KeyHostAlgorithm.SshKeyData();
      sshKeyData.Load(data);
      this.Key.Public = sshKeyData.Keys;
    }

    public override byte[] Sign(byte[] data) => new KeyHostAlgorithm.SignatureKeyData(this.Name, this.Key.Sign(data)).GetBytes();

    public override bool VerifySignature(byte[] data, byte[] signature)
    {
      KeyHostAlgorithm.SignatureKeyData signatureKeyData = new KeyHostAlgorithm.SignatureKeyData();
      signatureKeyData.Load(signature);
      return this.Key.VerifySignature(data, signatureKeyData.Signature);
    }

    private class SshKeyData : SshData
    {
      private byte[] _name;
      private IList<byte[]> _keys;

      public BigInteger[] Keys
      {
        get
        {
          BigInteger[] keys = new BigInteger[this._keys.Count];
          for (int index = 0; index < this._keys.Count; ++index)
          {
            byte[] key = this._keys[index];
            keys[index] = key.ToBigInteger();
          }
          return keys;
        }
        private set
        {
          this._keys = (IList<byte[]>) new List<byte[]>(value.Length);
          foreach (BigInteger bigInteger in value)
            this._keys.Add(bigInteger.ToByteArray().Reverse<byte>());
        }
      }

      private string Name
      {
        get => SshData.Utf8.GetString(this._name, 0, this._name.Length);
        set => this._name = SshData.Utf8.GetBytes(value);
      }

      protected override int BufferCapacity
      {
        get
        {
          int bufferCapacity = base.BufferCapacity + 4 + this._name.Length;
          foreach (byte[] key in (IEnumerable<byte[]>) this._keys)
          {
            bufferCapacity += 4;
            bufferCapacity += key.Length;
          }
          return bufferCapacity;
        }
      }

      public SshKeyData()
      {
      }

      public SshKeyData(string name, params BigInteger[] keys)
      {
        this.Name = name;
        this.Keys = keys;
      }

      protected override void LoadData()
      {
        this._name = this.ReadBinary();
        this._keys = (IList<byte[]>) new List<byte[]>();
        while (!this.IsEndOfData)
          this._keys.Add(this.ReadBinary());
      }

      protected override void SaveData()
      {
        this.WriteBinaryString(this._name);
        foreach (byte[] key in (IEnumerable<byte[]>) this._keys)
          this.WriteBinaryString(key);
      }
    }

    private class SignatureKeyData : SshData
    {
      private byte[] AlgorithmName { get; set; }

      public byte[] Signature { get; private set; }

      protected override int BufferCapacity => base.BufferCapacity + 4 + this.AlgorithmName.Length + 4 + this.Signature.Length;

      public SignatureKeyData()
      {
      }

      public SignatureKeyData(string name, byte[] signature)
      {
        this.AlgorithmName = SshData.Utf8.GetBytes(name);
        this.Signature = signature;
      }

      protected override void LoadData()
      {
        this.AlgorithmName = this.ReadBinary();
        this.Signature = this.ReadBinary();
      }

      protected override void SaveData()
      {
        this.WriteBinaryString(this.AlgorithmName);
        this.WriteBinaryString(this.Signature);
      }
    }
  }
}
