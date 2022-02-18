
using System.Collections;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] GameObject client, server;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Instantiate(client, transform);
            Destroy(this);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Instantiate(server, transform);
            StartCoroutine(spawnClient());
        }
    }

    IEnumerator spawnClient()
    {
        yield return new WaitForSeconds(1f);
        Instantiate(client, transform);
        Destroy(this);
    }
}
