using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    [SerializeField] float force = 100f;
    [SerializeField] NetworkObject obj;

    List<Rigidbody> caughtRbs = new List<Rigidbody>();

    private void FixedUpdate()
    {
        for (int i = 0; i < caughtRbs.Count; i++)
        {
            caughtRbs[i].velocity = (transform.position - (caughtRbs[i].transform.position + caughtRbs[i].centerOfMass)) * force * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player" && other.GetComponent<Rigidbody>() && other.GetComponent<NetworkObject>())
        {
            NetworkObject obj = other.GetComponent<NetworkObject>();
            if (obj.clientAuthority != this.obj.netId && obj.clientAuthority != -1)
            {
                return;
            }

            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (!caughtRbs.Contains(rb))
            {
                caughtRbs.Add(rb);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player" && other.GetComponent<Rigidbody>())
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (caughtRbs.Contains(rb))
            {
                caughtRbs.Remove(rb);
            }
        }
    }
}