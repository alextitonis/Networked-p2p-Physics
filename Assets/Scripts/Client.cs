using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public static Client getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] string ip;
    [SerializeField] int port;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Text localIdText;
    [SerializeField] Text dataCountText;

    EventBasedNetListener listener;
    NetManager client;
    NetPeer serverPeer;

    long _dataCount = 0;
    long dataCount
    {
        get { return _dataCount; }
        set
        {
            _dataCount = value;
            dataCountText.text = "Data Count: " + _dataCount + " bytes";
        }
    }
    public int localId;

    void Start()
    {
        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect(ip, port, "test");
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
        StartCoroutine(clearDataCount());
    }

    private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        ushort packetId = reader.GetUShort();

        if (packetId == 0)
        {
            serverPeer = peer;
            localId = reader.GetInt();
            localIdText.text = "Local ID: " + localId;
            Vector3 pos = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
            NetworkObject obj = go.GetComponent<Player>();
            obj.Init(localId);
            WorldManager.getInstance.OnSpawnPlayer(obj);
        }
        else if (packetId == 1)
        {
            int netId = reader.GetInt();
            Vector3 pos = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
            NetworkObject obj = go.GetComponent<Player>();
            obj.Init(netId);
            WorldManager.getInstance.OnSpawnPlayer(obj);
        }
        else if (packetId == 2)
        {
            dataCount += reader.RawDataSize;
            Snapshot sn = WorldManager.getInstance.currentSnapshot;
            sn.Deserialize(reader);
            WorldManager.getInstance.ApplySnapshot(ref sn);
        }

        reader.Recycle();
    }

    public int getPing() { return serverPeer.Ping; }

    IEnumerator clearDataCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            dataCount = 0;
        }
    }

    void Update()
    {
        client.PollEvents();
    }

    public void Send(NetDataWriter writer, DeliveryMethod dm = DeliveryMethod.ReliableOrdered)
    {
        if (serverPeer == null)
            return;

        serverPeer.Send(writer, dm);
    }

    public void SendSnapshot(Snapshot snapshot)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((ushort)2);
        snapshot.Serialize(ref writer, out int count);
        if (count <= 0) return;
        Send(writer);
    }
}
