using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoop : MonoBehaviour
{
    private int increase = 0;
    private float xRot;
    private float yRot;

    private void Start()
    {
        
    }
    //private void FixedUpdate()
    //{
    //    increase++;
    //    if(increase < 360)
    //    {
    //        transform.localRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, increase);
    //    }
    //    else
    //    {
    //        increase = 0;
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        
    }
}
