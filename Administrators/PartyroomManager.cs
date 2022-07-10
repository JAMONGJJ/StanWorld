using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using network.data;
    public class PartyroomManager
    {
        public RoomAdminInfo.AdminInfoContainer StreamingJoiningAdminInfoContainer { get; private set; } = new RoomAdminInfo.AdminInfoContainer();
        
        #region Administrator
        public void UpdateStreamingJoiningAdminList()
        {
            RoomVariable StreamingAdmins = ServerInstance.Instance.sfxInstance.LastJoinedRoom.GetVariable("StreamingAdminList");
            if (StreamingAdmins != null)
            {
                StreamingJoiningAdminInfoContainer.Parse(StreamingAdmins);
            }
        }

        public bool CheckIsLastAdminJoiningStreaming(long _midx)
        {
            UpdateStreamingJoiningAdminList();
            return StreamingJoiningAdminInfoContainer.CheckIsLastAdmin(_midx);
        }
        #endregion
    }
}
