using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities.Variables;

namespace stanworld
{
    using System;

    public class PartyManager
    {
        public RoomAdminInfo.AdminInfoMap AdministratorMap { get; private set; } = new RoomAdminInfo.AdminInfoMap();

        #region Room's Administrator
        public void UpdateCurrentRoomAdministratorMap()
        {
            RoomVariable Administrators = ServerInstance.Instance.sfxInstance.LastJoinedRoom.GetVariable("Administrators");
            if (Administrators != null)
            {
                AdministratorMap.Parse(Administrators);
            }
        }

        public RoomAdminInfo.AdminInfoMap GetRoomAdministratorMap()
        {
            return AdministratorMap;
        }

        public bool CheckIsRoomAdministrator(long _midx)
        {
            return AdministratorMap.SearchAdmin(_midx);
        }

        public bool CheckIsLastRoomAdministrator(long _midx)
        {
            return AdministratorMap.CheckIsLastAdmin(_midx);
        }
        #endregion
    }
}
