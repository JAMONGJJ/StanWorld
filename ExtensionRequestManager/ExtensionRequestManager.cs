using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities.Data;
using UnityEngine;

namespace stanworld
{
    using packets;
    using network;
    using System.Threading.Tasks;

    public delegate void CheckExtensionResponse(string cmd);

    public class PacketDelegateData
    {
        public int listenerIndex { get; private set; }
        public string command { get; private set; }
        public SFSObject Packet { get; private set; }

        public PacketDelegateData(int index, string cmd, SFSObject packet)
        {
            listenerIndex = index;
            command = cmd;
            Packet = packet;
        }
    }

    public class ExtensionRequestManager
    {
        RoomTransitionInfo.PacketRequestContainer requests;
        Action callBack;
        string currentRequestCMD;

        private bool processingRequest = false;
        private float waitingTimeOut = 10.0f;        // second
        private static float awaitedTimeAmount = 0.0f;
        private bool waitingForServerResponse = false;
        private Dictionary<string, int> WaitingPacketNameMap = new Dictionary<string, int>();
        private Dictionary<long, int> WaitingPacketIDMap = new Dictionary<long, int>();
        private System.Action ServerResponseCallback = null;
        private PacketDataContainer PacketDataContainer = null;
        private Queue<PacketDataContainer> WaitingPacketsData = new Queue<PacketDataContainer>();
        private Queue<PacketDelegateData> PacketDelegatesData = new Queue<PacketDelegateData>();
        public ExtensionRequestManager() { }

        private bool IsWaitingForServerResponse()
        {
            return waitingForServerResponse;
        }

        private bool IsProcessingRequest()
        {
            return processingRequest;
        }

        #region Extension send
        public void ExtensionSend(PacketDataContainer data)
        {
            AskServer(data);
        }

        private void AskServer(PacketDataContainer data)
        {
            try
            {
                if (processingRequest == true)
                {
                    LogFileWriter.Log("Extension request manager is now processing another request! Saving request in the Queue.");
                    WaitingPacketsData.Enqueue(data);
                    return;
                }

                if (data == null)
                {
                    throw new Exception("Packet data holder is null!");
                }
                else
                {
                    processingRequest = true;
                    PacketDataContainer = data;
                    if (PacketDataContainer.sendNextAfterResponse == false)
                    {
                        if (PacketDataContainer.synchronized == true)    // ��Ŷ�� ������ ������ ��ٷ��� �� ���, �������� ó���� �ʿ���.
                        {
                            UIInstance.Instance.ShowUIWaitingForReceiveData();
                            waitingForServerResponse = true;
                            ServerResponseCallback = PacketDataContainer.ReceiveCallback;

                            while (PacketDataContainer.PacketList.Count != 0)
                            {
                                PacketData packetData = PacketDataContainer.PacketList.Dequeue() as PacketData;
                                if (packetData == null)
                                {
                                    LogFileWriter.LogError("Packet data is null!");
                                    return;
                                }
                                else if (packetData.Packet.ContainsKey(PacketFlagClass.FLAG_COMMAND) == false)
                                {
                                    UIInstance.Instance.CloseUIWaitingForReceiveData();
                                    throw new Exception("There is no FLAG_COMMAND in the packet!");
                                }

                                // SWPID�� �ִ� ��� SWPID�� �����ϰ�, SWPID�� ���� ��� FLAG_COMMAND ����
                                // ������ ������ �������� ���������� SWPID�� ���� ��ȸ�ؼ� ��ٸ��� ��Ŷ�� �� ���� üũ��.
                                // CheckWaitingPacketList() �޼ҵ� Ȯ��.
                                if (packetData.Packet.ContainsKey(PacketFlagClass.STANWORLD_PACKET_ID))
                                {
                                    WaitingPacketIDMap.Add(packetData.Packet.GetLong(PacketFlagClass.STANWORLD_PACKET_ID), packetData.listenerIndex);
                                }
                                else
                                {
                                    WaitingPacketNameMap.Add(packetData.Packet.GetUtfString(PacketFlagClass.FLAG_COMMAND), packetData.listenerIndex);
                                }

                                if (packetData.sendToZone == true)
                                {
                                    ServerInstance.Instance.ExtensionSendToZone(packetData.Packet);
                                }
                                else
                                {
                                    ServerInstance.Instance.ExtensionSendToRoom(packetData.Packet);
                                }
                            }
                        }
                        else
                        {
                            ServerResponseCallback = PacketDataContainer.ReceiveCallback;
                            while (PacketDataContainer.PacketList.Count != 0)
                            {
                                PacketData packetData = PacketDataContainer.PacketList.Dequeue() as PacketData;
                                if (packetData.sendToZone == true)
                                {
                                    ServerInstance.Instance.ExtensionSendToZone(packetData.Packet);
                                }
                                else
                                {
                                    ServerInstance.Instance.ExtensionSendToRoom(packetData.Packet);
                                }
                            }
                        }
                    }
                    else    // dataHolder.sendNextAfterResponse == true�� ���, ������ synchronized == true�� ������.
                    {
                        UIInstance.Instance.ShowUIWaitingForReceiveData();
                        waitingForServerResponse = true;
                        ServerResponseCallback = PacketDataContainer.ReceiveCallback;
                        SendPacketAndWaitForResponse();
                    }
                }
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
            }
        }

