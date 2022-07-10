using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace stanworld
{
    namespace PrivateChatModel
    {
        using SimpleJSON;
        using StannerModel;

        public class PrivateChatInputData : IViewData
        {
            public long targetUserId { get; private set; }
            public int emoNum { get; private set; }
            public string messageContent { get; private set; }
            public long replyMessageId { get; private set; }

            public PrivateChatInputData(long id, int emo, string message, long replyId)
            {
                targetUserId = id;
                emoNum = emo;
                messageContent = message;
                replyMessageId = replyId;
            }
        }

        // for privatechat renewal ( ChatMsg와 다르게 구조를 짜 봄. 추후에 뭐가 더 편한지에 따라 데이터 구조는 변경할 생각입니당.)
        public class PrivateChatMessageContent
        {
            public string jsonContent { get; private set; }
            public string messageText { get; private set; }
            public int messageEmo { get; private set; }
            public string messageImagePath { get; private set; }
            public string ogTitle { get; private set; }
            public string ogDesc { get; private set; }
            public string ogImage { get; private set; }
            public string ogUrl { get; private set; }
            public bool bCheckURL { get; private set; }
            public bool isAvailableURL { get; private set; }

            public PrivateChatMessageContent() { }

            public PrivateChatMessageContent(string json)
            {
                jsonContent = json;
            }

            public void ParseJson(eMsgType msgType)
            {
                JSONNode json = JSON.Parse(jsonContent);
                switch (msgType)
                {
                    case eMsgType.Text:
                        {
                            messageText = json["text"];
                        }
                        break;
                    case eMsgType.Emoticon:
                        {
                            messageEmo = json["emo"];
                        }
                        break;
                    case eMsgType.EmoticonWithText:
                        {
                            messageText = json["text"];
                            messageEmo = json["emo"];
                        }
                        break;
                    case eMsgType.Picture:
                        {
                            messageImagePath = json["pic"];
                        }
                        break;
                    case eMsgType.URL:
                        {
                            messageText = GameInstance.Instance.GetChatMsgURL(json["text"]);
                        }
                        break;
                    default: break;
                }
            }

            public void SetJsonContent(string json)
            {
                jsonContent = json;
            }

            public void SetOgTitle(string title)
            {
                ogTitle = title;
            }

            public void SetOgDesc(string desc)
            {
                ogDesc = desc;
            }

            public void SetOgImage(string image)
            {
                ogImage = image;
            }

            public void SetOgUrl(string url)
            {
                ogUrl = url;
            }

            public void SetbCheckURL(bool state)
            {
                bCheckURL = state;
            }

            public void SetIsAvailableURL(bool state)
            {
                isAvailableURL = state;
            }
        }

        public class PrivateChatMessage : IModelData
        {
            public long senderUserId { get; private set; }
            public Stanner senderInfo { get; private set; }
            public long messageId { get; private set; }
            public eMsgType messageType { get; private set; }
            public PrivateChatMessageContent messageContent { get; private set; }
            public bool isHidden { get; private set; }
            public long replyMessageId { get; private set; }
            public long messageTimeStamp { get; private set; }
            public bool isSameSender { get; private set; }
            public bool isLastMessage { get; private set; }
            public bool isSameTimestamp { get; private set; }
            public bool isTimeUseSamePerson { get; private set; }

            public PrivateChatMessage() { }

            public PrivateChatMessage(long id, long replyId, string content, eMsgType type)
            {
                messageId = id;
                replyMessageId = replyId;
                messageContent = new PrivateChatMessageContent(content);
                messageType = type;
            }

            public void SetSenderUserId(long id)
            {
                senderUserId = id;
            }

            public void SetSenderInfo(Stanner info)
            {
                senderInfo = info;
            }

            public void SetMessageId(long id)
            {
                messageId = id;
            }

            public void SetMessageType(int type)
            {
                messageType = (eMsgType)type;
            }

            public void SetMessageType(eMsgType type)
            {
                messageType = type;
            }

            public void SetMessageContent(string content)
            {
                messageContent = new PrivateChatMessageContent(content);
            }

            public void SetMessageContent(PrivateChatMessageContent content)
            {
                messageContent = content;
            }

            public void ParseMessageContent(eMsgType msgType)
            {
                messageContent.ParseJson(msgType);
            }

            public void SetIsHidden(bool hidden)
            {
                isHidden = hidden;
            }

            public void SetReplyMessageId(long id)
            {
                replyMessageId = id;
            }

            public void SetMessageTimeStamp(long timeStamp)
            {
                messageTimeStamp = timeStamp;
            }

            public void SetIsSameSender(bool state)
            {
                isSameSender = state;
            }

            public void SetIsLastMessage(bool state)
            {
                isLastMessage = state;
            }

            public void SetIsSameTimestamp(bool state)
            {
                isSameTimestamp = state;
            }

            public void SetIsTimeUseSamePerson(bool state)
            {
                isTimeUseSamePerson = state;
            }

            public void SetMessageData(PrivateChatMessage message)
            {
                messageId = message.messageId;
                messageType = message.messageType;
                messageContent = message.messageContent;
                isHidden = message.isHidden;
                replyMessageId = message.replyMessageId;
            }
        }

        // for privatechat renewal
        public class PrivateChatMessageContainer : IModelData
        {
            public int Count { get { return MessageList.Count; } }
            public List<PrivateChatMessage> MessageList { get; private set; }
            public long LastMessageId { get; private set; }

            public PrivateChatMessageContainer()
            {
                MessageList = new List<PrivateChatMessage>();
            }

            public void SetLastMessageId(long id)
            {
                LastMessageId = id;
            }

            public void AddMessage(PrivateChatMessage message)
            {
                MessageList.Add(message);
                LastMessageId = message.messageId;
            }

            public void ClearMessageList()
            {
                if (MessageList != null)
                {
                    MessageList.Clear();
                }
            }

            public PrivateChatMessage ContentAt(int index)
            {
                if (Count > index && index >= 0)
                {
                    return MessageList[index];
                }
                return null;
            }

            public bool SearchMessageByMessageId(long id)
            {
                foreach (PrivateChatMessage message in MessageList)
                {
                    if (message.messageId == id)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void UpdateMessageData(PrivateChatMessage message)
            {
                if (message.messageId > LastMessageId)
                {
                    MessageList.Add(message);
                    LastMessageId = message.messageId;
                }
                else
                {
                    foreach (PrivateChatMessage data in MessageList)
                    {
                        if (data.messageId == message.messageId)
                        {
                            data.SetMessageData(message);
                            return;
                        }
                    }
                }
            }

            public PrivateChatMessage GetLastMessage()
            {
                if (Count == 0)
                {
                    return null;
                }

                return MessageList[Count - 1];
            }
        }

        public class PrivateChat : IModelData
        {
            public long targetUserId { get; private set; }
            public long firstReadableMessageId { get; private set; }
            public long lastReadMessageId { get; private set; }
            public PrivateChatMessage lastMessage { get; private set; }
            public Stanner targetUserInfo { get; private set; }

            public PrivateChat() { }

            public void SetTargetUserId(long id)
            {
                targetUserId = id;
            }

            public void SetFirstReadableMessageId(long id)
            {
                firstReadableMessageId = id;
            }

            public void SetLastReadMessageId(long? id)
            {
                lastReadMessageId = id == null ? -1 : (long)id; 
            }

            public void SetLastMessage(PrivateChatMessage message)
            {
                lastMessage = message;
            }

            public void SetTargetUserInfo(Stanner info)
            {
                targetUserInfo = info;
            }
        }

        public class PrivateChatContainer : IModelData
        {
            public int Count { get { return PrivateChatList.Count; } }
            public List<PrivateChat> PrivateChatList { get; private set; }

            public PrivateChatContainer()
            {
                PrivateChatList = new List<PrivateChat>();
            }

            public List<long> GetUserIdList()
            {
                List<long> result = new List<long>();
                foreach (PrivateChat chat in PrivateChatList)
                {
                    result.Add(chat.targetUserId);
                }
                return result;
            }

            public void AddPrivateChat(PrivateChat chat)
            {
                PrivateChatList.Add(chat);
            }

            public void RemovePrivateChat(long userId)
            {
                foreach (PrivateChat privateChat in PrivateChatList)
                {
                    if (privateChat.targetUserId == userId)
                    {
                        PrivateChatList.Remove(privateChat);
                        return;
                    }
                }
            }

            public PrivateChat SearchPrivateChatByUserId(long userId)
            {
                foreach (PrivateChat chat in PrivateChatList)
                {
                    if (chat.targetUserId == userId)
                    {
                        return chat;
                    }
                }
                return null;
            }

            public void ClearPrivateChat()
            {
                PrivateChatList.Clear();
            }

            public void SortWithLastMessageTimeStamp()
            {
                PrivateChatList.Sort((a, b) => a.lastMessage.messageTimeStamp.CompareTo(b.lastMessage.messageTimeStamp));
            }

            public void SetAdditionalStannerData(StannerContainer stanners)
            {
                foreach (PrivateChat chat in PrivateChatList)
                {
                    long userId = chat.targetUserId;
                    Stanner stanner = stanners.SearchStannerByUserId(userId);
                    if (stanner == null) continue;

                    chat.SetTargetUserInfo(stanner);
                }
            }

            public PrivateChat ContentAt(int index)
            {
                if (Count > index && index >= 0)
                {
                    return PrivateChatList[index];
                }
                return null;
            }

            public void UpdateLastMessage(long targetUserId, PrivateChatMessage message)
            {
                foreach (PrivateChat chat in PrivateChatList)
                {
                    if (chat.targetUserId == targetUserId)
                    {
                        if (chat.lastMessage.messageId < message.messageId)
                        {
                            chat.SetLastMessage(message);
                        }
                        return;
                    }
                }

                // 기존에 채팅 기록이 없었던 경우, 새로운 챗 생성 후 리스트에 추가해야함.
                PrivateChat newChat = new PrivateChat();
                newChat.SetFirstReadableMessageId(0);
                newChat.SetLastReadMessageId(message.messageId);
                newChat.SetLastMessage(message);
                newChat.SetTargetUserId(targetUserId);
                newChat.SetTargetUserInfo(message.senderInfo);
                AddPrivateChat(newChat);
            }
        }

        // for privatechat renewal
        public class PrivateChatData : IModelData
        {
            public int MessagesCount
            {
                get { return MessageList.Count; }
            }
            public long targetUserId { get; private set; }
            public Stanner targetUserInfo { get; private set; }
            public PrivateChatMessageContainer MessageList { get; private set; }

            public PrivateChatData() { }

            public PrivateChatData(Stanner stanner)
            {
                targetUserId = stanner.userId;
                targetUserInfo = stanner;
                MessageList = new PrivateChatMessageContainer();
            }

            public PrivateChatMessage SearchMessageWithMessageId(long messageId)
            {
                foreach (PrivateChatMessage message in MessageList.MessageList)
                {
                    if (message.messageId == messageId)
                    {
                        return message;
                    }
                }
                return null;
            }

            public void ClearMessageList()
            {
                MessageList.ClearMessageList();
            }

            public PrivateChatMessage ContentAt(int index)
            {
                if (MessagesCount > index && index >= 0)
                {
                    return MessageList.ContentAt(index);
                }
                return null;
            }

            public void UpdateMessagesData(PrivateChatMessageContainer container)
            {
                foreach (PrivateChatMessage message in container.MessageList)
                {
                    if (message.senderUserId == targetUserId || message.senderUserId == GameInstance.Instance.MIDX)
                    {
                        MessageList.UpdateMessageData(message);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
}
