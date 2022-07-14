using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using ChatMsgView;
    using network.data;
    using StannerModel;
    using PrivateChatModel;

    public class PrivateChatPopupController
    {
        #region VIEW
        public UIPrivateChatListPopup UIPrivateChatListPopup { get; private set; }
        public UIPrivateChatPopup UIPrivateChatPopup { get; private set; }
        public UIPrivateChatUserOptionPopup UIPrivateChatUserOptionPopup { get; private set; }
        public UIPrivateChatOptionPopup UIPrivateChatOptionPopup { get; private set; }
        public UIPrivateChatMenuPopup UIPrivateChatMenuPopup { get; private set; }
        public UIPrivateChatDeletePopup UIPrivateChatDeletePopup { get; private set; }
        #endregion

        public void Init()
        {

        }

        public void Release()
        {
            ReleasePrivateChatListPopup();
            ReleasePrivateChatPopup();
            ReleasePrivateChatOptionPopup();
            ReleasePrivateChatMenuPopup();
            ReleasePrivateChatDeletePopup();
            ReleasePrivateChatUserOptionPopup();
        }

        public void ShowPopup(ePopupType _type, UIData _data = null)
        {
            switch (_type)
            {
                case ePopupType.PrivateChatList:
                    {
                        RequestPrivateChatListPopupInitData();
                    }
                    break;
                case ePopupType.UIPrivateChatPopup:
                    {
                        InitPrivateChatPopup(_data);
                    }
                    break;
                case ePopupType.PrivateChatOption:
                    {
                        InitPrivateChatOptionPopup(_data);
                    }
                    break;
                case ePopupType.UIPrivateChatUserOptionPopup:
                    {
                        InitPrivateChatUserOptionPopup();
                    }
                    break;
                case ePopupType.UIPrivateChatMenuPopup:
                    {
                        InitPrivateChatMenuPopup(_data);
                    }
                    break;
                case ePopupType.DeletePrivateChat:
                    {
                        InitPrivateChatDeletePopup(_data);
                    }
                    break;
            }
        }

        public void ReleasePopup(ePopupType _type)
        {
            switch (_type)
            {
                case ePopupType.PrivateChatList:
                    {
                        ReleasePrivateChatListPopup();
                    }
                    break;
                case ePopupType.UIPrivateChatPopup:
                    {
                        ReleasePrivateChatPopup();
                    }
                    break;
                case ePopupType.PrivateChatOption:
                    {
                        ReleasePrivateChatOptionPopup();
                    }
                    break;
                case ePopupType.UIPrivateChatUserOptionPopup:
                    {
                        ReleasePrivateChatUserOptionPopup();
                    }
                    break;
                case ePopupType.UIPrivateChatMenuPopup:
                    {
                        ReleasePrivateChatMenuPopup();
                    }
                    break;
                case ePopupType.DeletePrivateChat:
                    {
                        ReleasePrivateChatDeletePopup();
                    }
                    break;
            }
        }

        #region Popup
        #region PrivateChatListPopup
        private void InitPrivateChatListPopup()
        {
            UIInstance.Instance.ShowPopupUI<UIPrivateChatListPopup>(ePopupType.PrivateChatList);
            UIPrivateChatListPopup = UIInstance.Instance.GetPopup<UIPrivateChatListPopup>();
        }

        private void ReleasePrivateChatListPopup()
        {
            UIInstance.Instance.ClosePopupUI<UIPrivateChatListPopup>();
            UIPrivateChatListPopup = null;
        }

        private void RequestPrivateChatListPopupInitData()
        {
            GameInstance.Instance.Send_PACKET_PRIVATECHAT_EXISTING_CHATS(true, ExistingChatsCallback);
        }

        private void ExistingChatsCallback()
        {
            List<long> userIdList = PartyMaister.PartyManagers.PrivateChat.GetPrivateChats().GetUserIdList();
            GameInstance.Instance.Send_PACKET_USER_DATA(userIdList, UserDataListCallback);
        }

        private void UserDataListCallback()
        {
            StannerContainer container = StannerManager.Instance.GetStanners();
            PartyMaister.PartyManagers.PrivateChat.PrivateChats.SetAdditionalStannerData(container);
            InitPrivateChatListPopup();
        }
        #endregion

        #region PrivateChatPopup
        private void InitPrivateChatPopup(UIData _data = null)
        {
            if (_data != null)
            {
                UIInstance.Instance.ShowPopupUI<UIPrivateChatPopup>(ePopupType.UIPrivateChatPopup, _data);
            }
            else
            {
                long targetUserId = PartyMaister.PartyManagers.PrivateChat.targetUserId;
                Stanner userInfo = PartyMaister.PartyManagers.PrivateChat.GetCurrentChatData().targetUserInfo;
                if (userInfo == null)
                {
                    LogFileWriter.LogError("InitPrivateChatPopup - userInfo is null!");
                    return;
                }

                UIPrivateChatPopup.Ininfo info = new UIPrivateChatPopup.Ininfo();
                info.targetUserId = userInfo.userId;
                info.relation = userInfo.userRelationShip;
                info.targetUserName = userInfo.userName;
                info.targetuserPhotoUrl = userInfo.userPhotoURL;
                info.premium = userInfo.userPremium;
                info.closeCallback = ReleasePrivateChatPopup;
                UIInstance.Instance.ShowPopupUI<UIPrivateChatPopup>(ePopupType.UIPrivateChatPopup, info);
            }
            UIPrivateChatPopup = UIInstance.Instance.GetPopup<UIPrivateChatPopup>();
        }

        private void ReleasePrivateChatPopup()
        {
            UIInstance.Instance.ClosePopupUI<UIPrivateChatPopup>();
            UIPrivateChatPopup = null;
        }
        #endregion

        #region PrivateChatOptionPopup
        private void InitPrivateChatOptionPopup(UIData _data = null)
        {
            UIInstance.Instance.ShowPopupUI<UIPrivateChatOptionPopup>(ePopupType.PrivateChatOption, _data);
            UIPrivateChatOptionPopup = UIInstance.Instance.GetPopup<UIPrivateChatOptionPopup>();
        }

        private void ReleasePrivateChatOptionPopup()
        {
            UIInstance.Instance.ClosePopupUI<UIPrivateChatOptionPopup>();
            UIPrivateChatOptionPopup = null;
        }
        #endregion

        #region PrivateChatUserOptionPopup
        private void InitPrivateChatUserOptionPopup(UIData _data = null)
        {
            var chatData = PartyMaister.PartyManagers.PrivateChat.GetCurrentChatData();
            UIPrivateChatUserOptionPopup.Ininfo ininfo = new UIPrivateChatUserOptionPopup.Ininfo();
            ininfo.relation = chatData.targetUserInfo.userRelationShip;
            ininfo.userName = chatData.targetUserInfo.userName;
            ininfo.userMidx = chatData.targetUserId;
            UIInstance.Instance.ShowPopupUI<UIPrivateChatUserOptionPopup>(ePopupType.UIPrivateChatUserOptionPopup, ininfo);
            UIPrivateChatUserOptionPopup = UIInstance.Instance.GetPopup<UIPrivateChatUserOptionPopup>();
        }

        private void ReleasePrivateChatUserOptionPopup()
        {
            UIInstance.Instance.ClosePopupUI<UIPrivateChatUserOptionPopup>();
            UIPrivateChatUserOptionPopup = null;
        }
        #endregion

        #region PrivateChatMenuPopup
        private void InitPrivateChatMenuPopup(UIData _data = null)
        {
            UIInstance.Instance.ShowPopupUI<UIPrivateChatMenuPopup>(ePopupType.UIPrivateChatMenuPopup, _data);
            UIPrivateChatMenuPopup = UIInstance.Instance.GetPopup<UIPrivateChatMenuPopup>();
        }

        private void ReleasePrivateChatMenuPopup()
        {
            UIInstance.Instance.ClosePopupUI<UIPrivateChatMenuPopup>();
            UIPrivateChatMenuPopup = null;
        }
        #endregion

        #region PrivateChatDeletePopup
        private void InitPrivateChatDeletePopup(UIData _data = null)
        {
            UIInstance.Instance.ShowPopupUI<UIPrivateChatDeletePopup>(ePopupType.DeletePrivateChat, _data);
            UIPrivateChatDeletePopup = UIInstance.Instance.GetPopup<UIPrivateChatDeletePopup>();
        }

        private void ReleasePrivateChatDeletePopup()
        {
            UIInstance.Instance.ClosePopupUI<UIPrivateChatDeletePopup>();
            UIPrivateChatDeletePopup = null;
        }
        #endregion
        #endregion
    }
}
