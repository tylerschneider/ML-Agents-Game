using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

public class HoopSpawner : MonoBehaviour
{
    //Variables for the hoop gameobject, the number of hoops spawned, the spawning area for the hoops, and the hoops list
    public GameObject hoop;
    public int hoopNum;
    public Collider spawnZone;
    public List<GameObject> hoops;

    //When the script starts, start generating hoops for the level
    private void Start()
    {
        LevelGeneration();
    }

    //Method used to generate the hoops in the level
    private void LevelGeneration()
    {
        //uses a for loop that only progresses if the position of the hoop is valid
        for(int i = 0; i < hoopNum;)
        {
           bool positionGood = SpawnHoop();
           if(positionGood == true)
           {
                i++;
           }
        }
    }

    //The method used for spawning in hoops and returning a bool value to check if the position is valid
    public bool SpawnHoop()
    {
        //Generate a random position within the spawnzone collider
        Vector3 hoopPosition = new Vector3(UnityEngine.Random.Range(spawnZone.bounds.min.x, spawnZone.bounds.max.x) , UnityEngine.Random.Range(spawnZone.bounds.min.y, spawnZone.bounds.max.y), UnityEngine.Random.Range(spawnZone.bounds.min.z, spawnZone.bounds.max.z));

        //if the hoops array has any values in it, check to see if the postions are valid
        if (hoops.Any())
        {
            //Goes through each hoop in the list
            foreach (GameObject hoop in hoops)
            {
                //if the hoops are too close to eachother, then return false to generate a new position
                if(Vector3.Distance(hoop.transform.position, hoopPosition) > 15)
                {
                    return false;
                }
            }
        }

        //Checks how many colliders are in the area for the hoop position
        Collider[] colliders = Physics.OverlapSphere(hoopPosition, 0.05f);

        //If there are more than two colliders in the spawning area, then return false to generate a new position
        if (colliders.Length >= 2)
        {
            return false;
        }

        //Add a new hoop gameobject to the hoop list if it passes the other checks
        hoops.Add(Instantiate(hoop, hoopPosition, UnityEngine.Random.rotation, transform));

        //return true to progress through the loop of level generation
        return true;
    }

    //Method used for reseting the hoops for starting a new game or training
    public void ResetHoops()
    {
        //Destroy each gameobject in the hoops array
        foreach(GameObject hoop in hoops)
        {
            Destroy(hoop);
        }

        //Clean up the list and generate a new level
        hoops.Clear();
        LevelGeneration();
    }
}
