using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Renderer wormHole;
    public float wormHoleFadeSpeed;
    public GameObject coverPrefab;
    public int coverCount;
    public float randomHorizontalSize;
    public float randomVerticalSize;
    public GameObject[] spawnerPrefab;
    public int spawnerCount;
    public BoxCollider planetSpawnRange;
    public GameObject[] planetPrefab;
    private BoxCollider col;
   // public List<EnemySpawner> spawners = new List<EnemySpawner>();
    public bool newLevel = false;
    public bool generatingLevel = false;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<BoxCollider>();
        GenerateLevel();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    //for each spawner
    //    foreach(EnemySpawner spawner in spawners)
    //    {
    //        //check if the spawner has spawned the total number of enemies it should and that none of the enemies are alive
    //        if(spawner.enemiesSpawned >= spawner.totalEnemies && spawner.transform.childCount == 0)
    //        {
    //            //if all enemies spawned and no enemies alive, set to true, then check the next spawner
    //            newLevel = true;
    //        }
    //        else
    //        {
    //            //if any spawner still needs to spawn enemies or enemies are alive, do not generate a new level and stop checking spawners
    //            newLevel = false;
    //            break;
    //        }
    //    }

    //    if(newLevel == true)
    //    {
    //        newLevel = false;
    //        generatingLevel = true;
    //        wormHole.gameObject.GetComponent<AudioSource>().Play();
    //    }

    //    float wormHoleAlpha = wormHole.material.GetFloat("_Alpha");

    //    if (generatingLevel == true && wormHoleAlpha < 1)
    //    {
    //        //if wormhole alpha + fade greater than one, set to one, otherwise just add fade
    //        wormHoleAlpha = wormHoleAlpha + wormHoleFadeSpeed > 1 ? 1 : wormHoleAlpha + wormHoleFadeSpeed;
    //        //set the number
    //        wormHole.material.SetFloat("_Alpha", wormHoleAlpha);
    //    }
    //    else if(generatingLevel == true && wormHoleAlpha == 1)
    //    {
    //        //generate a level and set bool to false
    //        generatingLevel = false;
    //        GenerateLevel();
    //    }
    //    else if(generatingLevel == false && wormHoleAlpha > 0)
    //    {
    //        //if wormhole alpha - fade less than zero, set to zero, otherwise just subtract fade
    //        wormHoleAlpha = wormHoleAlpha - wormHoleFadeSpeed < 0 ? 0 : wormHoleAlpha - wormHoleFadeSpeed;
    //        //set the number
    //        wormHole.material.SetFloat("_Alpha", wormHoleAlpha);
    //    }
    //}

    void GenerateLevel()
    {
        ////destroy any objects and spawners
        //foreach (Transform child in transform)
        //{
        //    //remove from list if a spawner
        //    if(child.GetComponent<EnemySpawner>())
        //    {
        //        spawners.Remove(child.GetComponent<EnemySpawner>());
        //    }
        //    //destroy the object
        //    Destroy(child.gameObject);
        //}

        //generate a number of covers equal to covercount
        for (int i = 0; i < coverCount; i++)
        {
            //get a random position
            Vector3 pos = new Vector3(
                Random.Range(col.bounds.min.x, col.bounds.max.x),
                50,
                Random.Range(col.bounds.min.z, col.bounds.max.z)
            );

            //raycast down to make sure object is on ground
            RaycastHit hit;
            if (Physics.Raycast(pos, Vector3.down, out hit, 1 << LayerMask.NameToLayer("Environment")))
            {
                pos = hit.point;
                //fix bug where object doesn't always spawn on ground level
                pos.y = hit.collider.transform.position.y;

                //instantiate cover at the random position on ground
                GameObject go = Instantiate(coverPrefab, pos, coverPrefab.transform.rotation, transform);
                //set a random size for the cover
                go.transform.localScale += new Vector3(
                    Random.Range(-randomHorizontalSize, randomHorizontalSize),
                    Random.Range(-randomVerticalSize, randomVerticalSize),
                    0
                );
            }
        }

        ////generate a number of spawners equal to spawnercount
        //for (int i = 0; i < spawnerCount; i++)
        //{
        //    //choose a random type of spawner/enemy from the array
        //    int type = Random.Range(0, spawnerPrefab.Length);
        //    GameObject go = Instantiate(spawnerPrefab[type], transform);
        //    //position at level manager origin
        //    go.transform.position += transform.position;
        //    //add spawner to list
        //    spawners.Add(go.GetComponent<EnemySpawner>());
        //}

        //randomize the background planet
        //choose a random type of planet from the array
        int planetType = Random.Range(0, planetPrefab.Length);
        //get a random position
        Vector3 planetPos = new Vector3(
            Random.Range(planetSpawnRange.bounds.min.x, planetSpawnRange.bounds.max.x),
            Random.Range(planetSpawnRange.bounds.min.y, planetSpawnRange.bounds.max.y),
            Random.Range(planetSpawnRange.bounds.min.z, planetSpawnRange.bounds.max.z)
        );
        GameObject planet = Instantiate(planetPrefab[planetType], planetPos, Random.rotation, transform);
        foreach(Transform child in planet.transform)
        {
            //give each component of the planet a random color
            child.GetComponent<Renderer>().material.color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
                );
        }
    }
}
