using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour
{

    private Collider innerCollider;

    private void Awake()
    {
        innerCollider = GetComponent<BoxCollider>();
    }

    public Vector3 HoopUpVector
    {
        get
        {
            return innerCollider.transform.up;
        }
    }

    public Vector3 RingCenterPosition
    {
        get
        {
            return innerCollider.transform.position;
        }
    }
}
