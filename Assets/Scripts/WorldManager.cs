using LiteNetLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        public int netId;
        public bool isPlayer;
        public int authorityIndex;
        public bool active;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        public void Serialize(ref NetDataWriter writer)
        {
            writer.Put(netId);
            writer.Put(isPlayer);
            writer.Put(authorityIndex);
            writer.Put(active);

            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);

            writer.Put(rotation.x);
            writer.Put(rotation.y);
            writer.Put(rotation.z);
            writer.Put(rotation.w);

            if (active)
            {
                writer.Put(velocity.x);
                writer.Put(velocity.y);
                writer.Put(velocity.z);

                writer.Put(angularVelocity.x);
                writer.Put(angularVelocity.y);
                writer.Put(angularVelocity.z);
            }
        }
        public void Deserialize(NetDataReader reader)
        {
            authorityIndex = reader.GetInt();
            active = reader.GetBool();

            position = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            rotation = new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            if (active)
            {
                velocity = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
                angularVelocity = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
            }
            else
            {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Net Id: " + netId);
            sb.AppendLine("Authority: " + authorityIndex);

            sb.AppendLine("Active: " + active);
            sb.AppendLine("Pos: " + position);
            sb.AppendLine("Rot: " + rotation);

            return sb.ToString();
        }
    }

    public static WorldManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] float tickRate = 5; 
    [HideInInspector] public List<NetworkObject> cubes = new List<NetworkObject>();
    [HideInInspector] public Snapshot currentSnapshot;

    void Start()
    {
        Application.targetFrameRate = 60;
        cubes.AddRange(GetComponentsInChildren<NetworkObject>());

        for (int i = 0; i < cubes.Count; i++)
            cubes[i].Init((ushort)i);

        currentSnapshot = new Snapshot(cubes);
        StartCoroutine(broadcast());
    }

    public void OnSpawnPlayer(NetworkObject obj)
    {
        cubes.Add(obj);
        currentSnapshot.OnPlayerSpawn(obj);
    }

    IEnumerator broadcast()
    {
        while (true)
        {
            if (Client.getInstance != null)
            {
                CreateSnapshot(ref currentSnapshot);
                Client.getInstance.SendSnapshot(currentSnapshot);
            }

            yield return new WaitForSeconds(1f / tickRate);
        }
    }

    public Vector3 getPlayerPosition(int netId)
    {
        for (int i = 0; i < cubes.Count; i++)
        {
            if (cubes[i] is Player && cubes[i].netId == netId)
                return cubes[i].transform.position;
        }

        return Vector3.zero;
    }

    public void CreateSnapshot(ref Snapshot snapshot)
    {
        Vector3 origin = transform.position;

        for (int i = 0; i < cubes.Count; i++)
            snapshot.getState(cubes[i], i, origin);
    }
    public void ApplySnapshot(ref Snapshot newSnapshot)
    {
        Vector3 origin = transform.position;

        for (int i = 0; i < cubes.Count; i++)
        {
            if ((newSnapshot.states[i].isPlayer && cubes[i].isLocal) || cubes[i].hasLocalAuthority)
                continue;

            currentSnapshot.ApplyState(newSnapshot.states[i], cubes[i], origin);
        }
    }

    public Vector3 getOrigin() { return transform.position; }
}