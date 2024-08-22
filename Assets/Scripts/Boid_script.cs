using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boid_script : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;  // find the Target entity
    Drive ds;


    [Header ("Wander Settings")]
        [SerializeField] float wanderRadius = 10;
        [SerializeField] public float wanderDistance = 20;
        [SerializeField] float wanderJitter = 1; 
        [SerializeField] Vector3 wanderTarget = Vector3.zero; // cannot be local as needs to remember between calls



    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ds = target.GetComponent<Drive>(); // This is costly to do so do it once
    }

    // Update is called once per frame
    void Update()
    {
        // Seek(target.transform.position);
        Wander();
    }



    // ============================================================
    //                         Navigation Methods
    // ============================================================

        void Seek(Vector3 location)
    {
        agent.destination = location;
    } // End of Seek Method

    void Wander()
    {
        wanderTarget += new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f) * wanderJitter,   // X
                                    0,                                                      // Y
                                    UnityEngine.Random.Range(-1.0f, 1.0f) * wanderJitter);  // Z

        // Move the target back onto the circle (currently ON the Agent)
        wanderTarget.Normalize(); // get a better number
        wanderTarget *= wanderRadius; // push it out to the right length

        // Move circle to *infront* of Agent
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance); // local because we are imagining the Agent as the center of the world
        Vector3 targetWorld = this.gameObject.transform.TransformPoint(targetLocal); // Now convert to world location 

        // Finally Seek the target location
        Seek(targetWorld);
    }
}
