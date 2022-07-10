using Sfs2X;
using Sfs2X.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using network;
    using network.data;
    using ChatPartyModel;
    using ChatMsgView;
    using Sfs2X.Entities.Variables;

    public class ChatPartyManager
    {
        public RoomAdminInfo.AdminInfoContainer ChattingAdminInfoContainer { get; private set; } = new RoomAdminInfo.AdminInfoContainer();
        
        #region Administrator
        public void UpdateChatPartyJoiningAdminList()
        {
            RoomVariable ChattingAdmins = ServerInstance.Instance.sfxInstance.LastJoinedRoom.GetVariable("ChatAdminList");
            if (ChattingAdmins != null)
            {
                ChattingAdminInfoContainer.Parse(ChattingAdmins);
            }
        }

        public bool CheckIsLastAdminJoiningChatParty(long _midx)
        {
            return ChattingAdminInfoContainer.CheckIsLastAdmin(_midx);
        }
        #endregion
    }
}
