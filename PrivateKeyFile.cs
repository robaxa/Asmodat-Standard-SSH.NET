// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.PrivateKeyFile
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
  public class PrivateKeyFile : IDisposable
  {
    private static readonly Regex PrivateKeyRegex = new Regex("^-+ *BEGIN (?<keyName>\\w+( \\w+)*) PRIVATE KEY *-+\\r?\\n((Proc-Type: 4,ENCRYPTED\\r?\\nDEK-Info: (?<cipherName>[A-Z0-9-]+),(?<salt>[A-F0-9]+)\\r?\\n\\r?\\n)|(Comment: \"?[^\\r\\n]*\"?\\r?\\n))?(?<data>([a-zA-Z0-9/+=]{1,80}\\r?\\n)+)-+ *END \\k<keyName> PRIVATE KEY *-+", RegexOptions.Multiline | RegexOptions.Compiled);
    private Key _key;
    private bool _isDisposed;

    public HostAlgorithm HostKey { get; private set; }

    public PrivateKeyFile(Stream privateKey) => this.Open(privateKey, (string) null);

    public PrivateKeyFile(string fileName)
      : this(fileName, (string) null)
    {
    }

    public PrivateKeyFile(string fileName, string passPhrase)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentNullException(nameof (fileName));
      using (FileStream privateKey = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        this.Open((Stream) privateKey, passPhrase);
    }

    public PrivateKeyFile(Stream privateKey, string passPhrase) => this.Open(privateKey, passPhrase);

    private void Open(Stream privateKey, string passPhrase)
    {
      if (privateKey == null)
        throw new ArgumentNullException(nameof (privateKey));
      Match match;
      using (StreamReader streamReader = new StreamReader(privateKey))
      {
        string end = streamReader.ReadToEnd();
        match = PrivateKeyFile.PrivateKeyRegex.Match(end);
      }
      string str1 = match.Success ? match.Result("${keyName}") : throw new SshException("Invalid private key file.");
      string str2 = match.Result("${cipherName}");
      string str3 = match.Result("${salt}");
      byte[] cipherData = Convert.FromBase64String(match.Result("${data}"));
      byte[] data1;
      if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3))
      {
        if (string.IsNullOrEmpty(passPhrase))
          throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
        byte[] binarySalt = new byte[str3.Length / 2];
        for (int index = 0; index < binarySalt.Length; ++index)
          binarySalt[index] = Convert.ToByte(str3.Substring(index * 2, 2), 16);
        string str4 = str2;
        CipherInfo cipherInfo;
        if (!(str4 == "DES-EDE3-CBC"))
        {
          if (!(str4 == "DES-EDE3-CFB"))
          {
            if (!(str4 == "DES-CBC"))
            {
              if (!(str4 == "AES-128-CBC"))
              {
                if (!(str4 == "AES-192-CBC"))
                {
                  if (!(str4 == "AES-256-CBC"))
                    throw new SshException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Private key cipher \"{0}\" is not supported.", (object) str2));
                  cipherInfo = new CipherInfo(256, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) new PKCS7Padding())));
                }
                else
                  cipherInfo = new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) new PKCS7Padding())));
              }
              else
                cipherInfo = new CipherInfo(128, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new AesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) new PKCS7Padding())));
            }
            else
              cipherInfo = new CipherInfo(64, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new DesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) new PKCS7Padding())));
          }
          else
            cipherInfo = new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TripleDesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CfbCipherMode(iv), (CipherPadding) new PKCS7Padding())));
        }
        else
          cipherInfo = new CipherInfo(192, (Func<byte[], byte[], Cipher>) ((key, iv) => (Cipher) new TripleDesCipher(key, (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(iv), (CipherPadding) new PKCS7Padding())));
        data1 = PrivateKeyFile.DecryptKey(cipherInfo, cipherData, passPhrase, binarySalt);
      }
      else
        data1 = cipherData;
      string str5 = str1;
      if (!(str5 == "RSA"))
      {
        if (!(str5 == "DSA"))
        {
          if (!(str5 == "SSH2 ENCRYPTED"))
            throw new NotSupportedException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Key '{0}' is not supported.", (object) str1));
          PrivateKeyFile.SshDataReader sshDataReader1 = new PrivateKeyFile.SshDataReader(data1);
          int num = sshDataReader1.ReadUInt32() == 1064303083U ? (int) sshDataReader1.ReadUInt32() : throw new SshException("Invalid SSH2 private key.");
          string str6 = sshDataReader1.ReadString(SshData.Ascii);
          string str7 = sshDataReader1.ReadString(SshData.Ascii);
          int length = (int) sshDataReader1.ReadUInt32();
          byte[] data2;
          if (str7 == "none")
          {
            data2 = sshDataReader1.ReadBytes(length);
          }
          else
          {
            if (!(str7 == "3des-cbc"))
              throw new SshException(string.Format("Cipher method '{0}' is not supported.", (object) str2));
            if (string.IsNullOrEmpty(passPhrase))
              throw new SshPassPhraseNullOrEmptyException("Private key is encrypted but passphrase is empty.");
            data2 = new TripleDesCipher(PrivateKeyFile.GetCipherKey(passPhrase, 24), (Renci.SshNet.Security.Cryptography.Ciphers.CipherMode) new CbcCipherMode(new byte[8]), (CipherPadding) new PKCS7Padding()).Decrypt(sshDataReader1.ReadBytes(length));
          }
          PrivateKeyFile.SshDataReader sshDataReader2 = new PrivateKeyFile.SshDataReader(data2);
          if ((long) sshDataReader2.ReadUInt32() > (long) (length - 4))
            throw new SshException("Invalid passphrase.");
          if (str6 == "if-modn{sign{rsa-pkcs1-sha1},encrypt{rsa-pkcs1v2-oaep}}")
          {
            BigInteger exponent = sshDataReader2.ReadBigIntWithBits();
            BigInteger d = sshDataReader2.ReadBigIntWithBits();
            BigInteger modulus = sshDataReader2.ReadBigIntWithBits();
            BigInteger inverseQ = sshDataReader2.ReadBigIntWithBits();
            BigInteger q = sshDataReader2.ReadBigIntWithBits();
            BigInteger p = sshDataReader2.ReadBigIntWithBits();
            this._key = (Key) new RsaKey(modulus, exponent, d, p, q, inverseQ);
            this.HostKey = (HostAlgorithm) new KeyHostAlgorithm("ssh-rsa", this._key);
          }
          else
          {
            if (!(str6 == "dl-modp{sign{dsa-nist-sha1},dh{plain}}"))
              throw new NotSupportedException(string.Format("Key type '{0}' is not supported.", (object) str6));
            BigInteger p = sshDataReader2.ReadUInt32() <= 0U ? sshDataReader2.ReadBigIntWithBits() : throw new SshException("Invalid private key");
            BigInteger g = sshDataReader2.ReadBigIntWithBits();
            BigInteger q = sshDataReader2.ReadBigIntWithBits();
            BigInteger y = sshDataReader2.ReadBigIntWithBits();
            BigInteger x = sshDataReader2.ReadBigIntWithBits();
            this._key = (Key) new DsaKey(p, q, g, y, x);
            this.HostKey = (HostAlgorithm) new KeyHostAlgorithm("ssh-dss", this._key);
          }
        }
        else
        {
          this._key = (Key) new DsaKey(data1);
          this.HostKey = (HostAlgorithm) new KeyHostAlgorithm("ssh-dss", this._key);
        }
      }
      else
      {
        this._key = (Key) new RsaKey(data1);
        this.HostKey = (HostAlgorithm) new KeyHostAlgorithm("ssh-rsa", this._key);
      }
    }

    private static byte[] GetCipherKey(string passphrase, int length)
    {
      List<byte> byteList = new List<byte>();
      using (MD5 md5 = CryptoAbstraction.CreateMD5())
      {
        byte[] bytes = Encoding.UTF8.GetBytes(passphrase);
        byte[] hash = md5.ComputeHash(bytes);
        byteList.AddRange((IEnumerable<byte>) hash);
        while (byteList.Count < length)
        {
          byte[] buffer = bytes.Concat(hash);
          hash = md5.ComputeHash(buffer);
          byteList.AddRange((IEnumerable<byte>) hash);
        }
      }
      return byteList.ToArray().Take(length);
    }

    private static byte[] DecryptKey(
      CipherInfo cipherInfo,
      byte[] cipherData,
      string passPhrase,
      byte[] binarySalt)
    {
      if (cipherInfo == null)
        throw new ArgumentNullException(nameof (cipherInfo));
      if (cipherData == null)
        throw new ArgumentNullException(nameof (cipherData));
      if (binarySalt == null)
        throw new ArgumentNullException(nameof (binarySalt));
      List<byte> byteList = new List<byte>();
      using (MD5 md5 = CryptoAbstraction.CreateMD5())
      {
        byte[] numArray = Encoding.UTF8.GetBytes(passPhrase).Concat(binarySalt.Take(8));
        byte[] hash = md5.ComputeHash(numArray);
        byteList.AddRange((IEnumerable<byte>) hash);
        while (byteList.Count < cipherInfo.KeySize / 8)
        {
          byte[] buffer = hash.Concat(numArray);
          hash = md5.ComputeHash(buffer);
          byteList.AddRange((IEnumerable<byte>) hash);
        }
      }
      return cipherInfo.Cipher(byteList.ToArray(), binarySalt).Decrypt(cipherData);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._isDisposed || !disposing)
        return;
      Key key = this._key;
      if (key != null)
      {
        ((IDisposable) key).Dispose();
        this._key = (Key) null;
      }
      this._isDisposed = true;
    }

    ~PrivateKeyFile() => this.Dispose(false);

    private class SshDataReader : SshData
    {
      public SshDataReader(byte[] data) => this.Load(data);

      public new uint ReadUInt32() => base.ReadUInt32();

      public new string ReadString(Encoding encoding) => base.ReadString(encoding);

      public new byte[] ReadBytes(int length) => base.ReadBytes(length);

      public BigInteger ReadBigIntWithBits()
      {
        byte[] src = base.ReadBytes(((int) base.ReadUInt32() + 7) / 8);
        byte[] numArray = new byte[src.Length + 1];
        Buffer.BlockCopy((Array) src, 0, (Array) numArray, 1, src.Length);
        return new BigInteger(numArray.Reverse<byte>());
      }

      protected override void LoadData()
      {
      }

      protected override void SaveData()
      {
      }
    }
  }
}
