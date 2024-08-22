using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;
    public GameObject animalPrefab; 
    public int numAnimals; 
    public GameObject[] allAnimals; 
    public Vector3 moveLimits = new Vector3(100, 0, 100); // TODO: maybe doesn't work with ours???? 
    public int spawnRange = 100;

    public Vector3 goalPos = Vector3.zero; 

    [Header ("Speed Settings")]
        [Range(0.0f, 5.0f)]
        public float minSpeed;

        [Range(0.0f, 5.0f)]
        public float maxSpeed;

        [Range(1.0f, 5.0f)]
        public float rotationSpeed;


    [Header ("Movement Settings")]
        [Range(1.0f, 10.0f)]
        public float neighbourDistance;
        public float aheadDistance = 5;
        public float edgeThreshold = 5;
        public float pushBackDistance = 5;

    [Header ("View Settings")]
        [Range(30.0f, 90.0f)]
        public float viewAngle;

        [Range(30.0f, 90.0f)]
        public float viewRange;

    // Start is called before the first frame update
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
        goalPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
