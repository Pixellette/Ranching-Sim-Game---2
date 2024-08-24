using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;

    [Header ("Spawn Settings ")]
        public GameObject animalPrefab; 

        [Range(1,200)]
        public int spawnRange;
        public int numAnimals; 
        public GameObject[] allAnimals; 

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
        [Range(1.0f, 10.0f)]
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


    [Header ("Animal Awareness Settings")]
        [Range(30.0f, 90.0f)]
        public float viewAngle;

        [Range(30.0f, 90.0f)]
        public float viewRange;

        [Range(1.0f, 30.0f)]
        public float senseRange; 


    void Start()
    {
        allAnimals = new GameObject[numAnimals];
        for(int i = 0; i < numAnimals; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-spawnRange, spawnRange), 
                                                                35.3f,
                                                                Random.Range(-spawnRange, spawnRange));
            
            
            allAnimals[i] = Instantiate(animalPrefab, pos, Quaternion.identity);

        }

        FM = this;
    }


    void Update()
    {
        
    }
}
