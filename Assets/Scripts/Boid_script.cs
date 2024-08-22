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

    [Header ("Boid Rule Settings")]
        [SerializeField] float speed; 
        [SerializeField] bool turning = false; 
        [SerializeField] float checkRange = 5.0f;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ds = target.GetComponent<Drive>(); // This is costly to do so do it once

        speed = Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        // Seek(target.transform.position);
        // Wander();

        if(Random.Range(0, 100) < 10)
        {
            ApplyRules();
        }
        else { 
            Wander();
        }
    }



    // ============================================================
    //                         Navigation Methods
    // ============================================================

    void Seek(Vector3 location)
    {
        NavMeshHit navHit;

        // Check if the target location is on the NavMesh within a given distance (1.0f here).
        if (NavMesh.SamplePosition(location, out navHit, 1.0f, NavMesh.AllAreas))
        {
            // If valid, set the agent's destination to this position
            agent.destination = navHit.position;
        }
        else
        {
            Debug.Log("Location was out of bounds- rerouting!");
            agent.destination = SetDestinationBehind(); 
        }
    } // End of Seek Method


    Vector3 SetDestinationBehind()
    {
        // Get the current forward direction of the boid
        Vector3 currentForward = transform.forward;

        // Calculate the opposite direction (behind the boid)
        Vector3 oppositeDirection = -currentForward;

        // Randomly rotate to within 60 degrees of behind 
        float angle = Random.Range(-30.0f, 30.0f);
        Quaternion rotation = Quaternion.Euler(0, angle, 0); 

        // Apply the rotation to the opposite direction 
        Vector3 newDirection = rotation * oppositeDirection;

        // Set a destination along this new direction
        float distance = 10.0f; 
        Vector3 destination = transform.position + newDirection * distance; 

        return destination;
    }


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


    // ============================================================
    //                           Boid Rules 
    // ============================================================

    void ApplyRules1() 
    {
        GameObject[] gos; 
        gos = FlockManager.FM.allAnimals; 
        
        int groupSize = 10;
        Vector3 vcentre = Vector3.zero; // average centre of group 
        Vector3 vavoid = Vector3.zero; // avoid others 
        float gSpeed = 0.01f;
        float nDistance; 

        foreach(GameObject go in gos)
        {
            if (go != this.gameObject)
            { 
                nDistance = Vector3.Distance(go.transform.position, this.transform.position);
                if (nDistance <= FlockManager.FM.neighbourDistance)
                {
                    vcentre += go.transform.position;
                    groupSize++; 

                    if ( nDistance < checkRange) 
                    {
                        vavoid = vavoid +(this.transform.position - go.transform.position);
                    } 

                    Boid_script anotherFlock = go.GetComponent<Boid_script>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if ( groupSize > 0)
        {
            vcentre = vcentre/groupSize + this.transform.position;
            speed = gSpeed/groupSize; 
            if(speed > FlockManager.FM.maxSpeed)
            {
                speed = FlockManager.FM.maxSpeed;
            }

            Vector3 direction = (vcentre + vavoid) - transform.position; 
            if(direction!= Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        Quaternion.LookRotation(direction), 
                                        FlockManager.FM.rotationSpeed * Time.deltaTime);
            }
        }
    }

    void ApplyRules()
    {
        GameObject[] gos = FlockManager.FM.allAnimals;

        Vector3 vcentre = Vector3.zero;  // Average center of group
        Vector3 vavoid = Vector3.zero;   // Vector to avoid others
        Vector3 vheading = Vector3.zero; // Average heading of the group
        int groupSize = 0;

        foreach (GameObject go in gos)
        {
            if (go != this.gameObject)
            {
                float nDistance = Vector3.Distance(go.transform.position, this.transform.position);

                if (nDistance <= FlockManager.FM.neighbourDistance)
                {
                    // Accumulate the position for the center of the group
                    vcentre += go.transform.position;
                    vheading += go.transform.forward; // Accumulate the heading direction
                    groupSize++;

                    // If too close, calculate the avoid vector
                    if (nDistance < 1.0f)
                    {
                        vavoid += (this.transform.position - go.transform.position);
                    }
                }
            }
        }

        if (groupSize > 0)
        {
            // Calculate the average center position and heading direction of the group
            vcentre = vcentre / groupSize;
            vheading = vheading.normalized;

            // Calculate the direction to move towards the center and the goal ahead
            Vector3 direction = (vcentre + vavoid) - transform.position;

            // Project a goal point ahead based on the average heading
            Vector3 goalAhead = vcentre + vheading * FlockManager.FM.aheadDistance; // aheadDistance can be a configurable parameter

            // Adjust the final direction to be towards both the center and the goal ahead
            direction = (direction + (goalAhead - transform.position)).normalized;

            // Use the Seek method to move towards the calculated position
            if (direction != Vector3.zero)
            {
                Seek(transform.position + direction * FlockManager.FM.neighbourDistance);
            }
        }
    }



}
