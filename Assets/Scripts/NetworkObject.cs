using System.Collections;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public Renderer _renderer;
    public Rigidbody rb;
    public Vector3 positionError = Vector3.zero;
    public Quaternion rotationError = Quaternion.identity;

    Vector3 previousPosition;
    Color defaultColour;

    public int netId;
    public int clientAuthority = -1;
    [SerializeField] float _priority;
    public float priority
    {
        get
        {
            return _priority;
        }
        set
        {
            if (!hasLocalAuthority)
                return;

            _priority = value;
        }
    }

    Coroutine resetAuthorityRoutine;

    public bool isLocal
    {
        get
        {
            if (Client.getInstance != null)
                return netId == Client.getInstance.localId;
            return false;
        }
    }
    public bool hasLocalAuthority
    {
        get
        {
            if (Client.getInstance != null)
                return clientAuthority == Client.getInstance.localId;
            return false;
        }
    }

    public virtual void Init(int netId)
    {
        this.netId = netId;
        previousPosition = transform.position;
        defaultColour = _renderer.materials[0].color;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, previousPosition) > .1f)
            priority++;
    }

    public void setPriority(int priority)
    {
        this.priority = priority;
    }

    public void Interact(int remoteNetId)
    {
        if (this is Player)
            return;

        if (remoteNetId == -1)
        {
            clientAuthority = -1;
            _renderer.materials[0].color = defaultColour;
        }
        else
        {
            if (clientAuthority != -1)
                return;

            clientAuthority = remoteNetId;
            _renderer.materials[0].color = new Color(remoteNetId * 100, remoteNetId * 100, remoteNetId * 100);
            priority += 100;
            if (resetAuthorityRoutine != null)
                StopCoroutine(resetAuthorityRoutine);

            resetAuthorityRoutine = StartCoroutine(resetClientAuthority());
        }
    }

    IEnumerator resetClientAuthority()
    {
        yield return new WaitForSeconds(5f - (Client.getInstance.getPing() + (1f / 60f)));
        Interact(-1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasLocalAuthority)
            return;

        NetworkObject obj = collision.collider.GetComponent<NetworkObject>();
        if (obj == null || obj is Player)
            return;

        priority += 50;
        obj.Interact(this is Player ? netId : clientAuthority);
    }

    public void moveWithSmoothing(Vector3 pos, Quaternion rot)
    {
        if (moveSmoothlyRoutine != null)
            StopCoroutine(moveSmoothlyRoutine);
        if (rotateSmoothlyRoutine != null)
            StopCoroutine(rotateSmoothlyRoutine);

        moveSmoothlyRoutine = StartCoroutine(moveSmoothly(pos));
        rotateSmoothlyRoutine = StartCoroutine(rotateSmoothly(rot.normalized));
    }
    Coroutine moveSmoothlyRoutine;
    Coroutine rotateSmoothlyRoutine;
    IEnumerator moveSmoothly(Vector3 toPos)
    {
        while (transform.position != toPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, toPos, 5f * Time.deltaTime);
            yield return null;
        }
    }
    IEnumerator rotateSmoothly(Quaternion toRot)
    {
        while (transform.rotation != toRot)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, toRot, 5f * Time.deltaTime);
            yield return null;
        }
    }
}