        public void ExtensionSend(List<PacketDataContainer> packetDataContainer)
        {
            foreach (PacketDataContainer holder in packetDataContainer)
            {
                ExtensionSend(holder);
            }
        }

        private async void SendPacketAndWaitForResponse()
        {
            while (PacketDataContainer.PacketList.Count != 0)
            {
                PacketData packetData = PacketDataContainer.PacketList.Dequeue() as PacketData;
                if (packetData == null)
                {
                    LogFileWriter.LogError("Packet data is null!");
                    return;
                }

                if (packetData.Packet.ContainsKey(PacketFlagClass.STANWORLD_PACKET_ID))
                {
                    WaitingPacketIDMap.Add(packetData.Packet.GetLong(PacketFlagClass.STANWORLD_PACKET_ID), packetData.listenerIndex);
                }
                else
                {
                    WaitingPacketNameMap.Add(packetData.Packet.GetUtfString(PacketFlagClass.FLAG_COMMAND), packetData.listenerIndex);
                }

                if (packetData.sendToZone == true)
                {
                    ServerInstance.Instance.ExtensionSendToZone(packetData.Packet);
                }
                else
                {
                    ServerInstance.Instance.ExtensionSendToRoom(packetData.Packet);
                }

                while (IsWaitingPacketListEmpty() == false)
                {
                    await Task.Delay(10);
                }
            }
        }
        #endregion

        #region Extension response
        public void ExtensionResponse(string cmd, SFSObject packet)
        {
            long? packetID = null;
            if (packet.ContainsKey(PacketFlagClass.STANWORLD_PACKET_ID))
            {
                packetID = packet.GetLong(PacketFlagClass.STANWORLD_PACKET_ID);
            }
            int listenerIndex = SearchListenerIndex(cmd, packetID);
            if (IsWaitingForServerResponse() == true && CheckWaitingPacketList(cmd, packetID) == true)
            {
                PacketDelegateData data = new PacketDelegateData(listenerIndex, cmd, packet);
                PacketDelegatesData.Enqueue(data);
                ExtensionResponseCallback();
            }
            else
            {
                ResponseListener.Instance.Dispatch(cmd, packet, listenerIndex);
            }
        }

        private async void ExtensionResponseCallback()
        {
            while (IsWaitingForServerResponse() == true)
            {
                await Task.Delay(10);
                Core.System.UtilityManager.StartTimer(waitingTimeOut, TimeOutCallback);
            }

            if (ServerResponseCallback != null)
            {
                while (PacketDelegatesData.Count != 0)
                {
                    PacketDelegateData data = PacketDelegatesData.Dequeue();
                    ResponseListener.Instance.Dispatch(data.command, data.Packet, data.listenerIndex);
                }
                ServerResponseCallback.Invoke();
                ServerResponseCallback = null;
                UIInstance.Instance.CloseUIWaitingForReceiveData();
                ReleaseRequestProcess();
            }
        }

        private void TimeOutCallback()
        {
            if (IsWaitingForServerResponse() == true)
            {
                LogFileWriter.LogError(string.Empty, LogFileWriter.LogType.PACKET_RECEIVE_TIMEOUT);
                ServerResponseCallback = null;
                PacketDelegatesData.Clear();
                ResetAskingServerPackets();
                UIInstance.Instance.CloseUIWaitingForReceiveData();
                UIInstance.Instance.ShowUIServerNoResponse(ReleaseRequestProcess);
            }
        }

        private bool CheckWaitingPacketList(string cmd, long? packetID)
        {
            bool isWaitingPacket = false;
            if (packetID != null)
            {
                long id = (long)packetID;
                if (WaitingPacketIDMap.ContainsKey(id))
                {
                    WaitingPacketIDMap.Remove(id);
                    isWaitingPacket = true;
                }
            }
            else
            {
                if (WaitingPacketNameMap.ContainsKey(cmd))
                {
                    WaitingPacketNameMap.Remove(cmd);
                    isWaitingPacket = true;
                }
            }

            if (isWaitingPacket == true)
            {
                if (IsWaitingPacketListEmpty() == true && IsRemainPacketListEmpty() == true)
                {
                    ResetAskingServerPackets();
                }
            }

            return isWaitingPacket;
        }

        private void ResetAskingServerPackets()
        {
            awaitedTimeAmount = 0.0f;
            WaitingPacketIDMap.Clear();
            WaitingPacketNameMap.Clear();
            waitingForServerResponse = false;
        }

        private bool IsWaitingPacketListEmpty()
        {
            if (WaitingPacketIDMap.Count + WaitingPacketNameMap.Count == 0)
            {
                return true;
            }
            return false;
        }

        private bool IsRemainPacketListEmpty()
        {
            if (PacketDataContainer != null)
            {
                if (PacketDataContainer.PacketList.Count == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private int SearchListenerIndex(string cmd, long? packetID)
        {
            int result = 0;
            if (packetID != null)
            {
                long tmpID = (long)packetID;
                if (WaitingPacketIDMap.TryGetValue(tmpID, out result) == true)
                {
                    return result;
                }
            }
            else
            {
                if (WaitingPacketNameMap.TryGetValue(cmd, out result) == true)
                {
                    return result;
                }
            }
            return result;
        }

        private void ReleaseRequestProcess()
        {
            processingRequest = false;
            if (WaitingPacketsData.Count > 0)
            {
                LogFileWriter.Log("Extension request manager is processing next request!");
                AskServer(WaitingPacketsData.Dequeue());
            }
        }
        #endregion
    }
}