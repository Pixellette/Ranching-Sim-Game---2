using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;

    [Header ("Spawn Settings ")]
        public GameObject sheepPrefab; 
        [Range(1,200)]
        public int spawnRangeSheep;
        public int numSheep; 
        public GameObject[] allSheep; 

        public GameObject cowPrefab; 


        [Range(1,200)]
        public int spawnRangeCow;
        public int numCow; 
        public GameObject[] allCow; 

    [Header ("Behaviour Settings")]
        [Range(1, 5)]
        public int minWait;

        [Range(1, 10)]
        public int maxWait;

    [Header ("Wander Settings")]
        [SerializeField] public float wanderRadius = 10;
        [SerializeField] public float wanderDistance = 20;
        [SerializeField] public float wanderJitter = 1; 


    [Header ("Flocking Settings")]
        [Range(1.0f, 30.0f)]
        public float neighbourDistance;
        public float aheadDistance = 5;
        
        [Range(1,100)]
        public int flockingChance;

        [Range(0.01f,5.0f)]
        public float seperationWeight;

        [Range(0.01f,5.0f)]
        public float alignmentWeight;

        [Range(0.01f,5.0f)]
        public float cohesionWeight;
        
        [Range(0.01f, 5.0f)]
        public float fleeWeight;


    [Header ("Animal Awareness Settings")]
        [Range(30.0f, 90.0f)]
        public float viewAngle;

        [Range(30.0f, 90.0f)]
        public float viewRange;

        [Range(1.0f, 30.0f)]
        public float senseRange; 


    void Start()
    {
        // Create parent GameObjects for sheep and cows
        GameObject sheepParent = new GameObject("SheepParent");
        GameObject cowParent = new GameObject("CowParent");

        // Create and position all sheep
        allSheep = new GameObject[numSheep];
        for (int i = 0; i < numSheep; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-spawnRangeSheep, spawnRangeSheep),
                                                                50f, 
                                                                Random.Range(-spawnRangeSheep, spawnRangeSheep));

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit))
            {
                pos.y = hit.point.y;
            }

            allSheep[i] = Instantiate(sheepPrefab, pos, Quaternion.identity);
            allSheep[i].transform.parent = sheepParent.transform; // Set parent
        }

        // Create and position all cows
        allCow = new GameObject[numCow];
        for (int i = 0; i < numCow; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-spawnRangeCow, spawnRangeCow),
                                                                50f,
                                                                Random.Range(-spawnRangeCow, spawnRangeCow));

            if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit))
            {
                pos.y = hit.point.y;
            }

            allCow[i] = Instantiate(cowPrefab, pos, Quaternion.identity);
            allCow[i].transform.parent = cowParent.transform; // Set parent
        }

        FM = this;
    }




    void Update()
    {
        
    }
}
