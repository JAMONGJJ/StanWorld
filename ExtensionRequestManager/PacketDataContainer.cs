using Sfs2X.Entities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    namespace packets
    {
        public class PacketData
        {
            public int listenerIndex { get; private set; }
            public bool sendToZone { get; private set; }
            public ISFSObject Packet { get; private set; }

            public PacketData()
            {
                listenerIndex = 0;
                sendToZone = true;
                Packet = null;
            }

            public PacketData(ISFSObject packet)
            {
                listenerIndex = 0;
                sendToZone = true;
                Packet = packet;
            }

            public PacketData(int index, bool toZone, ISFSObject packet)
            {
                listenerIndex = index;
                sendToZone = toZone;
                Packet = packet;
            }

            public void SetListenerIndex(int index)
            {
                listenerIndex = index;
            }

            public void SetSendToZone(bool state)
            {
                sendToZone = state;
            }

            public void SetPacket(ISFSObject packet)
            {
                Packet = packet;
            }
        }

        public class PacketDataContainer
        {
            public bool synchronized { get; private set; }
            public bool sendNextAfterResponse { get; private set; } // true일 경우 이전에 보낸 패킷의 응답이 오기전까지 다음 패킷 전송 안 함
            public Action ReceiveCallback { get; private set; }
            public Queue<PacketData> PacketList { get; private set; }

            public PacketDataContainer()
            {
                synchronized = false;
                sendNextAfterResponse = false;
                ReceiveCallback = null;
                PacketList = new Queue<PacketData>();
            }

            public PacketDataContainer(params ISFSObject[] packets)
            {
                synchronized = false;
                sendNextAfterResponse = false;
                ReceiveCallback = null;
                PacketList = new Queue<PacketData>();
                foreach (SFSObject packet in packets)
                {
                    PacketData data = new PacketData(0, true, packet);
                    PacketList.Enqueue(data);
                }
            }

            public PacketDataContainer(int listenerIndex, bool toZone, params ISFSObject[] packets)
            {
                synchronized = false;
                sendNextAfterResponse = false;
                ReceiveCallback = null;
                PacketList = new Queue<PacketData>();
                foreach (SFSObject packet in packets)
                {
                    PacketData data = new PacketData(listenerIndex, toZone, packet);
                    PacketList.Enqueue(data);
                }
            }

            public void SetSynchronized(bool state)
            {
                synchronized = state;
            }

            public void SetSendNextAfterResponse(bool state)
            {
                sendNextAfterResponse = state;
            }

            public void SetReceiveCallback(Action callback)
            {
                ReceiveCallback = callback;
            }

            public void EnqueuePacketData(PacketData packet)
            {
                if (PacketList != null)
                {
                    PacketList.Enqueue(packet);
                }
            }

            public PacketData DequeuePacketData()
            {
                if (PacketList != null && PacketList.Count > 0)
                {
                    return PacketList.Dequeue();
                }
                return null;
            }
        }
    }
}
