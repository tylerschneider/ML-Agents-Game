using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

public class HoopSpawner : MonoBehaviour
{
    public GameObject hoop;
    public int hoopNum;
    public Collider spawnZone;

    public List<GameObject> hoops;

    private void Start()
    {
        LevelGeneration();
    }

    private void LevelGeneration()
    {
        for(int i = 0; i < hoopNum;)
        {
           bool positionGood = SpawnHoop();
           if(positionGood == true)
           {
                i++;
           }
        }
    }


    public bool SpawnHoop()
    {
        Vector3 hoopPosition = new Vector3(UnityEngine.Random.Range(spawnZone.bounds.min.x, spawnZone.bounds.max.x) , UnityEngine.Random.Range(spawnZone.bounds.min.y, spawnZone.bounds.max.y), UnityEngine.Random.Range(spawnZone.bounds.min.z, spawnZone.bounds.max.z));

        if (hoops.Any())
        {
            foreach (GameObject hoop in hoops)
            {
                if(Vector3.Distance(hoop.transform.position, hoopPosition) > 10)
                {
                    return false;
                }
                Collider[] colliders = Physics.OverlapSphere(hoopPosition, 0.05f);

                if(colliders.Length >= 2)
                {
                    Debug.Log("Collision presented");
                    return false;
                }
            }
        }

        hoops.Add(Instantiate(hoop, hoopPosition, UnityEngine.Random.rotation, transform));
        return true;
    }

    public void ResetHoops()
    {
        foreach(GameObject hoop in hoops)
        {
            Destroy(hoop);
        }

        hoops.Clear();
        LevelGeneration();
    }
}
