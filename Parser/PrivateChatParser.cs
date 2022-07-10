using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace stanworld
{
    using network;
    using network.data;
    using PrivateChatModel;
    using System.Linq;
    using packets;

    public class PrivateChatParser
    {
        private ParsingManager Manager;

        public void Init()
        {
            Manager = new ParsingManager();
            InitSTMParser();
            InitMTVParser();
            InitVTSParser();
        }

        public void Release()
        {
            ReleaseSTMParser();
            ReleaseMTVParser();
            ReleaseVTSParser();
            Manager = null;
        }

        /// <summary>
        /// 데이터를 파싱하는 메소드를 호출하는 메소드
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key">파싱하려는 데이터를 받아온 패킷 이름</param>
        /// <param name="data">파싱하려는 데이터</param>
        /// <param name="helper">파싱하는데 필요한 데이터(필요없는 경우도 있음. 주로 View 출력하는데에 필요)</param>
        /// <returns>파싱된 데이터를 반환받아서 타입 캐스팅해서 사용하면 됨. null 체크도 필요</returns>
        public IParseOutputData Parse(ParserType type, string key, IParseInputData data, IParseHelper helper = null)
        {
            return Manager.Parse(type, key, data, helper);
        }

        #region Sfs to Model
        private void InitSTMParser()
        {
            SfsToModelParser parser = new SfsToModelParser();
            parser.InitParsingDelegateContainer();
            parser.AddDelegate(PACKET_PRIVATECHAT_EXISTING_CHATS.PacketName, STM_PACKET_PRIVATECHAT_EXISTING_CHATS);
            parser.AddDelegate(PACKET_PRIVATECHAT_MESSAGES.PacketName, STM_PACKET_PRIVATECHAT_MESSAGES);

            Manager.Add(ParserType.SFStoMODEL, parser);
        }

        private void ReleaseSTMParser()
        {
            Manager.Remove(ParserType.SFStoMODEL);
        }

        private IParseOutputData STM_PACKET_PRIVATECHAT_EXISTING_CHATS(IParseInputData data, IParseHelper helper = null)
        {
            PACKET_PRIVATECHAT_EXISTING_CHATS.PrivateChats privateChatData = data as PACKET_PRIVATECHAT_EXISTING_CHATS.PrivateChats;
            PrivateChatContainer privateChatContainer = new PrivateChatContainer();
            foreach (var chat in privateChatData.privateChatList)
            {
                PrivateChat privateChat = new PrivateChat();
                privateChat.SetTargetUserId(chat.friendUserId);
                privateChat.SetFirstReadableMessageId(chat.firstReadableMessageId);
                privateChat.SetLastReadMessageId(chat.lastReadMessageId);
                PrivateChatMessage lastMessage = new PrivateChatMessage();
                lastMessage.SetSenderUserId(chat.lastMessage.senderUserId);
                lastMessage.SetMessageTimeStamp(chat.lastMessage.timestamp);
                lastMessage.SetMessageId(chat.lastMessage.messageId);
                lastMessage.SetMessageType(chat.lastMessage.messageType == null ? eMsgType.None : (eMsgType)chat.lastMessage.messageType);
                lastMessage.SetMessageContent(chat.lastMessage.messageContent);
                lastMessage.SetIsHidden(chat.lastMessage.isHidden);
                lastMessage.SetReplyMessageId(chat.lastMessage.replyMessageId == null ? -1 : (long)chat.lastMessage.replyMessageId);
                lastMessage.ParseMessageContent(lastMessage.messageType);
                privateChat.SetLastMessage(lastMessage);
                privateChatContainer.AddPrivateChat(privateChat);
            }
            return privateChatContainer;
        }

        private IParseOutputData STM_PACKET_PRIVATECHAT_MESSAGES(IParseInputData data, IParseHelper helper = null)
        {
            PACKET_PRIVATECHAT_MESSAGES.ChatMessageList messageList = data as PACKET_PRIVATECHAT_MESSAGES.ChatMessageList;
            PrivateChatMessageContainer messageContainer = new PrivateChatMessageContainer();
            PrivateChatData CurrentChatData = PartyMaister.PartyManagers.PrivateChat.GetCurrentChatData();
            foreach (PACKET_PRIVATECHAT_MESSAGES.ChatMessageList.Message chatMessage in messageList.chatMessages)
            {
                PrivateChatMessage message = new PrivateChatMessage();
                message.SetSenderUserId(chatMessage.senderUserId);
                message.SetMessageTimeStamp(chatMessage.timestamp);
                message.SetMessageId(chatMessage.messageId);
                message.SetSenderInfo(chatMessage.senderUserId == GameInstance.Instance.MIDX ? StannerManager.Instance.GetMyStannerInfo() : CurrentChatData.targetUserInfo);
                message.SetMessageType(chatMessage.messageType == null ? eMsgType.None : (eMsgType)chatMessage.messageType);
                message.SetMessageContent(chatMessage.messageContent);
                message.SetIsHidden(chatMessage.isHidden);
                message.SetReplyMessageId(chatMessage.replyMessageId == null ? -1 : (long)chatMessage.replyMessageId);
                message.ParseMessageContent(message.messageType);
                messageContainer.AddMessage(message);
            }
            return messageContainer;
        }
        #endregion

        #region Model to View
        private void InitMTVParser()
        {
            ModelToViewParser parser = new ModelToViewParser();
            parser.InitParsingDelegateContainer();
            parser.AddDelegate(PACKET_PRIVATECHAT_MESSAGES.PacketName, MTV_PACKET_PRIVATECHAT_MESSAGES);

            Manager.Add(ParserType.MODELtoVIEW, parser);
        }

        private void ReleaseMTVParser()
        {
            Manager.Remove(ParserType.MODELtoVIEW);
        }
        
        private IParseOutputData MTV_PACKET_PRIVATECHAT_MESSAGES(IParseInputData data, IParseHelper helper = null)
        {
            if (helper == null) return null;

            PrivateChatMessage message = data as PrivateChatMessage;
            UIPrivateChatPopup.ParseHelper parseHelper = helper as UIPrivateChatPopup.ParseHelper;
            UIPrivateChatPopup.ViewDataHolder dataHolder = new UIPrivateChatPopup.ViewDataHolder();
            SuperScrollView.LoopListViewItem2 item;

            if (message == null) return null;

            bool isSameSender = true;
            bool isSameTimeStamp = true;
            bool isTimeUseSamePerson = true;
            int prevIndex = parseHelper.messageIndex - 2;
            int nextIndex = parseHelper.messageIndex;

            PrivateChatModel.PrivateChatData currentChatData = PartyMaister.PartyManagers.PrivateChat.CurrentChatData;
            PrivateChatModel.PrivateChatMessage previousMessage = currentChatData.ContentAt(prevIndex);
            PrivateChatModel.PrivateChatMessage nextMessage = currentChatData.ContentAt(nextIndex);

            if (previousMessage != null)
            {
                isSameSender = (message.senderUserId == previousMessage.senderUserId);
            }
            else
            {
                isSameSender = false;
            }

            if (nextMessage != null)    // 마지막 메세지인 경우
            {
                isSameTimeStamp = (message.messageTimeStamp == nextMessage.messageTimeStamp);
                isTimeUseSamePerson = (message.senderUserId == nextMessage.senderUserId);
            }

            message.SetIsSameSender(isSameSender);
            message.SetIsLastMessage(nextMessage == null);
            message.SetIsSameTimestamp(isSameTimeStamp);
            message.SetIsTimeUseSamePerson(isTimeUseSamePerson);

            switch (message.messageType)
            {
                case eMsgType.Text:
                    {
                        if (message.senderUserId == GameInstance.Instance.MIDX)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemTextMeNoName");
                            ChatMsgItemTextNoName itemScript = item.GetComponent<ChatMsgItemTextNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else if (message.isSameSender)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemTextNoName");
                            ChatMsgItemTextNoName itemScript = item.GetComponent<ChatMsgItemTextNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemText");
                            ChatMsgItemText itemScript = item.GetComponent<ChatMsgItemText>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                    }
                    break;
                case eMsgType.URL:
                    {
                        if (message.senderUserId == GameInstance.Instance.MIDX)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemURLMeNoName");
                            ChatMsgItemURLNoName itemScript = item.GetComponent<ChatMsgItemURLNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else if (message.isSameSender)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemURLNoName");
                            ChatMsgItemURLNoName itemScript = item.GetComponent<ChatMsgItemURLNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemURL");
                            ChatMsgItemURL itemScript = item.GetComponent<ChatMsgItemURL>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                    }
                    break;
                case eMsgType.Picture:
                    {
                        if (message.senderUserId == GameInstance.Instance.MIDX)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemPictureMeNoName");
                            ChatMsgItemPictureNoName itemScript = item.GetComponent<ChatMsgItemPictureNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else if (message.isSameSender)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemPictureNoName");
                            ChatMsgItemPictureNoName itemScript = item.GetComponent<ChatMsgItemPictureNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemPicture");
                            ChatMsgItemPicture itemScript = item.GetComponent<ChatMsgItemPicture>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                    }
                    break;
                case eMsgType.Emoticon:
                    {
                        if (message.senderUserId == GameInstance.Instance.MIDX)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemEmoticonMeNoName");
                            ChatMsgItemEmoticonNoName itemScript = item.GetComponent<ChatMsgItemEmoticonNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else if (message.isSameSender)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemEmoticonNoName");
                            ChatMsgItemEmoticonNoName itemScript = item.GetComponent<ChatMsgItemEmoticonNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemEmoticon");
                            ChatMsgItemEmoticon itemScript = item.GetComponent<ChatMsgItemEmoticon>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                    }
                    break;
                case eMsgType.EmoticonWithText:
                    {
                        if (message.senderUserId == GameInstance.Instance.MIDX)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemEmoticonWithTextMeNoName");
                            ChatMsgItemEmoticonWithTextNoName itemScript = item.GetComponent<ChatMsgItemEmoticonWithTextNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else if (message.isSameSender)
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemEmoticonWithTextNoName");
                            ChatMsgItemEmoticonWithTextNoName itemScript = item.GetComponent<ChatMsgItemEmoticonWithTextNoName>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                        else
                        {
                            item = parseHelper.listView.NewListViewItem("ChatMsgItemEmoticonWithText");
                            ChatMsgItemEmoticonWithText itemScript = item.GetComponent<ChatMsgItemEmoticonWithText>();
                            itemScript.SetItemData(message, parseHelper.messageIndex);
                        }
                    }
                    break;
                default:
                    {
                        item = null;
                    }
                    break;
            }

            dataHolder.item = item;
            return dataHolder;
        }
        #endregion

        #region View to Sfs
        private void InitVTSParser()
        {
            ViewToSfsParser parser = new ViewToSfsParser();
            parser.InitParsingDelegateContainer();
            parser.AddDelegate(PACKET_PRIVATECHAT_NEW_MESSAGE.PacketName, VTS_PACKET_PRIVATECHAT_NEW_MESSAGE);

            Manager.Add(ParserType.VIEWtoSFS, parser);
        }

        private void ReleaseVTSParser()
        {
            Manager.Remove(ParserType.VIEWtoSFS);
        }

        private IParseOutputData VTS_PACKET_PRIVATECHAT_NEW_MESSAGE(IParseInputData data, IParseHelper helper = null)
        {
            PrivateChatInputData inputData = data as PrivateChatInputData;
            long targetUserId = inputData.targetUserId;
            long replyMessageId = inputData.replyMessageId;
            eMsgType messageType = eMsgType.None;
            JSONNode jsonObject = new JSONObject();
            if (inputData.emoNum != -1)
            {
                if (string.IsNullOrEmpty(inputData.messageContent))
                {
                    messageType = eMsgType.Emoticon;
                    jsonObject["emo"] = inputData.emoNum;
                }
                else
                {
                    messageType = eMsgType.EmoticonWithText;
                    jsonObject["emo"] = inputData.emoNum;
                    jsonObject["text"] = inputData.messageContent;
                }
            }
            else
            {
                string url = GameInstance.Instance.GetChatMsgURL(inputData.messageContent);
                if (string.IsNullOrEmpty(url))
                {
                    messageType = (int)eMsgType.Text;
                    jsonObject["text"] = inputData.messageContent;
                }
                else
                {
                    messageType = eMsgType.URL;
                    jsonObject["text"] = inputData.messageContent;
                }
            }
            string messageContent = jsonObject.ToString();

            PrivateChatMessage message = new PrivateChatMessage(targetUserId, replyMessageId, messageContent, messageType);
            return message;
        }
        #endregion
    }
}
