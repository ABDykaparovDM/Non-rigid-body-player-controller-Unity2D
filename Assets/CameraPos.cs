using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPos : MonoBehaviour
{
    Vector3 PzeroPose;
    public Vector3 Offset;
    private float followFaster;

    void Start()
    {
        Offset = new Vector3(0f, 0f, -10f);
        transform.position = GameObject.Find("player").transform.position + Offset;
        followFaster = 8f;
    }

    void FixedUpdate()
    {
        PzeroPose = GameObject.Find("player").transform.position + Offset;
        transform.position = new Vector3(Mathf.Lerp(transform.position.x, PzeroPose.x, Time.deltaTime * followFaster),
                                         Mathf.Lerp(transform.position.y, PzeroPose.y, Time.deltaTime * followFaster), 
                                         0) + Offset;
    }
}
