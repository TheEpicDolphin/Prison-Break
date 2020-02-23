using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    bool locked;
    Transform target;
    readonly float camSpeed = 0.05f;
    // Start is called before the first frame update
    void Start()
    {
        locked = true;
        target = Game.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            locked = false;
            transform.position += camSpeed * new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            locked = false;
            transform.position += camSpeed * new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            locked = false;
            transform.position += camSpeed * new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            locked = false;
            transform.position += camSpeed * new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            locked = true;
        }

        if (locked)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, -10.0f), 5.0f * Time.deltaTime);
        }
    }

    public void SetTarget(Transform targetTrans)
    {
        target = targetTrans;
        locked = true;
    }
}
