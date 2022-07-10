using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    public class RoomTransitionInfo
    {
        public class PacketRequestContainer
        {
            public int Count { get; private set; }
            public Queue<(Action, string)> requestQueue { get; private set; }

            public PacketRequestContainer()
            {
                requestQueue = new Queue<(Action, string)>();
            }

            public void Clear()
            {
                requestQueue.Clear();
                Count = 0;
            }

            public void Enqueue(Action _action, string _packetName)
            {
                requestQueue.Enqueue((_action, _packetName));
                Count++;
            }

            public void Enqueue((Action, string) _action)
            {
                requestQueue.Enqueue(_action);
                Count++;
            }

            public (Action, string) Dequeue()
            {
                var tmp = requestQueue.Dequeue();
                if (tmp.Item1 != null || tmp.Item2 != null)
                {
                    Count--;
                    return tmp;
                }
                else
                {
                    return (null, string.Empty);
                }
            }

            public PacketRequestContainer CopyData()
            {
                PacketRequestContainer container = new PacketRequestContainer();
                IEnumerator e = requestQueue.GetEnumerator();
                while (e.MoveNext())
                {
                    container.Enqueue(((Action, string))e.Current);
                }
                return container;
            }
        }

        public class Actions
        {
            public PacketRequestContainer PacketRequestContainer { get; private set; } = new PacketRequestContainer();

            public Actions(Queue<(Action, string)> _actions)
            {
                while (_actions.Count > 0)
                {
                    PacketRequestContainer.Enqueue(_actions.Dequeue());
                }
            }

            public PacketRequestContainer CopyData()
            {
                return PacketRequestContainer.CopyData();
            }
        }

        public class ActionsContainer
        {
            private Queue<(Action, string)> requestQueue = new Queue<(Action, string)>();
            public Actions JoinLobbyActions { get; private set; }
            public Actions LeaveLobbyActions { get; private set; }
            public Actions JoinPartyroomActions { get; private set; }
            public Actions LeavePartyroomActions { get; private set; }
            public Actions JoinChatPartyActions { get; private set; }
            public Actions LeaveChatPartyActions { get; private set; }

            public ActionsContainer()
            {
                SetJoinLobbyActions();
                SetLeaveLobbyActions();
                SetJoinPartyroomActions();
                SetLeavePartyroomActions();
                SetJoinChatPartyActions();
                SetLeaveChatPartyActions();
                requestQueue = null;
            }

            private void SetJoinLobbyActions()
            {
                requestQueue.Clear();
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_USER_ADMIN_INFO(), "PACKET_USER_ADMIN_INFO"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_LOBBY_PARTY_ROOM_LIST(), "PACKET_LOBBY_PARTY_ROOM_LIST"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_FANDOMSHOP_STATUS(), "PACKET_FANDOMSHOP_STATUS"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_CHECK_VITAPOINTS(), "PACKET_CHECK_VITAPOINTS"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_WORD_BLOCK_LIST(), "PACKET_WORD_BLOCK_LIST"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_EVENT_STATUS(), "PACKET_EVENT_STATUS"));
                JoinLobbyActions = new Actions(requestQueue);
            }

            private void SetLeaveLobbyActions()
            {
                requestQueue.Clear();
                LeaveLobbyActions = new Actions(requestQueue);
            }

            private void SetJoinPartyroomActions()
            {
                requestQueue.Clear();
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_CHAT_PARTY_ENTRANCE(), "PACKET_CHAT_PARTY_ENTRANCE"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_PARTY_ROOM_INFO(), "PACKET_PARTY_ROOM_INFO"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_SMART_INFO(), "PACKET_SMART_INFO"));
                //requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_STREAMING_REMINDER_INFO(), "PACKET_STREAMING_REMINDER_INFO"));
                JoinPartyroomActions = new Actions(requestQueue);
            }

            private void SetLeavePartyroomActions()
            {
                requestQueue.Clear();
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_STREAMING_JOINING_ADMINS(false), "PACKET_STREAMING_JOINING_ADMINS"));
                LeavePartyroomActions = new Actions(requestQueue);
            }

            private void SetJoinChatPartyActions()
            {
                requestQueue.Clear();
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_PICTURE_LIST(), "PACKET_PICTURE_LIST"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_EVENT_STATUS(), "PACKET_EVENT_STATUS"));
                requestQueue.Enqueue((() => GameInstance.Instance.Send_PACKET_BOARD_NOTI_SEND(), "PACKET_BOARD_NOTI_SEND"));
                JoinChatPartyActions = new Actions(requestQueue);
            }

            private void SetLeaveChatPartyActions()
            {
                requestQueue.Clear();
                LeaveChatPartyActions = new Actions(requestQueue);
            }
        }
    }
}
