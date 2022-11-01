// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.Abstractions.SocketAbstraction
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Renci.SshNet.Abstractions
{
  internal static class SocketAbstraction
  {
    public static bool CanRead(Socket socket) => socket.Connected && socket.Poll(-1, SelectMode.SelectRead) && socket.Available > 0;

    public static bool CanWrite(Socket socket) => socket != null && socket.Connected && socket.Poll(-1, SelectMode.SelectWrite);

    public static Socket Connect(IPEndPoint remoteEndpoint, TimeSpan connectTimeout)
    {
      Socket socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
      {
        NoDelay = true
      };
      ManualResetEvent manualResetEvent = new ManualResetEvent(false);
      SocketAsyncEventArgs e = new SocketAsyncEventArgs()
      {
        UserToken = (object) manualResetEvent,
        RemoteEndPoint = (EndPoint) remoteEndpoint
      };
      e.Completed += new EventHandler<SocketAsyncEventArgs>(SocketAbstraction.ConnectCompleted);
      if (socket.ConnectAsync(e) && !manualResetEvent.WaitOne(connectTimeout))
      {
        e.Completed -= new EventHandler<SocketAsyncEventArgs>(SocketAbstraction.ConnectCompleted);
        socket.Dispose();
        manualResetEvent.Dispose();
        e.Dispose();
        throw new SshOperationTimeoutException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Connection failed to establish within {0:F0} milliseconds.", (object) connectTimeout.TotalMilliseconds));
      }
      manualResetEvent.Dispose();
      if (e.SocketError != 0)
      {
        int socketError = (int) e.SocketError;
        socket.Dispose();
        e.Dispose();
        throw new SocketException(socketError);
      }
      e.Dispose();
      return socket;
    }

    public static void ClearReadBuffer(Socket socket)
    {
      TimeSpan timeout = TimeSpan.FromMilliseconds(500.0);
      byte[] buffer = new byte[256];
      do
        ;
      while (SocketAbstraction.ReadPartial(socket, buffer, 0, buffer.Length, timeout) > 0);
    }

    public static int ReadPartial(
      Socket socket,
      byte[] buffer,
      int offset,
      int size,
      TimeSpan timeout)
    {
      socket.ReceiveTimeout = (int) timeout.TotalMilliseconds;
      try
      {
        return socket.Receive(buffer, offset, size, SocketFlags.None);
      }
      catch (SocketException ex)
      {
        if (ex.SocketErrorCode == SocketError.TimedOut)
          throw new SshOperationTimeoutException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Socket read operation has timed out after {0:F0} milliseconds.", (object) timeout.TotalMilliseconds));
        throw;
      }
    }

    public static void ReadContinuous(
      Socket socket,
      byte[] buffer,
      int offset,
      int size,
      Action<byte[], int, int> processReceivedBytesAction)
    {
      socket.ReceiveTimeout = 0;
      while (socket.Connected)
      {
        try
        {
          int num = socket.Receive(buffer, offset, size, SocketFlags.None);
          if (num == 0)
            break;
          processReceivedBytesAction(buffer, offset, num);
        }
        catch (SocketException ex)
        {
          if (!SocketAbstraction.IsErrorResumable(ex.SocketErrorCode))
          {
            switch (ex.SocketErrorCode)
            {
              case SocketError.Interrupted:
                return;
              case SocketError.ConnectionAborted:
              case SocketError.ConnectionReset:
                return;
              default:
                throw;
            }
          }
        }
      }
    }

    public static int ReadByte(Socket socket, TimeSpan timeout)
    {
      byte[] buffer = new byte[1];
      return SocketAbstraction.Read(socket, buffer, 0, 1, timeout) == 0 ? -1 : (int) buffer[0];
    }

    public static void SendByte(Socket socket, byte value)
    {
      byte[] data = new byte[1]{ value };
      SocketAbstraction.Send(socket, data, 0, 1);
    }

    public static byte[] Read(Socket socket, int size, TimeSpan timeout)
    {
      byte[] buffer = new byte[size];
      SocketAbstraction.Read(socket, buffer, 0, size, timeout);
      return buffer;
    }

    public static int Read(Socket socket, byte[] buffer, int offset, int size, TimeSpan timeout)
    {
      int num1 = 0;
      int num2 = size;
      socket.ReceiveTimeout = (int) timeout.TotalMilliseconds;
      do
      {
        try
        {
          int num3 = socket.Receive(buffer, offset + num1, num2 - num1, SocketFlags.None);
          if (num3 == 0)
            return 0;
          num1 += num3;
        }
        catch (SocketException ex)
        {
          if (SocketAbstraction.IsErrorResumable(ex.SocketErrorCode))
          {
            ThreadAbstraction.Sleep(30);
          }
          else
          {
            if (ex.SocketErrorCode == SocketError.TimedOut)
              throw new SshOperationTimeoutException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Socket read operation has timed out after {0:F0} milliseconds.", (object) timeout.TotalMilliseconds));
            throw;
          }
        }
      }
      while (num1 < num2);
      return num1;
    }

    public static void Send(Socket socket, byte[] data) => SocketAbstraction.Send(socket, data, 0, data.Length);

    public static void Send(Socket socket, byte[] data, int offset, int size)
    {
      int num1 = 0;
      int num2 = size;
      do
      {
        try
        {
          int num3 = socket.Send(data, offset + num1, num2 - num1, SocketFlags.None);
          if (num3 == 0)
            throw new SshConnectionException("An established connection was aborted by the server.", DisconnectReason.ConnectionLost);
          num1 += num3;
        }
        catch (SocketException ex)
        {
          if (SocketAbstraction.IsErrorResumable(ex.SocketErrorCode))
            ThreadAbstraction.Sleep(30);
          else
            throw;
        }
      }
      while (num1 < num2);
    }

    public static bool IsErrorResumable(SocketError socketError)
    {
      switch (socketError)
      {
        case SocketError.IOPending:
        case SocketError.WouldBlock:
        case SocketError.NoBufferSpaceAvailable:
          return true;
        default:
          return false;
      }
    }

    private static void ConnectCompleted(object sender, SocketAsyncEventArgs e) => ((EventWaitHandle) e.UserToken)?.Set();
  }
}
