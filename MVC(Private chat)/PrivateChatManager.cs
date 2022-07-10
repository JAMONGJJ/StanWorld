using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using network.data;
    using PrivateChatModel;
    using StannerModel;
    using packets;
    using network;

    public class PrivateChatManager
    {
        #region MODEL
        public PrivateChatData CurrentChatData { get; private set; } = new PrivateChatData();
        public PrivateChatContainer PrivateChats { get; private set; } = new PrivateChatContainer();    // UIPrivateChatListPopup에 표시될 데이터
        // will be deleted after privatechat renewal
        public PrivateChatListInfo PrivateChatListInfo { get; private set; } = new PrivateChatListInfo();
        public ChatPartyPhotoInfo PrivateChatPhotoInfo { get; set; } = new ChatPartyPhotoInfo();
        #endregion

        #region CONTROLLER
        public PrivateChatPopupController Popup { get; private set; }
        public PrivateChatParser Parser { get; private set; }
        #endregion

        #region VIEW
        public UIPrivateChatPopup CurrentChat { get; private set; }
        #endregion

        public void OpenPrivateChatControl()
        {
            LogFileWriter.Log("PRIVATECHAT MANAGER", LogFileWriter.LogType.CONTROLLER_INIT);
            InitPrivateChatPopupControl();
            InitPrivateChatParser();
        }

        public void ClosePrivateChatControl()
        {
            ReleasePrivateChatParser();
            ReleasePrivateChatPopupControl();
            LogFileWriter.Log("PRIVATECHAT MANAGER", LogFileWriter.LogType.CONTROLLER_RELEASE);
        }

        #region PrivateChats
        public PrivateChatContainer GetPrivateChats()
        {
            return PrivateChats;
        }

        public void SetPrivateChats(PrivateChatContainer container)
        {
            PrivateChats = container;
        }

        public void RemovePrivateChat(long userId)
        {
            if (Popup.UIPrivateChatListPopup != null)
            {
                PrivateChats.RemovePrivateChat(userId);
                Popup.UIPrivateChatListPopup.ReflushItem();
            }
        }

        private void UpdateLastMessage(PrivateChatMessage message)
        {
            if (Popup.UIPrivateChatListPopup != null)
            {
                // To do : UIPrivateChatListPopup이 OnTop이 됐을때, PrivateChats의 모든 PrivateChat의 LastMessage 갱신 필요.
                PrivateChats.UpdateLastMessage(CurrentChatData.targetUserId, message);
                PrivateChats.SortWithLastMessageTimeStamp();
                Popup.UIPrivateChatListPopup.ReflushItem();
            }
        }
        #endregion

        #region PrivateChat Popup Controller
        private void InitPrivateChatPopupControl()
        {
            if (Popup == null)
            {
                Popup = new PrivateChatPopupController();
                Popup.Init();
            }
        }

        private void ReleasePrivateChatPopupControl()
        {
            if (Popup != null)
            {
                Popup.Release();
                Popup = null;
            }
        }

        public void ShowPopup(ePopupType _type, UIData _data = null)
        {
            if (Popup != null)
            {
                Popup.ShowPopup(_type, _data);
            }
        }

        public void ReleasePopup(ePopupType _type)
        {
            if (Popup != null)
            {
                Popup.ReleasePopup(_type);
            }
        }

        public void ClearPhotoInfoList()
        {
            PartyMaister.PartyManagers.PrivateChat.PrivateChatPhotoInfo.ClearList();
        }
        #endregion

        #region PrivateChat Parser
        private void InitPrivateChatParser()
        {
            if (Parser == null)
            {
                Parser = new PrivateChatParser();
                Parser.Init();
            }
        }

        private void ReleasePrivateChatParser()
        {
            if (Parser != null)
            {
                Parser.Release();
                Parser = null;
            }
        }

        public IParseOutputData Parse(ParserType type, string key, IParseInputData data, IParseHelper helper = null)
        {
            if (Parser != null)
            {
                return Parser.Parse(type, key, data, helper);
            }
            return null;
        }
        #endregion

        #region Current Chat
        public void InitCurrentChat(UIPrivateChatPopup _popup)
        {
            CurrentChat = _popup;
        }

        public void ReleaseCurrentChat()
        {
            CurrentChatData.ClearMessageList();
            CurrentChat = null;
        }

        public PrivateChatData GetCurrentChatData()
        {
            return CurrentChatData;
        }

        public void SetCurrentChatData(PrivateChatData privateChatData)
        {
            CurrentChatData = privateChatData;
        }

        public void ReceivePrivateChatMessages(PrivateChatMessageContainer messageContainer)
        {
            // To do : 메세지에서 이미지 데이터 뽑아서 캐싱해야함.
            CurrentChatData.UpdateMessagesData(messageContainer);
            CurrentChat.OnChatting_Reflush();

            PrivateChatMessage lastMessage = messageContainer.GetLastMessage();
            UpdateLastMessage(lastMessage);
        }

        public void SetPrivateChatAlarm(bool state)
        {
            // To do : 디엠 알람 기능 구현 -> 서버에서 보내주는 데이터가 없어서 오스카랑 얘기해서 추가해야 함.

        }
        #endregion

        #region Try to open privatechat
        public long targetUserId { get; private set; }
        private const int messageRequestAmount = 20;
        public void TryOpenPrivateChat(long userId)
        {
            if (CurrentChat == null)
            {
                targetUserId = userId;
                if (UIInstance.Instance.GetPopup<UIPrivateChatListPopup>() == null)
                {
                    GameInstance.Instance.Send_PACKET_PRIVATECHAT_EXISTING_CHATS(true, RequestTargetUserData);
                }
                else
                {
                    RequestTargetUserData();
                }
            }
            else
            {
                // To do : PrivateChat Popup 켜져있는데 다시 들어가려고 하면...? PrivateChat 팝업으로 이동해야 하나..?
            }
        }

        private void RequestTargetUserData()
        {
            GameInstance.Instance.Send_PACKET_USER_DATA(targetUserId, RequestTargetUserDataCallback);
        }

        private void RequestTargetUserDataCallback()
        {
            Stanner stanner = StannerManager.Instance.GetStanners().ContentAt(0);

            // Current Chat Data 초기화
            PrivateChatData privateChatData = new PrivateChatData(stanner);
            SetCurrentChatData(privateChatData);

            PrivateChat privateChat = PrivateChats.SearchPrivateChatByUserId(targetUserId);
            if (privateChat == null)    // 기존에 PrivateChat 채팅을 한 기록이 없는 경우
            {
                // UIPrivateChatPopup 초기화
                UIPrivateChatPopup.Ininfo info = new UIPrivateChatPopup.Ininfo();
                info.targetUserId = stanner.userId;
                info.targetuserPhotoUrl = stanner.userPhotoURL;
                info.premium = stanner.userPremium;
                info.relation = stanner.userRelationShip;
                info.targetUserName = stanner.userName;
                info.closeCallback = delegate
                {
                    ReleasePopup(ePopupType.UIPrivateChatPopup);
                };
                ShowPopup(ePopupType.UIPrivateChatPopup, info);
            }
            else    // 기존에 PrivateChat 채팅을 한 기록이 있는 경우
            {
                ShowPopup(ePopupType.UIPrivateChatPopup);
                long? requestLastMessageId = (privateChat.lastMessage.messageId == -1 ? null : (long?)privateChat.lastMessage.messageId);
                long requestFirstMessageId = (long)Mathf.Max(privateChat.firstReadableMessageId, privateChat.lastMessage.messageId - messageRequestAmount);
                // To do : 상황에 맞게 특정 구간의 메세지들만 받아오는 기능 구현 필요
                GameInstance.Instance.Send_PACKET_PRIVATECHAT_MESSAGES(
                    targetUserId, requestFirstMessageId, requestLastMessageId);
            }
        }
        #endregion
    }
}
