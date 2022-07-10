using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities.Data;

namespace stanworld
{
    using network;
    using network.data;
    using packets;
    using SimpleJSON;
    using ChatMsgView;
    using PrivateChatModel;

    namespace network
    {
        public partial class ResponseListenerContainer
        {
            private void DoInitListener_PACKET_PrivateChat()
            {
                ResponseListenerList.AddListener(PacketNameClass.PACKET_DM_FRIEND_LIST, RECEIVE_PACKET_DM_FRIEND_LIST);
                ResponseListenerList.AddListener(PACKET_PRIVATECHAT_MESSAGES.PacketName, RECEIVE_PACKET_PRIVATECHAT_MESSAGES, 0);
                ResponseListenerList.AddListener(PACKET_PRIVATECHAT_EXISTING_CHATS.PacketName, RECEIVE_PACKET_PRIVATECHAT_EXISTING_CHATS, 0);

            }

            private void RECEIVE_PACKET_DM_FRIEND_LIST(SFSObject data)
            {
                UIPrivateChatListPopup messagePopup = UIInstance.Instance.GetPopup<UIPrivateChatListPopup>();
                if (messagePopup)
                {
                    messagePopup.FriendUpdateScrollView(DMFriendList.Parse(data));
                    messagePopup.isMoreLoading = false;
                }
            }

            private void RECEIVE_PACKET_PRIVATECHAT_MESSAGES(SFSObject data)
            {
                if (PartyMaister.PartyManagers.PrivateChat.CurrentChat == null)
                {
                    PartyMaister.PartyManagers.PrivateChat.SetPrivateChatAlarm(true);
                    return;
                }

                PACKET_PRIVATECHAT_MESSAGES.ChatMessageList chatMessageList = PACKET_PRIVATECHAT_MESSAGES.ChatMessageList.ParseSFSObject(data);
                PrivateChatMessageContainer messageContainer = PartyMaister.PartyManagers.PrivateChat.Parse(ParserType.SFStoMODEL, PACKET_PRIVATECHAT_MESSAGES.PacketName, chatMessageList) as PrivateChatMessageContainer;
                PartyMaister.PartyManagers.PrivateChat.ReceivePrivateChatMessages(messageContainer);
            }

            private void RECEIVE_PACKET_PRIVATECHAT_EXISTING_CHATS(SFSObject data)
            {
                PACKET_PRIVATECHAT_EXISTING_CHATS.PrivateChats existingChats = PACKET_PRIVATECHAT_EXISTING_CHATS.PrivateChats.ParseSFSObject(data);
                PrivateChatContainer container = PartyMaister.PartyManagers.PrivateChat.Parse(ParserType.SFStoMODEL, PACKET_PRIVATECHAT_EXISTING_CHATS.PacketName, existingChats) as PrivateChatContainer;
                PartyMaister.PartyManagers.PrivateChat.SetPrivateChats(container);
            }
        }
    }

    public partial class GameInstance : Singleton<GameInstance>
    {        
        #region DM
        public void Send_PACKET_DM_FRIEND_LIST(int in_page, string in_searchWord)
        {
            ISFSObject so = new SFSObject();
            so.PutInt(PacketFlagClass.FLAG_PAGE, in_page);
            so.PutUtfString(PacketFlagClass.FLAG_SEARCH_WORD, in_searchWord);
            ServerInstance.Instance.ExtensionSend(PacketNameClass.PACKET_DM_FRIEND_LIST, so);
        }
        #endregion

        #region PRIVATECHAT_RENEWAL_PACKETS
        public void Send_PACKET_PRIVATECHAT_EXISTING_CHATS(bool synchronized, Action callback)
        {
            ISFSObject so = PACKET_PRIVATECHAT_EXISTING_CHATS.Request.MakeSfsObject();
            PacketDataContainer container = new PacketDataContainer(0, true, so);
            container.SetSynchronized(synchronized);
            container.SetReceiveCallback(callback);
            Core.System.ExtensionRequestManager.ExtensionSend(container);
        }

        public void Send_PACKET_PRIVATECHAT_NEW_MESSAGE(long targetUserId, PrivateChatMessage message)
        {
            ISFSObject so = PACKET_PRIVATECHAT_NEW_MESSAGE.Request.MakeSfsObject(
                targetUserId,
                message.replyMessageId == -1 ? null : (long?)message.replyMessageId,
                message.messageContent.jsonContent,
                (int)message.messageType);
            ServerInstance.Instance.ExtensionSend(so, true);
        }

        public void Send_PACKET_PRIVATECHAT_NEW_MESSAGE(long targetUserId, string messageContent, eMsgType messageType)
        {
            ISFSObject so = PACKET_PRIVATECHAT_NEW_MESSAGE.Request.MakeSfsObject(
                targetUserId,
                null,
                messageContent,
                (int)messageType);
            ServerInstance.Instance.ExtensionSend(so, true);
        }

        public void Send_PACKET_PRIVATECHAT_REMOVE(long friendUserId)
        {
            ISFSObject so = PACKET_PRIVATECHAT_REMOVE.Request.MakeSfsObject(friendUserId);
            ServerInstance.Instance.ExtensionSend(so, true);
        }

        public void RequestPrivateChatRemove(long friendUserId, Action callback)
        {
            ISFSObject so = PACKET_PRIVATECHAT_REMOVE.Request.MakeSfsObject(friendUserId);
            PacketDataContainer container = new PacketDataContainer();
            container.SetSynchronized(true);
            container.SetReceiveCallback(callback);
            PacketData data = new PacketData(so);
            container.EnqueuePacketData(data);
            Core.System.ExtensionRequestManager.ExtensionSend(container);
        }

        public void Send_PACKET_PRIVATECHAT_MESSAGES_READ(long friendUserId, long? lastReadMessageId)
        {
            ISFSObject so = PACKET_PRIVATECHAT_MESSAGES_READ.Request.MakeSfsObject(friendUserId, lastReadMessageId);
            ServerInstance.Instance.ExtensionSend(so, true);
        }

        public void Send_PACKET_PRIVATECHAT_HIDE_MESSAGE(long friendUserId, long messageId)
        {
            ISFSObject so = PACKET_PRIVATECHAT_HIDE_MESSAGE.Request.MakeSfsObject(friendUserId, messageId);
            ServerInstance.Instance.ExtensionSend(so, true);
        }

        public void Send_PACKET_PRIVATECHAT_MESSAGES(long friendUserId, long messageIdFrom, long? messageIdTo)
        {
            ISFSObject so = PACKET_PRIVATECHAT_MESSAGES.Request.MakeSfsObject(friendUserId, messageIdFrom, messageIdTo);
            ServerInstance.Instance.ExtensionSend(so, true);
        }

        public void RequestPrivateChatMessages(long friendUserId, long messageIdFrom, long? messageIdTo, Action callback)
        {
            ISFSObject so = PACKET_PRIVATECHAT_MESSAGES.Request.MakeSfsObject(friendUserId, messageIdFrom, messageIdTo);
            PacketDataContainer container = new PacketDataContainer(so);
            container.SetReceiveCallback(callback);
            container.SetSynchronized(true);
            Core.System.ExtensionRequestManager.ExtensionSend(container);
        }
        #endregion
    }
}
