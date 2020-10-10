using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour
{

    private Collider innerCollider;
    private Collider ringCollider;

    //private HoopSpawner spawner;


    private void Awake()
    {
        innerCollider = GetComponent<BoxCollider>();
        ringCollider = GetComponent<MeshCollider>();
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

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.tag == "Agent")
    //    {
    //        Destroy(this);
    //    }
    //}


    //private void OnDestroy()
    //{
    //    spawner.SpawnHoop();
    //    spawner.hoops.RemoveAll(item => item == null);
    //}
}
