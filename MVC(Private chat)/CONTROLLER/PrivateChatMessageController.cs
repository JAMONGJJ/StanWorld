using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using ChatMsgView;

    public class PrivateChatMessageController
    {
        public PrivateChatData CurrentChatData { get; set; }
        public System.Action<long> OnReflush = null;

        public void Init()
        {
            CurrentChatData = new PrivateChatData();
        }

        public void Release()
        {
            CurrentChatData = null;
        }

        public void SetCurrentChatData(PrivateChatData _chatData)
        {
            CurrentChatData = _chatData;
        }

        public void ReceiveChatInfo(ChatMsg chat)
        {
            if (CurrentChatData.userId == chat.senderUserId || chat.senderUserId == GameInstance.Instance.MIDX)
            {
                CurrentChatData.AddItem(chat);
                if (OnReflush != null)
                    OnReflush(chat.senderUserId);
            }
        }

        public void ClearChatList()
        {
            CurrentChatData.ClearList();
        }
    }
}
