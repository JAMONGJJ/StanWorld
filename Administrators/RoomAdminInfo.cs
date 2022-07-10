using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using network;
    using network.data;
    using Sfs2X.Entities.Data;
    using Sfs2X.Entities.Variables;

    public class RoomAdminInfo
    {
        public class AdminInfo
        {
            public long userMIDX { get; private set; }

            public AdminInfo(long _midx)
            {
                userMIDX = _midx;
            }
        }

        public class AdminInfoContainer
        {
            public List<AdminInfo> adminInfoList { get; private set; } = new List<AdminInfo>();

            public AdminInfoContainer() { }

            public void Parse(RoomVariable _variable)
            {
                if (_variable == null) return;

                adminInfoList.Clear();
                ISFSArray tmpArray = _variable.GetSFSArrayValue();
                foreach (long data in tmpArray)
                {
                    AdminInfo info = new AdminInfo(data);
                    adminInfoList.Add(info);
                }
            }

            public bool SearchAdmin(long _midx)
            {
                foreach (var admin in adminInfoList)
                {
                    if (admin.userMIDX == _midx)
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool CheckIsLastAdmin(long _midx)
            {
                if (adminInfoList.Count == 1 && adminInfoList[0].userMIDX == _midx)
                {
                    return true;
                }
                return false;
            }
        }

        public class AdminInfoMap
        {
            public Dictionary<long, eAdminType> adminInfoMap { get; private set; } = new Dictionary<long, eAdminType>();

            public AdminInfoMap() { }

            public void Parse(RoomVariable _variable)
            {
                if (_variable == null) return;

                adminInfoMap.Clear();
                ISFSArray array = _variable.GetSFSArrayValue();
                foreach (SFSObject data in array)
                {
                    long midx = data.GetLong(PacketFlagClass.FLAG_USER_MIDX);
                    eAdminType adminType = (eAdminType)data.GetInt(PacketFlagClass.FLAG_USER_ADMIN_TYPE);
                    adminInfoMap.Add(midx, adminType);
                }
            }

            public bool SearchAdmin(long _midx)
            {
                if (adminInfoMap.ContainsKey(_midx))
                {
                    return true;
                }
                return false;
            }

            public bool CheckIsLastAdmin(long _midx)
            {
                if (adminInfoMap.Count == 1 && adminInfoMap.ContainsKey(_midx))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
