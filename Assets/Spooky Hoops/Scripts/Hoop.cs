using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour
{

    private Collider innerCollider;

    //When the hoop spawns in, populate the box collider
    private void Awake()
    {
        innerCollider = GetComponent<BoxCollider>();
    }

    //Return the hoop up vector to be checked in witch.cs
    public Vector3 HoopUpVector
    {
        get
        {
            return innerCollider.transform.up;
        }
    }

    //Return the hoop center position to be checked in witch.cs
    public Vector3 RingCenterPosition
    {
        get
        {
            return innerCollider.transform.position;
        }
    }
}
