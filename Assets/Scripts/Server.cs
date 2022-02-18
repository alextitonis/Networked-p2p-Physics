using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    [SerializeField] int port;
    [SerializeField] Vector3[] positions;

    EventBasedNetListener listener;
    NetManager server;

    Dictionary<NetPeer, Vector3> players = new Dictionary<NetPeer, Vector3>();

    void Start()
    {
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(port);

        listener.ConnectionRequestEvent += request => request.Accept();
        listener.PeerConnectedEvent += peer => {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((ushort)0);
            writer.Put(peer.Id);
            Vector3 pos = positions[Random.Range(0, positions.Length)];
            writer.Put(pos.x);
            writer.Put(pos.y);
            writer.Put(pos.z);
            Send(peer, writer);
            players.Add(peer, pos);

            foreach (var p in players)
            {
                NetDataWriter w = new NetDataWriter();
                w.Put((ushort)1);
                w.Put(p.Key.Id);
                Vector3 po = WorldManager.getInstance.getPlayerPosition(p.Key.Id);
                if (po == Vector3.zero)
                {
                    w.Put(p.Value.x);
                    w.Put(p.Value.y);
                    w.Put(p.Value.z);
                }
                else
                {
                    w.Put(po.x);
                    w.Put(po.y);
                    w.Put(po.z);
                }

                if (p.Key.Id == peer.Id)
                {
                    foreach (var _p in players)
                    {
                        if (_p.Key != p.Key)
                            Send(_p.Key, w);
                    }
                }
                else Send(peer, w);
            }
        };
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
    }

    private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        ushort packetId = reader.GetUShort();
        if (packetId == 2)
        {
            Snapshot sn = WorldManager.getInstance.currentSnapshot;
            sn.Deserialize(reader);
            NetDataWriter writer = new NetDataWriter();
            writer.Put((ushort)2);
            sn.Serialize(ref writer, out _);
            SendToAllExcept(peer, writer);
        }
    }

    void Update()
    {
        server.PollEvents();
    }

    public void Send(NetPeer peer, NetDataWriter writer, DeliveryMethod dm = DeliveryMethod.ReliableOrdered)
    {
        peer.Send(writer, dm);
    }
    public void SendToAll(NetDataWriter writer, DeliveryMethod dm = DeliveryMethod.ReliableOrdered)
    {
        for (int i = 0; i < server.ConnectedPeerList.Count; i++)
            Send(server.ConnectedPeerList[i], writer, dm);
    }
    public void SendToAllExcept(NetPeer peer, NetDataWriter writer, DeliveryMethod dm = DeliveryMethod.ReliableOrdered)
    {
        for (int i = 0; i < server.ConnectedPeerList.Count; i++)
        {
            if (server.ConnectedPeerList[i] != peer)
                Send(server.ConnectedPeerList[i], writer, dm);
        }
    }
}
