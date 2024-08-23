using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class Boid_script : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;  // find the Target entity
    Drive ds;

    [SerializeField] bool viewCooldown = false; 
    
    [SerializeField] bool isFleeing = false;


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
        target = GameObject.FindWithTag("Predator");
    }

    // Update is called once per frame
    void Update()
    {
        if (!viewCooldown)
        {
            if (CanSeeTarget())
            {
                Flee(target.transform.position);
                viewCooldown = true; 
                isFleeing = true;
                Invoke("ViewBehaviourCooldown", 5);
            }
            else if (TargetInRange(FlockManager.FM.senseRange))
            {
                // Debug.Log("Can SENSE target!");
                Flee(target.transform.position);
                viewCooldown = true; 
                isFleeing = true;
                Invoke("ViewBehaviourCooldown", 5);
            }
            else if (isFleeing && (TargetInRange(FlockManager.FM.viewRange) || TargetInRange(FlockManager.FM.senseRange)))
            {
                Flee(target.transform.position);
            }
            else if (isFleeing && (!TargetInRange(FlockManager.FM.viewRange) || TargetInRange(FlockManager.FM.senseRange)))
            {
                // Debug.Log("Target left range");
                isFleeing = false;
            }
            else 
            {
                if(Random.Range(0, 100) < 10)
                {
                    if(Random.Range(0, 100) < 30){
                        ApplyRules();
                    }
                    else { 
                        Wander();
                    }
                }
            }
        }
        else {
            Flee(target.transform.position);

            if (!TargetInRange(FlockManager.FM.viewRange))
            {
                // Debug.Log("Target left view range");
                // viewCooldown = false;
                isFleeing = false;
            }
        }
    }



    // ============================================================
    //                         Navigation Methods
    // ============================================================

    void Seek(Vector3 location)
    {
        RaycastHit hit;
        Vector3 direction = (location - agent.transform.position).normalized;
        float distance = Vector3.Distance(agent.transform.position, location);

        // Cast a ray to detect the obstacle in the path
        if (Physics.Raycast(agent.transform.position, direction, out hit, distance))
        {
            // Calculate the closest point on the obstacle's surface to the agent
            Vector3 closestPoint = hit.point - direction * agent.radius;

            // Move to the closest point on the obstacle edge
            agent.SetDestination(closestPoint);
        }
        else
        {
            // No obstacle, move directly to the target location
            agent.SetDestination(location);
        }
    } // End of Seek Method


        void Flee(Vector3 location)
    {
        // Debug.Log("Running away!");
        Vector3 fleeVector = location - this.transform.position;
        Vector3 fleeLocation = this.transform.position - fleeVector;

        Seek(fleeLocation);
    }


    void Wander()
    {
        // Debug.Log("Wandering...");
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
    //                       Vision Methods
    // ============================================================

    bool CanSeeTarget()
    {
        RaycastHit raycastInfo;

        // Calculate the direction from the agent to the target
        Vector3 rayToTarget = target.transform.position - this.transform.position;


        // Check the distance to the target
        if (TargetInRange(FlockManager.FM.viewRange))
        {
            // Calculate the angle between the agent's forward direction and the direction to the target
            float lookAngle = Vector3.Angle(this.transform.forward, rayToTarget);

            // Check if the target is within the field of view and if the raycast hits the target
            if ((Mathf.Abs(lookAngle) <= FlockManager.FM.viewAngle) && Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo))
            {
                
                if (raycastInfo.transform.gameObject.tag == "Predator")
                {
                    // Debug.Log("Can SEE target");
                    return true;
                }
            }
        }

        return false;
    }

    bool TargetInRange(float range) 
    {
        RaycastHit raycastInfo;

        // Calculate the direction from the agent to the target
        Vector3 rayToTarget = target.transform.position - this.transform.position;

        // Debug.Log("Target location: " + target.transform.position + "    Distance to target = " + rayToTarget.magnitude);

        // Check the distance to the target
        if (rayToTarget.magnitude <= range)
        {
            // Debug.Log("Target in range of range: " + range); 
            return true;
        }

        return false; 
    }

    void ViewBehaviourCooldown() 
    {
        // Debug.Log("Run timer stopped");
        viewCooldown = false;
    }

    // ============================================================
    //                           Boid Rules 
    // ============================================================

    void ApplyRules()
    {
        // Debug.Log("Applying boid rules... ");
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
