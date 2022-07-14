using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using stanworld.network;
using stanworld.network.data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    public enum RoomType
    {
        None = -1,
        Lobby,
        PartyRoom,
        StarChat
    }

    public class RoomInfo
    {
        public RoomType myRoomType { get; private set; } = RoomType.None;
        public string myRoomName { get; private set; } = string.Empty;
        public int myRoomId { get; private set; } = -1;


        public RoomInfo(Room _room)
        {
            myRoomType = (RoomType)Enum.Parse(typeof(RoomType), _room.GroupId, true);
            myRoomName = _room.Name;
            myRoomId = _room.Id;
        }

        public void SetRoomType(RoomType _type)
        {
            myRoomType = _type;
        }
        public void SetRoomName(string _name)
        {
            myRoomName = _name;
        }
        public void SetRoomId(int _id)
        {
            myRoomId = _id;
        }
    }

    public class RoomTransitionManager
    {
        private RoomTransitionInfo.PacketRequestContainer PacketRequestContainer = new RoomTransitionInfo.PacketRequestContainer();
        private Queue<Action> sceneLoadingEventQueue = new Queue<Action>();

        public Room currentRoom { get; private set; }
        public RoomInfo currentRoomInfo { get; private set; }
        public RoomInfo GetCurrentRoomInfo()
        {
            return currentRoomInfo;
        }

        public string JoiningRoomName { get; private set; }
        public void SetJoiningRoomName(string _name)
        {
            JoiningRoomName = _name;
        }

        public static bool LeaveRoomCompleted { get; private set; } = true;
        public static bool isTransitioningRoom { get; private set; } = false;

        private RoomTransitionInfo.ActionsContainer RoomTransitionActionsContainer;

        public RoomTransitionManager()
        {
            RoomTransitionActionsContainer = new RoomTransitionInfo.ActionsContainer();
        }

        public void SetIsTransitioningRoom(bool _isTransitioningRoom)
        {
            isTransitioningRoom = _isTransitioningRoom;
        }

        public void AddLoadSceneCompleteEvent(Action loadSceneCompleteEvent)
        {
            sceneLoadingEventQueue.Enqueue(loadSceneCompleteEvent);
        }
        #region Room transition
        public void TryJoinRoom(eStanWorldStates _state)
        {
            if (CanJoinRoom(JoiningRoomName))
            {
                GameInstance.Instance.Next(_state);
            }
            else
            {
                LogFileWriter.LogError("Cannot transit room right now!");
            }
        }


        private bool CanJoinRoom(string _roomName)
        {
            if (!string.IsNullOrEmpty(GameInstance.Instance.partyRoomStarName) && currentRoom.Name == _roomName && GameInstance.Instance.GetCurrentState() != eStanWorldStates.Reconnection)
            {
                P_UINoticePopup.InInfo ininfo = new P_UINoticePopup.InInfo();
                ininfo.desc = "You are already in this party room.";
                ininfo.callback = null;

                UIInstance.Instance.ShowPopupUI<P_UINoticePopup>(ePopupType.P_UINoticePopup, ininfo);
                return false;
            }
            else if (isTransitioningRoom)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void StartRoomTransition()
        {
            try
            {
                LogFileWriter.Log(string.Empty, LogFileWriter.LogType.ROOM_TRANSITION_START);
                LeaveRoomCompleted = false;
                isTransitioningRoom = true;
                if (GameInstance.Instance.MyPlayerController != null)
                {
                    GameInstance.Instance.MyPlayerController.bCanMove = false;
                }
                if (GameInstance.Instance.touchPanel != null)
                {
                    GameInstance.Instance.touchPanel.bCanRotate = false;
                }
                LoadingInstance.Instance.StartLoadingTransition(delegate
                {
                    UIInstance.Instance.ExitPopupUI();
                    if (ServerInstance.Instance.sfxInstance?.LastJoinedRoom != null)
                    {
                        DoSetLastPosition();
                        BeforeLeaveRoom(currentRoomInfo.myRoomType);
                    }
                    else    // First time joining Lobby(LoginState, CreateCharacterState) -> No need to Leave room, Just Join room.
                    {
                        FirstJoinStanWorld();
                        LeaveRoomCompleted = true;
                        return;
                    }
                });
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
            }
        }

        private void FirstJoinStanWorld()
        {
            Core.System.UtilityManager.OpenUtilityControl();
            PartyMaister.PartyManagers.PrivateChat.OpenPrivateChatControl();
        }

        private void BeforeLeaveRoom(RoomType _type)
        {
            LogFileWriter.Log($"{_type}", LogFileWriter.LogType.ROOM_TRANSITION_BEFORE_LEAVE_ROOM);
            PacketRequestContainer.Clear();
            switch (_type)
            {
                case RoomType.Lobby:
                    {
                        SendPacketsToServer_LeaveLobby();
                    }
                    break;
                case RoomType.PartyRoom:
                    {
                        SendPacketsToServer_LeavePartyroom();
                    }
                    break;
                case RoomType.StarChat:
                    {
                        SendPacketsToServer_LeaveChatParty();
                    }
                    break;
                default:
                    break;
            }
            ExtensionRequestManager requestManager = new ExtensionRequestManager(PacketRequestContainer, () => DoLeaveRoom());
        }

        private void DoLeaveRoom()
        {
            ServerInstance.Instance.sfxInstance.Send(new LeaveRoomRequest(ServerInstance.Instance.sfxInstance.LastJoinedRoom));
            LogFileWriter.Log($"{currentRoomInfo.myRoomType}", LogFileWriter.LogType.ROOM_TRANSITION_LEAVE_ROOM_REQUEST);
            LeaveRoomCompleted = true;
        }

        public void RequestJoinRoom(RoomType roomType)
        {
            if(GameInstance.Instance.MyPlayerController)
                GameInstance.Instance.MyPlayerController.bCanMove = false;

            LogFileWriter.Log($"{roomType}", LogFileWriter.LogType.ROOM_TRANSITION_JOIN_ROOM_REQUEST);
            try
            {
                switch (roomType)
                {
                    case RoomType.Lobby:
                        {
                            ServerInstance.Instance.sfxInstance.Send(new JoinRoomRequest(StringsKeyDefine.LobbyName));
                        }
                        break;
                    case RoomType.PartyRoom:
                        if (!string.IsNullOrEmpty(JoiningRoomName))
                        {
                            ServerInstance.Instance.sfxInstance.Send(new JoinRoomRequest(JoiningRoomName));
                        }
                        else 
                        {
                            LogFileWriter.Log("RoomID is null");
                        }
                        break;
                    case RoomType.StarChat:
                        if (!string.IsNullOrEmpty(JoiningRoomName))
                        {
                            ServerInstance.Instance.sfxInstance.Send(new JoinRoomRequest(JoiningRoomName));
                        }
                        else
                        {
                            LogFileWriter.Log("RoomID is null");
                        }
                        break;
                    default:
                        LogFileWriter.Log("RequestJoinRoom RoomType Error! Not valid room type.");
                        return;
                }
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
            }
        }

        private void DoSetLastPosition()
        {
            switch (currentRoomInfo.myRoomType)
            {
                case RoomType.None:
                    break;
                case RoomType.Lobby:
                    {
                        if (GameInstance.Instance.MyPlayer)
                            GameInstance.Instance.lastLobbyPos = GameInstance.Instance.MyPlayer.transform.position;
                        else
                            GameInstance.Instance.lastLobbyPos = null;
                    }
                    break;
                case RoomType.PartyRoom:
                    {
                        if (GameInstance.Instance.MyPlayer)
                            GameInstance.Instance.lastPartyRoomPos = GameInstance.Instance.MyPlayer.transform.position;
                        else
                            GameInstance.Instance.lastPartyRoomPos = null;
                    }
                    break;
                case RoomType.StarChat:
                    break;
            }
        }

        public void ResponseJoinRoom(BaseEvent evt)
        {
            Room room = evt.Params["room"] as Room;
            LogFileWriter.Log($"Room type : {room.GroupId}, Room name : {room.Name}, Room id : {room.Id}", LogFileWriter.LogType.ROOM_TRANSITION_SUCCESS);
            currentRoomInfo = new RoomInfo(room);
            if (GameInstance.Instance.CurrentLeaderboard != null)
            {
                UIInstance.Instance.ClosePopupUI<UILeaderBoard>();
            }

            if (room != null)
            {
                GameInstance.Instance.UpdateCurrentJoinRoom(room);
                currentRoom = room;

                switch (room.GroupId)
                {
                    case string group when (group == RoomType.Lobby.ToString()):
                        {
                            Core.System.LogManager.SendEnterLocation(LobbyPartyRoomType.Lobby, "Lobby", PartyType.None);
                            currentRoomInfo.SetRoomType(RoomType.Lobby);
                            DoJoinLobby();
                        }
                        break;
                    case string group when (group == RoomType.PartyRoom.ToString()):
                        {                                                       
                            Core.System.LogManager.SendEnterLocation(LobbyPartyRoomType.PartyRoom, room.Name, PartyType.None);
                            currentRoomInfo.SetRoomType(RoomType.PartyRoom);
                            DoJoinPartyRoom();
                        }
                        break;
                    case string group when (group == RoomType.StarChat.ToString()):
                        {
                            string partyRoomName = PartyMaister.PartyManagers.ChatParty.chattingInfo.StarName;
                            Core.System.LogManager.SendEnterLocation(LobbyPartyRoomType.PartyRoom, partyRoomName, PartyType.Chat);
                            currentRoomInfo.SetRoomType(RoomType.StarChat);
                            DoJoinChatParty();
                        }
                        break;
                }
            }
        }

        public void JoinRoomError(BaseEvent evt)
        {
            LogFileWriter.Log(string.Empty, LogFileWriter.LogType.ROOM_TRANSITION_JOIN_ROOM_ERROR);
            GameInstance.Instance.isFromChatroomToPartyRoom = false;
            TryJoinRoom(eStanWorldStates.Lobby);
        }

        public void CreateRoomError(BaseEvent evt)
        {
            LogFileWriter.Log(string.Empty, LogFileWriter.LogType.ROOM_TRANSITION_CREATE_ROOM_ERROR);
            TryJoinRoom(eStanWorldStates.Lobby);
        }

        private void InitSceneState(UserLocation _location)
        {
            if (!RuntimeManager.IsInitialized())
                RuntimeManager.Init();

            UIInstance.Instance.SetWorldCanvas();
            GameInstance.Instance.SetCameraMoveActive(true);
            GameInstance.Instance.SetUserLocation(_location);
            GameInstance.Instance.ClearPlayer();
            GameInstance.Instance.EnterField();
            GameInstance.Instance.ClearUpdateUsersCharacters();

            //UIInstance.Instance.DestoryMainHUD();
            UIInstance.Instance.CreateMainHUD();
        }

        private void AfterJoinRoom(RoomType _type)
        {
            LogFileWriter.Log($"{_type}", LogFileWriter.LogType.ROOM_TRANSITION_PACKET_REQUEST_START);
            PacketRequestContainer.Clear();
            switch (_type)
            {
                case RoomType.Lobby:
                    {
                        SendPacketsToServer_JoinLobby();
                    }
                    break;
                case RoomType.PartyRoom:
                    {
                        SendPacketsToServer_JoinPartyroom();
                    }
                    break;
                case RoomType.StarChat:
                    {
                        SendPacketsToServer_JoinChatparty();
                    }
                    break;
                default:
                    break;
            }
            ExtensionRequestManager requestManager = new ExtensionRequestManager(PacketRequestContainer, () => AfterPacketsReceived(currentRoomInfo.myRoomType));
        }

        private void AfterPacketsReceived(RoomType _type)
        {
            LogFileWriter.Log($"{_type}", LogFileWriter.LogType.ROOM_TRANSITION_PACKET_RESPONSE_END);
            switch (_type)
            {
                case RoomType.Lobby:
                    {
                        AfterPacketsReceived_JoinLobby();
                    }
                    break;
                case RoomType.PartyRoom:
                    {
                        AfterPacketsReceived_JoinPartyroom();
                    }
                    break;
                case RoomType.StarChat:
                    {
                        AfterPacketsReceived_JoinChatParty();
                    }
                    break;
                default:
                    break;
            }
            EndLoadingTransaction();
            isTransitioningRoom = false;
            LogFileWriter.Log(string.Empty, LogFileWriter.LogType.ROOM_TRANSITION_END);
        }

        private void EndLoadingTransaction()
        {
            LoadingInstance.Instance.ShowLoadingHide();
            LoadingInstance.Instance.EndLoadingTransition();
        }
        #endregion

        #region Lobby
        private void SendPacketsToServer_LeaveLobby()
        {
            GameInstance.Instance.LeaveField();
            Core.System.StannerCenter.ReleaseStannerCenterObjectControl();
            PacketRequestContainer = RoomTransitionActionsContainer.LeaveLobbyActions.CopyData();
        }

        private void DoJoinLobby()
        {
            GameInstance.Instance.LoadScene(SceneDefine.Lobby, FinishLobbySceneLoad, true);
        }

        private void FinishLobbySceneLoad()
        {
            AuthInstance.Instance.InitPush();
            LoadingInstance.Instance.SetLoadingCanvasResolution(eScreenType.Landscape);
            UIInstance.Instance.ChangeScreen(eScreenType.Landscape);

            Core.System.StannerCenter.InitStannerCenterObjectControl();
            Core.System.StannerCenter.CheckStannerCenter();

            if (false == GameInstance.Instance.isFirstEnterLobby)
            {
                GameInstance.Instance.SetFirstEnterLobby();
            }

            InitSceneState(UserLocation.Lobby);
            GameInstance.Instance.SetCurrentPartyType(network.data.PartyType.None);
            GameInstance.Instance.partyRoomStarName = null;
            GameInstance.Instance.partyRoomFandomName = null;
            GameInstance.Instance.partyRoomAmbassadorName = null;
            GameInstance.Instance.ambassadorMidx = 0;

            DoLoadSceneCompleteEvent();

            if (GameInstance.Instance.MyPlayerController)
            {
                GameInstance.Instance.MyPlayerController.bCanMove = true;
            }

            AfterJoinRoom(RoomType.Lobby);
        }

        private void SendPacketsToServer_JoinLobby()
        {
            string referring_link = PlayerPrefs.GetString("referring_link", string.Empty);
            if (!string.IsNullOrEmpty(referring_link))
            {
                GameInstance.Instance.Send_PACKET_LINK_ADD_POINT(referring_link);
            }
            if (GameInstance.Instance.bFirstLogin)
            {
                GameInstance.Instance.Send_PACKET_STANWORLD_NOTICE();
                GameInstance.Instance.bFirstLogin = false;
            }
            GameInstance.Instance.SendVariable();
            GameInstance.Instance.SendLoginHistory();
            PacketRequestContainer = RoomTransitionActionsContainer.JoinLobbyActions.CopyData();
        }

        private void AfterPacketsReceived_JoinLobby()
        {
            GameInstance.Instance.Send_PACKET_CHECKIN_NOTI();   // There are some cases that server doesn't reply. So not handled by requestManager.
            if (UIInstance.Instance.GetPopup<UIAccessPopup>() != null)
            {
                UIInstance.Instance.ClosePopupUI<UIAccessPopup>();
            }
        }
        #endregion

        #region Partyroom
        private void SendPacketsToServer_LeavePartyroom()
        {
            GameInstance.Instance.LeaveField();
            PartyMaister.PartyManagers.Partyroom.ClosePartyroomControl();

            PacketRequestContainer = RoomTransitionActionsContainer.LeavePartyroomActions.CopyData();
        }

        private void DoJoinPartyRoom()
        {
            GameInstance.Instance.LoadScene(SceneDefine.Partyroom, FinishPartyroomSceneLoad);
        }

        private void FinishPartyroomSceneLoad()
        {
            LoadingInstance.Instance.SetLoadingCanvasResolution(eScreenType.Landscape);
            UIInstance.Instance.ChangeScreen(eScreenType.Landscape);

            InitSceneState(UserLocation.PartyRoom);

            DoLoadSceneCompleteEvent();

            if (GameInstance.Instance.MyPlayerController)
            {
                GameInstance.Instance.MyPlayerController.bCanMove = true;
            }

            AfterJoinRoom(currentRoomInfo.myRoomType);
        }

        private void SendPacketsToServer_JoinPartyroom()
        {
            GameInstance.Instance.SendVariable();

            PacketRequestContainer = RoomTransitionActionsContainer.JoinPartyroomActions.CopyData();
        }
        
        private void AfterPacketsReceived_JoinPartyroom()
        {
            PartyMaister.PartyManagers.Partyroom.OpenPartyroomControl();

            GameInstance.Instance.DoCheckVitaPointInPartyRoom();
            GameInstance.Instance.DoCheckUserCountInPartyRoom();
            GameInstance.Instance.DoCheckStreamingDurationInPartyRoom();

        }
        #endregion

        #region Chatparty
        private void SendPacketsToServer_LeaveChatParty()
        {
            Application.targetFrameRate = 30;
            PartyMaister.PartyManagers.ChatParty.CloseChatPartyControl();
            PartyMaister.PartyManagers.ChatParty.ClosePopup(ePopupType.UIChatPartyPopup);
            GameInstance.Instance.IsChatting = false;
            PacketRequestContainer = RoomTransitionActionsContainer.LeaveChatPartyActions.CopyData();
        }

        private void DoJoinChatParty()
        {
            GameInstance.Instance.LoadScene(SceneDefine.ChatParty, FinishChatpartySceneLoad);
        }

        private void FinishChatpartySceneLoad()
        {
            PartyMaister.PartyManagers.ChatParty.OpenChatPartyControl();
            LoadingInstance.Instance.SetLoadingCanvasResolution(eScreenType.Portrait);
            UIInstance.Instance.ChangeScreen(eScreenType.Portrait);
            PartyMaister.PartyManagers.ChatParty.ShowPopup(ePopupType.UIChatPartyPopup);

            GameInstance.Instance.SetCurrentPartyType(network.data.PartyType.Chat);
            GameInstance.Instance.SetUserLocation(UserLocation.Chat);
            GameInstance.Instance.partyRoomStarName = null;
            GameInstance.Instance.partyRoomFandomName = null;
            GameInstance.Instance.partyRoomAmbassadorName = null;
            GameInstance.Instance.ambassadorMidx = 0;


            DoLoadSceneCompleteEvent();

            AfterJoinRoom(currentRoomInfo.myRoomType);
        }

        private void SendPacketsToServer_JoinChatparty()
        {
            PacketRequestContainer = RoomTransitionActionsContainer.JoinChatPartyActions.CopyData();

        }

        private void AfterPacketsReceived_JoinChatParty()
        {
            Application.targetFrameRate = 60;
        }

        private void DoLoadSceneCompleteEvent()
        {
            while (sceneLoadingEventQueue.Count != 0)
            {
                System.Action action = sceneLoadingEventQueue.Dequeue();
                action?.Invoke();
            }
        }
        #endregion
    }
}