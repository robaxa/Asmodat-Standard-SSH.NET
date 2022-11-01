// Decompiled with JetBrains decompiler
// Type: Renci.SshNet.SshMessageFactory
// Assembly: Asmodat Standard SSH.NET, Version=1.0.5.1, Culture=neutral, PublicKeyToken=null
// MVID: 504BBE18-5FBE-4C0C-8018-79774B0EDD0B
// Assembly location: C:\Users\ebacron\AppData\Local\Temp\Kuzebat\89eb444bc2\lib\net5.0\Asmodat Standard SSH.NET.dll

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Renci.SshNet
{
  internal class SshMessageFactory
  {
    private readonly SshMessageFactory.MessageMetadata[] _enabledMessagesByNumber;
    private readonly bool[] _activatedMessagesById;
    internal static readonly SshMessageFactory.MessageMetadata[] AllMessages = new SshMessageFactory.MessageMetadata[31]
    {
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<KeyExchangeInitMessage>((byte) 0, "SSH_MSG_KEXINIT", (byte) 20),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<NewKeysMessage>((byte) 1, "SSH_MSG_NEWKEYS", (byte) 21),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<RequestFailureMessage>((byte) 2, "SSH_MSG_REQUEST_FAILURE", (byte) 82),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelOpenFailureMessage>((byte) 3, "SSH_MSG_CHANNEL_OPEN_FAILURE", (byte) 92),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelFailureMessage>((byte) 4, "SSH_MSG_CHANNEL_FAILURE", (byte) 100),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelExtendedDataMessage>((byte) 5, "SSH_MSG_CHANNEL_EXTENDED_DATA", (byte) 95),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelDataMessage>((byte) 6, "SSH_MSG_CHANNEL_DATA", (byte) 94),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelRequestMessage>((byte) 7, "SSH_MSG_CHANNEL_REQUEST", (byte) 98),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<BannerMessage>((byte) 8, "SSH_MSG_USERAUTH_BANNER", (byte) 53),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<InformationResponseMessage>((byte) 9, "SSH_MSG_USERAUTH_INFO_RESPONSE", (byte) 61),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<FailureMessage>((byte) 10, "SSH_MSG_USERAUTH_FAILURE", (byte) 51),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<DebugMessage>((byte) 11, "SSH_MSG_DEBUG", (byte) 4),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<GlobalRequestMessage>((byte) 12, "SSH_MSG_GLOBAL_REQUEST", (byte) 80),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelOpenMessage>((byte) 13, "SSH_MSG_CHANNEL_OPEN", (byte) 90),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelOpenConfirmationMessage>((byte) 14, "SSH_MSG_CHANNEL_OPEN_CONFIRMATION", (byte) 91),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<InformationRequestMessage>((byte) 15, "SSH_MSG_USERAUTH_INFO_REQUEST", (byte) 60),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<UnimplementedMessage>((byte) 16, "SSH_MSG_UNIMPLEMENTED", (byte) 3),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<RequestSuccessMessage>((byte) 17, "SSH_MSG_REQUEST_SUCCESS", (byte) 81),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelSuccessMessage>((byte) 18, "SSH_MSG_CHANNEL_SUCCESS", (byte) 99),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<PasswordChangeRequiredMessage>((byte) 19, "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", (byte) 60),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<DisconnectMessage>((byte) 20, "SSH_MSG_DISCONNECT", (byte) 1),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<SuccessMessage>((byte) 21, "SSH_MSG_USERAUTH_SUCCESS", (byte) 52),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<PublicKeyMessage>((byte) 22, "SSH_MSG_USERAUTH_PK_OK", (byte) 60),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<IgnoreMessage>((byte) 23, "SSH_MSG_IGNORE", (byte) 2),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelWindowAdjustMessage>((byte) 24, "SSH_MSG_CHANNEL_WINDOW_ADJUST", (byte) 93),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelEofMessage>((byte) 25, "SSH_MSG_CHANNEL_EOF", (byte) 96),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ChannelCloseMessage>((byte) 26, "SSH_MSG_CHANNEL_CLOSE", (byte) 97),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<ServiceAcceptMessage>((byte) 27, "SSH_MSG_SERVICE_ACCEPT", (byte) 6),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<KeyExchangeDhGroupExchangeGroup>((byte) 28, "SSH_MSG_KEX_DH_GEX_GROUP", (byte) 31),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<KeyExchangeDhReplyMessage>((byte) 29, "SSH_MSG_KEXDH_REPLY", (byte) 31),
      (SshMessageFactory.MessageMetadata) new SshMessageFactory.MessageMetadata<KeyExchangeDhGroupExchangeReply>((byte) 30, "SSH_MSG_KEX_DH_GEX_REPLY", (byte) 33)
    };
    private static readonly IDictionary<string, SshMessageFactory.MessageMetadata> MessagesByName = (IDictionary<string, SshMessageFactory.MessageMetadata>) new Dictionary<string, SshMessageFactory.MessageMetadata>(SshMessageFactory.AllMessages.Length);
    internal const byte HighestMessageNumber = 100;
    internal const int TotalMessageCount = 31;

    static SshMessageFactory()
    {
      for (int index = 0; index < SshMessageFactory.AllMessages.Length; ++index)
      {
        SshMessageFactory.MessageMetadata allMessage = SshMessageFactory.AllMessages[index];
        SshMessageFactory.MessagesByName.Add(allMessage.Name, allMessage);
      }
    }

    public SshMessageFactory()
    {
      this._activatedMessagesById = new bool[31];
      this._enabledMessagesByNumber = new SshMessageFactory.MessageMetadata[101];
    }

    public void Reset()
    {
      Array.Clear((Array) this._activatedMessagesById, 0, this._activatedMessagesById.Length);
      Array.Clear((Array) this._enabledMessagesByNumber, 0, this._enabledMessagesByNumber.Length);
    }

    public Message Create(byte messageNumber)
    {
      SshMessageFactory.MessageMetadata messageMetadata1 = messageNumber <= (byte) 100 ? this._enabledMessagesByNumber[(int) messageNumber] : throw SshMessageFactory.CreateMessageTypeNotSupportedException(messageNumber);
      if (messageMetadata1 != null)
        return messageMetadata1.Create();
      SshMessageFactory.MessageMetadata messageMetadata2 = (SshMessageFactory.MessageMetadata) null;
      for (int index = 0; index < SshMessageFactory.AllMessages.Length; ++index)
      {
        SshMessageFactory.MessageMetadata allMessage = SshMessageFactory.AllMessages[index];
        if ((int) allMessage.Number == (int) messageNumber)
        {
          messageMetadata2 = allMessage;
          break;
        }
      }
      if (messageMetadata2 == null)
        throw SshMessageFactory.CreateMessageTypeNotSupportedException(messageNumber);
      throw new SshException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Message type {0} is not valid in the current context.", (object) messageNumber));
    }

    public void DisableNonKeyExchangeMessages()
    {
      for (int index = 0; index < SshMessageFactory.AllMessages.Length; ++index)
      {
        byte number = SshMessageFactory.AllMessages[index].Number;
        if (number > (byte) 2 && number < (byte) 20 || number > (byte) 30)
          this._enabledMessagesByNumber[(int) number] = (SshMessageFactory.MessageMetadata) null;
      }
    }

    public void EnableActivatedMessages()
    {
      for (int index = 0; index < SshMessageFactory.AllMessages.Length; ++index)
      {
        SshMessageFactory.MessageMetadata allMessage = SshMessageFactory.AllMessages[index];
        if (this._activatedMessagesById[(int) allMessage.Id])
        {
          SshMessageFactory.MessageMetadata messageMetadata = this._enabledMessagesByNumber[(int) allMessage.Number];
          this._enabledMessagesByNumber[(int) allMessage.Number] = messageMetadata == null || messageMetadata == allMessage ? allMessage : throw SshMessageFactory.CreateMessageTypeAlreadyEnabledForOtherMessageException(allMessage.Number, allMessage.Name, messageMetadata.Name);
        }
      }
    }

    public void EnableAndActivateMessage(string messageName)
    {
      if (messageName == null)
        throw new ArgumentNullException(nameof (messageName));
      lock (this)
      {
        SshMessageFactory.MessageMetadata messageMetadata1;
        if (!SshMessageFactory.MessagesByName.TryGetValue(messageName, out messageMetadata1))
          throw SshMessageFactory.CreateMessageNotSupportedException(messageName);
        SshMessageFactory.MessageMetadata messageMetadata2 = this._enabledMessagesByNumber[(int) messageMetadata1.Number];
        this._enabledMessagesByNumber[(int) messageMetadata1.Number] = messageMetadata2 == null || messageMetadata2 == messageMetadata1 ? messageMetadata1 : throw SshMessageFactory.CreateMessageTypeAlreadyEnabledForOtherMessageException(messageMetadata1.Number, messageMetadata1.Name, messageMetadata2.Name);
        this._activatedMessagesById[(int) messageMetadata1.Id] = true;
      }
    }

    public void DisableAndDeactivateMessage(string messageName)
    {
      if (messageName == null)
        throw new ArgumentNullException(nameof (messageName));
      lock (this)
      {
        SshMessageFactory.MessageMetadata messageMetadata1;
        if (!SshMessageFactory.MessagesByName.TryGetValue(messageName, out messageMetadata1))
          throw SshMessageFactory.CreateMessageNotSupportedException(messageName);
        SshMessageFactory.MessageMetadata messageMetadata2 = this._enabledMessagesByNumber[(int) messageMetadata1.Number];
        if (messageMetadata2 != null && messageMetadata2 != messageMetadata1)
          throw SshMessageFactory.CreateMessageTypeAlreadyEnabledForOtherMessageException(messageMetadata1.Number, messageMetadata1.Name, messageMetadata2.Name);
        this._activatedMessagesById[(int) messageMetadata1.Id] = false;
        this._enabledMessagesByNumber[(int) messageMetadata1.Number] = (SshMessageFactory.MessageMetadata) null;
      }
    }

    private static SshException CreateMessageTypeNotSupportedException(
      byte messageNumber)
    {
      throw new SshException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Message type {0} is not supported.", (object) messageNumber));
    }

    private static SshException CreateMessageNotSupportedException(string messageName) => throw new SshException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Message '{0}' is not supported.", (object) messageName));

    private static SshException CreateMessageTypeAlreadyEnabledForOtherMessageException(
      byte messageNumber,
      string messageName,
      string currentEnabledForMessageName)
    {
      throw new SshException(string.Format((IFormatProvider) CultureInfo.InvariantCulture, "Cannot enable message '{0}'. Message type {1} is already enabled for '{2}'.", (object) messageName, (object) messageNumber, (object) currentEnabledForMessageName));
    }

    internal abstract class MessageMetadata
    {
      public readonly byte Id;
      public readonly string Name;
      public readonly byte Number;

      protected MessageMetadata(byte id, string name, byte number)
      {
        this.Id = id;
        this.Name = name;
        this.Number = number;
      }

      public abstract Message Create();
    }

    internal class MessageMetadata<T> : SshMessageFactory.MessageMetadata where T : Message, new()
    {
      public MessageMetadata(byte id, string name, byte number)
        : base(id, name, number)
      {
      }

      public override Message Create() => (Message) new T();
    }
  }
}
