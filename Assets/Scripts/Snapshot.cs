using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static WorldManager;

[System.Serializable]
public class Snapshot
{
    public State[] states;

    public Snapshot(List<NetworkObject> cubes)
    {
        states = new State[cubes.Count];
        for (int i = 0; i < cubes.Count; i++)
        {
            states[i] = new State();
            getState(cubes[i], i, getInstance.getOrigin());
        }
    }

    public void OnPlayerSpawn(NetworkObject obj)
    {
        State[] temp = states;
        states = new State[temp.Length + 1];
        for (int i = 0; i < temp.Length; i++)
            states[i] = temp[i];
        states[states.Length - 1] = new State();
        getState(obj, states.Length - 1, getInstance.getOrigin());
    }

    public void getState(NetworkObject obj, int index, Vector3 origin)
    {
        if (index < 0 || index > states.Length - 1)
            return;

        State state = new State();
        state.netId = obj.netId;
        state.isPlayer = obj is Player;
        state.active = !obj.rb.IsSleeping();

        state.authorityIndex = obj.clientAuthority;

        state.position = obj.transform.position - origin;
        state.rotation = obj.transform.rotation;
        state.velocity = obj.rb.velocity;
        state.angularVelocity = obj.rb.angularVelocity;

        states[index] = state;
        obj.priority = 0;
    }
    public void ApplyState(State state, NetworkObject obj, Vector3 origin, bool smoothing = false)
    {
        if (state.active && obj.rb.IsSleeping())
            obj.rb.WakeUp();
        else if (!state.active && !obj.rb.IsSleeping())
            obj.rb.Sleep();

        obj.Interact(state.authorityIndex);

        Vector3 pos = state.position + origin;
        Quaternion rot = state.rotation;

        if (smoothing)
            obj.moveWithSmoothing(pos, rot);
        else
        {
            obj.transform.position = pos;
            obj.transform.rotation = rot.normalized;
        }

        obj.rb.velocity = state.velocity;
        obj.rb.angularVelocity = state.angularVelocity;
    }

    public void Serialize(ref NetDataWriter writer, out int count)
    {
        count = 0;
        for (int i = 0; i < states.Length; i++)
        {
            if (!getInstance.cubes[i].hasLocalAuthority)
                continue;

            count++;
        }
        writer.Put(count);

        for (int i = 0; i < states.Length; i++)
        {
            if (!getInstance.cubes[i].hasLocalAuthority)
                continue;

            states[i].Serialize(ref writer);
        }
    }
    public void Deserialize(NetDataReader reader)
    {
        int length = reader.GetInt();
        for (int i = 0; i < length; i++)
        {
            int netId = reader.GetInt();
            bool isPlayer = reader.GetBool();
            updateState(netId, isPlayer, reader);
        }
    }

    void updateState(int netId, bool isPlayer, NetDataReader reader)
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].netId == netId && states[i].isPlayer == isPlayer)
            {
                states[i].Deserialize(reader);
                return;
            }
        }
    }
    public bool isInactive(int index, Snapshot prev)
    {
        if (prev == null || prev.states == null || prev.states.Length == 0)
            return false;

        if (!states[index].active && !prev.states[index].active)
            return true;

        return false;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < states.Length; i++)
            sb.AppendLine(states[i].ToString());

        return sb.ToString(); ;
    }
}