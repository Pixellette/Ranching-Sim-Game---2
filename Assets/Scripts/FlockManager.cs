using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;
    public GameObject animalPrefab; 
    public int numAnimals; 
    public GameObject[] allAnimals; 
    public Vector3 moveLimits = new Vector3(5, 5, 5); // TODO: maybe doesn't work with ours???? 

    public Vector3 goalPos = Vector3.zero; 

    [Header ("Animal Settings")]
        [Range(0.0f, 5.0f)]
        public float minSpeed;

        [Range(0.0f, 5.0f)]
        public float maxSpeed;

        [Range(1.0f, 10.0f)]
        public float neighbourDistance;

        [Range(1.0f, 5.0f)]
        public float rotationSpeed;
        public float aheadDistance = 5;
        public float edgeThreshold = 5;
        public float pushBackDistance = 5;


    // Start is called before the first frame update
    void Start()
    {
        allAnimals = new GameObject[numAnimals];
        for(int i = 0; i < numAnimals; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-moveLimits.x, moveLimits.x), 
                                                                35.3f,
                                                                Random.Range(-moveLimits.z, moveLimits.z));
            
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
