using LiteNetLib;
using UnityEngine;

public class Player : NetworkObject
{
    [SerializeField] float speed = 5f;

    public override void Init(int netId)
    {
        base.Init(netId);
        clientAuthority = netId;
        _renderer.materials[0].color = new Color(netId * 100, netId * 100, netId * 100);

        if (isLocal)
            Camera.main.GetComponent<dg_simpleCamFollow>().target = transform;
    }

    private void Update()
    {
        if (!isLocal)
            return;

        Vector3 inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        rb.velocity = (inputVector * speed) + new Vector3(0, rb.velocity.y, 0);
    }
}