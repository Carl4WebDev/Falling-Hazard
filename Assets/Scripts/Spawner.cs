using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Transform[] spawnPoints;
    public GameObject[] hazards;

    private float timeBtwSpawns;
    public float startTimeBtwSpawns;

    public float minTimeBetweenSpawns;
    public float decrease;

    public GameObject player;

    [Header("Object Pooling")]
    public int poolSize = 5;

    private Queue<GameObject>[] pools;

    void Start()
    {
        pools = new Queue<GameObject>[hazards.Length];
        for (int i = 0; i < hazards.Length; i++)
        {
            pools[i] = new Queue<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = Instantiate(hazards[i], transform);
                obj.SetActive(false);
                SetupReturnCallback(obj, i);
                pools[i].Enqueue(obj);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            if (timeBtwSpawns <= 0)
            {

                Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                int hazardIndex = Random.Range(0, hazards.Length);

                SpawnFromPool(hazardIndex, randomSpawnPoint.position);

                if (startTimeBtwSpawns > minTimeBetweenSpawns)
                {
                    startTimeBtwSpawns -= decrease;
                }

                timeBtwSpawns = startTimeBtwSpawns;

             }
            else
            {
                timeBtwSpawns -= Time.deltaTime;
            }
        }
    }

    private GameObject SpawnFromPool(int index, Vector3 position)
    {
        Queue<GameObject> pool = pools[index];
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Pool exhausted — expand
            obj = Instantiate(hazards[index], transform);
            SetupReturnCallback(obj, index);
        }

        obj.transform.position = position;
        obj.SetActive(true);

        // Reset enemy state
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ResetState();
        }

        return obj;
    }

    private void SetupReturnCallback(GameObject obj, int poolIndex)
    {
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null)
        {
            int capturedIndex = poolIndex;
            enemy.returnToPool = (returnedObj) =>
            {
                returnedObj.SetActive(false);
                pools[capturedIndex].Enqueue(returnedObj);
            };
        }
    }
}
