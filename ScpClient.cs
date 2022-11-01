// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.ScpClient
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
  public class ScpClient : BaseClient
  {
    private static readonly Regex FileInfoRe = new Regex("C(?<mode>\\d{4}) (?<length>\\d+) (?<filename>.+)");
    private static readonly byte[] SuccessConfirmationCode = new byte[1];
    private static readonly byte[] ErrorConfirmationCode = new byte[1]
    {
      (byte) 1
    };
    private IRemotePathTransformation _remotePathTransformation;
    private static readonly Regex DirectoryInfoRe = new Regex("D(?<mode>\\d{4}) (?<length>\\d+) (?<filename>.+)");
    private static readonly Regex TimestampRe = new Regex("T(?<mtime>\\d+) 0 (?<atime>\\d+) 0");

    public TimeSpan OperationTimeout { get; set; }

    public uint BufferSize { get; set; }

    public IRemotePathTransformation RemotePathTransformation
    {
      get => this._remotePathTransformation;
      set => this._remotePathTransformation = value != null ? value : throw new ArgumentNullException(nameof (value));
    }

    public event EventHandler<ScpDownloadEventArgs> Downloading;

    public event EventHandler<ScpUploadEventArgs> Uploading;

    public ScpClient(ConnectionInfo connectionInfo)
      : this(connectionInfo, false)
    {
    }

    public ScpClient(string host, int port, string username, string password)
      : this((ConnectionInfo) new PasswordConnectionInfo(host, port, username, password), true)
    {
    }

    public ScpClient(string host, string username, string password)
      : this(host, ConnectionInfo.DefaultPort, username, password)
    {
    }

    public ScpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
      : this((ConnectionInfo) new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
    {
    }

    public ScpClient(string host, string username, params PrivateKeyFile[] keyFiles)
      : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
    {
    }

    private ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
      : this(connectionInfo, ownsConnectionInfo, (IServiceFactory) new ServiceFactory())
    {
    }

    internal ScpClient(
      ConnectionInfo connectionInfo,
      bool ownsConnectionInfo,
      IServiceFactory serviceFactory)
      : base(connectionInfo, ownsConnectionInfo, serviceFactory)
    {
      this.OperationTimeout = Session.InfiniteTimeSpan;
      this.BufferSize = 16384U;
      this._remotePathTransformation = serviceFactory.CreateRemotePathDoubleQuoteTransformation();
    }

    public void Upload(Stream source, string path)
    {
      using (PipeStream input = this.ServiceFactory.CreatePipeStream())
      {
        using (IChannelSession channelSession = this.Session.CreateChannelSession())
        {
          channelSession.DataReceived += (EventHandler<ChannelDataEventArgs>) ((sender, e) => input.Write(e.Data, 0, e.Data.Length));
          channelSession.Open();
          if (!channelSession.SendExecRequest(string.Format("scp -t {0}", (object) this._remotePathTransformation.Transform(path))))
            throw ScpClient.SecureExecutionRequestRejectedException();
          this.CheckReturnCode((Stream) input);
          this.UploadFileModeAndName(channelSession, (Stream) input, source.Length, string.Empty);
          this.UploadFileContent(channelSession, (Stream) input, source, PosixPath.GetFileName(path));
        }
      }
    }

    public void Download(string filename, Stream destination)
    {
      if (filename.IsNullOrWhiteSpace())
        throw new ArgumentException(nameof (filename));
      if (destination == null)
        throw new ArgumentNullException(nameof (destination));
      using (PipeStream input1 = this.ServiceFactory.CreatePipeStream())
      {
        using (IChannelSession channelSession = this.Session.CreateChannelSession())
        {
          channelSession.DataReceived += (EventHandler<ChannelDataEventArgs>) ((sender, e) => input1.Write(e.Data, 0, e.Data.Length));
          channelSession.Open();
          if (!channelSession.SendExecRequest(string.Format("scp -f {0}", (object) this._remotePathTransformation.Transform(filename))))
            throw ScpClient.SecureExecutionRequestRejectedException();
          ScpClient.SendSuccessConfirmation((IChannel) channelSession);
          string input2 = this.ReadString((Stream) input1);
          Match match = ScpClient.FileInfoRe.Match(input2);
          if (match.Success)
          {
            ScpClient.SendSuccessConfirmation((IChannel) channelSession);
            long length = long.Parse(match.Result("${length}"));
            string filename1 = match.Result("${filename}");
            this.InternalDownload((IChannel) channelSession, (Stream) input1, destination, filename1, length);
          }
          else
            this.SendErrorConfirmation((IChannel) channelSession, string.Format("\"{0}\" is not valid protocol message.", (object) input2));
        }
      }
    }

    private void UploadFileModeAndName(
      IChannelSession channel,
      Stream input,
      long fileSize,
      string serverFileName)
    {
      this.SendData((IChannel) channel, string.Format("C0644 {0} {1}\n", (object) fileSize, (object) serverFileName));
      this.CheckReturnCode(input);
    }

    private void UploadFileContent(
      IChannelSession channel,
      Stream input,
      Stream source,
      string remoteFileName)
    {
      long length1 = source.Length;
      byte[] buffer = new byte[(int) this.BufferSize];
      int length2 = source.Read(buffer, 0, buffer.Length);
      long uploaded = 0;
      for (; length2 > 0; length2 = source.Read(buffer, 0, buffer.Length))
      {
        ScpClient.SendData((IChannel) channel, buffer, length2);
        uploaded += (long) length2;
        this.RaiseUploadingEvent(remoteFileName, length1, uploaded);
      }
      ScpClient.SendSuccessConfirmation((IChannel) channel);
      this.CheckReturnCode(input);
    }

    private void InternalDownload(
      IChannel channel,
      Stream input,
      Stream output,
      string filename,
      long length)
    {
      byte[] buffer = new byte[Math.Min(length, (long) this.BufferSize)];
      long val1 = length;
      do
      {
        int count = input.Read(buffer, 0, (int) Math.Min(val1, (long) this.BufferSize));
        output.Write(buffer, 0, count);
        this.RaiseDownloadingEvent(filename, length, length - val1);
        val1 -= (long) count;
      }
      while (val1 > 0L);
      output.Flush();
      this.RaiseDownloadingEvent(filename, length, length - val1);
      ScpClient.SendSuccessConfirmation(channel);
      this.CheckReturnCode(input);
    }

    private void RaiseDownloadingEvent(string filename, long size, long downloaded)
    {
      if (this.Downloading == null)
        return;
      this.Downloading((object) this, new ScpDownloadEventArgs(filename, size, downloaded));
    }

    private void RaiseUploadingEvent(string filename, long size, long uploaded)
    {
      if (this.Uploading == null)
        return;
      this.Uploading((object) this, new ScpUploadEventArgs(filename, size, uploaded));
    }

    private static void SendSuccessConfirmation(IChannel channel) => ScpClient.SendData(channel, ScpClient.SuccessConfirmationCode);

    private void SendErrorConfirmation(IChannel channel, string message)
    {
      ScpClient.SendData(channel, ScpClient.ErrorConfirmationCode);
      this.SendData(channel, message + "\n");
    }

    private void CheckReturnCode(Stream input)
    {
      if (ScpClient.ReadByte(input) > 0)
        throw new ScpException(this.ReadString(input));
    }

    private void SendData(IChannel channel, string command) => channel.SendData(this.ConnectionInfo.Encoding.GetBytes(command));

    private static void SendData(IChannel channel, byte[] buffer, int length) => channel.SendData(buffer, 0, length);

    private static void SendData(IChannel channel, byte[] buffer) => channel.SendData(buffer);

    private static int ReadByte(Stream stream)
    {
      int num = stream.ReadByte();
      return num != -1 ? num : throw new SshException("Stream has been closed.");
    }

    private string ReadString(Stream stream)
    {
      bool flag = false;
      List<byte> byteList = new List<byte>();
      int num = ScpClient.ReadByte(stream);
      if (num == 1 || num == 2)
      {
        flag = true;
        num = ScpClient.ReadByte(stream);
      }
      for (; num != 10; num = ScpClient.ReadByte(stream))
        byteList.Add((byte) num);
      byte[] array = byteList.ToArray();
      if (flag)
        throw new ScpException(this.ConnectionInfo.Encoding.GetString(array, 0, array.Length));
      return this.ConnectionInfo.Encoding.GetString(array, 0, array.Length);
    }

    private static SshException SecureExecutionRequestRejectedException() => throw new SshException("Secure copy execution request was rejected by the server. Please consult the server logs.");

    public void Upload(FileInfo fileInfo, string path)
    {
      if (fileInfo == null)
        throw new ArgumentNullException(nameof (fileInfo));
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException(nameof (path));
      using (PipeStream input = this.ServiceFactory.CreatePipeStream())
      {
        using (IChannelSession channelSession = this.Session.CreateChannelSession())
        {
          channelSession.DataReceived += (EventHandler<ChannelDataEventArgs>) ((sender, e) => input.Write(e.Data, 0, e.Data.Length));
          channelSession.Open();
          if (!channelSession.SendExecRequest(string.Format("scp -t {0}", (object) this._remotePathTransformation.Transform(path))))
            throw ScpClient.SecureExecutionRequestRejectedException();
          this.CheckReturnCode((Stream) input);
          using (FileStream source = fileInfo.OpenRead())
          {
            this.UploadTimes(channelSession, (Stream) input, (FileSystemInfo) fileInfo);
            this.UploadFileModeAndName(channelSession, (Stream) input, source.Length, string.Empty);
            this.UploadFileContent(channelSession, (Stream) input, (Stream) source, fileInfo.Name);
          }
        }
      }
    }

    public void Upload(DirectoryInfo directoryInfo, string path)
    {
      if (directoryInfo == null)
        throw new ArgumentNullException(nameof (directoryInfo));
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException(nameof (path));
      using (PipeStream input = this.ServiceFactory.CreatePipeStream())
      {
        using (IChannelSession channelSession = this.Session.CreateChannelSession())
        {
          channelSession.DataReceived += (EventHandler<ChannelDataEventArgs>) ((sender, e) => input.Write(e.Data, 0, e.Data.Length));
          channelSession.Open();
          if (!channelSession.SendExecRequest(string.Format("scp -rt {0}", (object) this._remotePathTransformation.Transform(path))))
            throw ScpClient.SecureExecutionRequestRejectedException();
          this.CheckReturnCode((Stream) input);
          this.UploadTimes(channelSession, (Stream) input, (FileSystemInfo) directoryInfo);
          this.UploadDirectoryModeAndName(channelSession, (Stream) input, ".");
          this.UploadDirectoryContent(channelSession, (Stream) input, directoryInfo);
        }
      }
    }

    public void Download(string filename, FileInfo fileInfo)
    {
      if (string.IsNullOrEmpty(filename))
        throw new ArgumentException(nameof (filename));
      if (fileInfo == null)
        throw new ArgumentNullException(nameof (fileInfo));
      using (PipeStream input = this.ServiceFactory.CreatePipeStream())
      {
        using (IChannelSession channelSession = this.Session.CreateChannelSession())
        {
          channelSession.DataReceived += (EventHandler<ChannelDataEventArgs>) ((sender, e) => input.Write(e.Data, 0, e.Data.Length));
          channelSession.Open();
          if (!channelSession.SendExecRequest(string.Format("scp -pf {0}", (object) this._remotePathTransformation.Transform(filename))))
            throw ScpClient.SecureExecutionRequestRejectedException();
          ScpClient.SendSuccessConfirmation((IChannel) channelSession);
          this.InternalDownload(channelSession, (Stream) input, (FileSystemInfo) fileInfo);
        }
      }
    }

    public void Download(string directoryName, DirectoryInfo directoryInfo)
    {
      if (string.IsNullOrEmpty(directoryName))
        throw new ArgumentException(nameof (directoryName));
      if (directoryInfo == null)
        throw new ArgumentNullException(nameof (directoryInfo));
      using (PipeStream input = this.ServiceFactory.CreatePipeStream())
      {
        using (IChannelSession channelSession = this.Session.CreateChannelSession())
        {
          channelSession.DataReceived += (EventHandler<ChannelDataEventArgs>) ((sender, e) => input.Write(e.Data, 0, e.Data.Length));
          channelSession.Open();
          if (!channelSession.SendExecRequest(string.Format("scp -prf {0}", (object) this._remotePathTransformation.Transform(directoryName))))
            throw ScpClient.SecureExecutionRequestRejectedException();
          ScpClient.SendSuccessConfirmation((IChannel) channelSession);
          this.InternalDownload(channelSession, (Stream) input, (FileSystemInfo) directoryInfo);
        }
      }
    }

    private void UploadTimes(IChannelSession channel, Stream input, FileSystemInfo fileOrDirectory)
    {
      DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      long totalSeconds1 = (long) (fileOrDirectory.LastWriteTimeUtc - dateTime).TotalSeconds;
      long totalSeconds2 = (long) (fileOrDirectory.LastAccessTimeUtc - dateTime).TotalSeconds;
      this.SendData((IChannel) channel, string.Format("T{0} 0 {1} 0\n", (object) totalSeconds1, (object) totalSeconds2));
      this.CheckReturnCode(input);
    }

    private void UploadDirectoryContent(
      IChannelSession channel,
      Stream input,
      DirectoryInfo directoryInfo)
    {
      foreach (FileInfo file in directoryInfo.GetFiles())
      {
        using (FileStream source = file.OpenRead())
        {
          this.UploadTimes(channel, input, (FileSystemInfo) file);
          this.UploadFileModeAndName(channel, input, source.Length, file.Name);
          this.UploadFileContent(channel, input, (Stream) source, file.Name);
        }
      }
      foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
      {
        this.UploadTimes(channel, input, (FileSystemInfo) directory);
        this.UploadDirectoryModeAndName(channel, input, directory.Name);
        this.UploadDirectoryContent(channel, input, directory);
      }
      this.SendData((IChannel) channel, "E\n");
      this.CheckReturnCode(input);
    }

    private void UploadDirectoryModeAndName(
      IChannelSession channel,
      Stream input,
      string directoryName)
    {
      this.SendData((IChannel) channel, string.Format("D0755 0 {0}\n", (object) directoryName));
      this.CheckReturnCode(input);
    }

    private void InternalDownload(
      IChannelSession channel,
      Stream input,
      FileSystemInfo fileSystemInfo)
    {
      DateTime dateTime1 = DateTime.Now;
      DateTime dateTime2 = DateTime.Now;
      string fullName = fileSystemInfo.FullName;
      int num1 = 0;
      while (true)
      {
        string input1;
        Match match1;
        do
        {
          do
          {
            input1 = this.ReadString(input);
            if (!(input1 == "E"))
            {
              match1 = ScpClient.DirectoryInfoRe.Match(input1);
              if (!match1.Success)
              {
                Match match2 = ScpClient.FileInfoRe.Match(input1);
                if (match2.Success)
                {
                  ScpClient.SendSuccessConfirmation((IChannel) channel);
                  long length = long.Parse(match2.Result("${length}"));
                  string str = match2.Result("${filename}");
                  if (!(fileSystemInfo is FileInfo fileInfo))
                    fileInfo = new FileInfo(Path.Combine(fullName, str));
                  using (FileStream output = fileInfo.OpenWrite())
                    this.InternalDownload((IChannel) channel, input, (Stream) output, str, length);
                  fileInfo.LastAccessTime = dateTime2;
                  fileInfo.LastWriteTime = dateTime1;
                }
                else
                  goto label_18;
              }
              else
                goto label_3;
            }
            else
              goto label_1;
          }
          while (num1 != 0);
          goto label_16;
label_1:
          ScpClient.SendSuccessConfirmation((IChannel) channel);
          --num1;
          fullName = new DirectoryInfo(fullName).Parent.FullName;
        }
        while (num1 != 0);
        break;
label_3:
        ScpClient.SendSuccessConfirmation((IChannel) channel);
        string path2 = match1.Result("${filename}");
        DirectoryInfo directoryInfo;
        if (num1 > 0)
        {
          directoryInfo = Directory.CreateDirectory(Path.Combine(fullName, path2));
          directoryInfo.LastAccessTime = dateTime2;
          directoryInfo.LastWriteTime = dateTime1;
        }
        else
          directoryInfo = fileSystemInfo as DirectoryInfo;
        ++num1;
        fullName = directoryInfo.FullName;
        continue;
label_18:
        Match match3 = ScpClient.TimestampRe.Match(input1);
        if (match3.Success)
        {
          ScpClient.SendSuccessConfirmation((IChannel) channel);
          long num2 = long.Parse(match3.Result("${mtime}"));
          long num3 = long.Parse(match3.Result("${atime}"));
          DateTime dateTime3 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
          dateTime1 = dateTime3.AddSeconds((double) num2);
          dateTime2 = dateTime3.AddSeconds((double) num3);
        }
        else
          this.SendErrorConfirmation((IChannel) channel, string.Format("\"{0}\" is not valid protocol message.", (object) input1));
      }
      return;
label_16:;
    }
  }
}
