using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class Boid_script : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;  

    public LayerMask boidLayer; 
    public LayerMask nonBoidLayer;

    [SerializeField] bool behaviorOnCooldown = false;
    [SerializeField] bool seenTargetCooldown = false; 
    
    [Header ("Behaviours Active")]
        [SerializeField] bool isFleeing = false;
        [SerializeField] bool isWandering = false; // For DEBUG only
        [SerializeField] bool isFlocking = false; // For DEBUG only


    [Header ("Wander Settings")]
        [SerializeField] Vector3 wanderTarget = Vector3.zero; // cannot be local as needs to remember between calls

    
    [Header ("Species")]
        [SerializeField] bool isCow;
        [SerializeField] bool isSheep;


    [Header ("Hunger")]
        [SerializeField] bool isHungry = false;
        // public float hunger = 1f; // Boid's hunger level
        [SerializeField] float currentHunger; 
        [SerializeField] int displayedHunger;
        [SerializeField] float maxHunger = 100f;
        [SerializeField] float minHunger = 0f;
        public float hungerRate = 2f; // Rate at which hunger decreases

        [SerializeField] int hungryThreshold = 50; 
        public float eatAmount = 0.5f; // Amount of grass to eat
        [SerializeField] float searchRadius = 30f; // Radius to search for grass
        [SerializeField] float reqProxToGrass = 3.0f;

        public LayerMask grassLayer; // Layer mask to identify grass
        private Grass targetGrass; // Current target grass for the boid
        


    // ============================================================
    //                         Methods 
    // ============================================================

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindWithTag("Predator");

        boidLayer = LayerMask.GetMask("Boid");
        nonBoidLayer = ~boidLayer; 

        currentHunger = 99;
        displayedHunger = Mathf.RoundToInt(currentHunger);


        if (this.gameObject.CompareTag("Cow"))
        {
            isCow = true;
            isSheep = false;
        }
        else if (this.gameObject.CompareTag("Sheep"))
        {
            isCow = false;
            isSheep = true;
        }
        else
        {
            Debug.LogError("Boid has an unknown tag: " + this.gameObject.name);
        }
    }

    void Update()
    {
        CheckHunger();

        // Movement();
    }



    // ============================================================
    //                         Navigation Methods
    // ============================================================

    void Movement() 
    {
        if (!seenTargetCooldown)
        {
            if (CanSeeTarget()) // Target is within SIGHT range; flee
            { 
                Flee(target.transform.position);
                seenTargetCooldown = true; 
                isFleeing = true;
                Invoke("ViewBehaviourCooldown", 5);
            }
            else if (TargetInRange(FlockManager.FM.senseRange)) // Target is within SENSE range; flee
            {
                // Debug.Log("Can SENSE target!");
                Flee(target.transform.position);
                seenTargetCooldown = true; 
                isFleeing = true;
                isFlocking = false;
                isWandering = false;
                Invoke("ViewBehaviourCooldown", 5);
            }
            else if (isFleeing && (TargetInRange(FlockManager.FM.viewRange) || TargetInRange(FlockManager.FM.senseRange))) // If currently fleeing and still within range KEEP fleeing
            {
                Flee(target.transform.position);
                isFlocking = false;
                isWandering = false;
            }
            else if (isFleeing) // IF target is fleeing but no longer in range then STOP fleeing 
            {
                // Debug.Log("Target left range");
                isFleeing = false;
            }
            else // Not currently Fleeing
            {
                if (!behaviorOnCooldown) // Check if already behaving 
                {
                    if(Random.Range(0, 100) < FlockManager.FM.flockingChance){ // // Apply flocking Chance. Flock
                        ApplyFlockingRules();
                        isFlocking = true;
                        isWandering = false;
                        }
                    else { // If not flocking, wander. 
                        Wander();
                        isWandering = true; 
                        isFlocking = false; 
                    }
                    int cooldownTime = Random.Range(FlockManager.FM.minWait, FlockManager.FM.maxWait);
                    behaviorOnCooldown = true; 
                    Invoke("BehavoiurCooldown", cooldownTime);
                    
                }
                
            }
        } // if having seen target is still active we're still fleeing. 
        else {
            Flee(target.transform.position);

            if (!TargetInRange(FlockManager.FM.viewRange)) // If they've left range we can stop fleeing 
            {
                // Debug.Log("Target left view range");
                // seenTargetCooldown = false;
                isFleeing = false;
            }
        }
    }


    void Seek(Vector3 location)
    {
        RaycastHit hit;
        Vector3 direction = (location - agent.transform.position).normalized;
        float distance = Vector3.Distance(agent.transform.position, location);

        // Cast a ray to detect the obstacle in the path
        if (Physics.Raycast(agent.transform.position, direction, out hit, distance, nonBoidLayer))
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
    } 


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
        wanderTarget += new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f) * FlockManager.FM.wanderJitter,   // X
                                    0,                                                      // Y
                                    UnityEngine.Random.Range(-1.0f, 1.0f) * FlockManager.FM.wanderJitter);  // Z

        // Move the target back onto the circle (currently ON the Agent)
        wanderTarget.Normalize(); // get a better number
        wanderTarget *= FlockManager.FM.wanderRadius; // push it out to the right length

        // Move circle to *infront* of Agent
        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, FlockManager.FM.wanderDistance); // local because we are imagining the Agent as the center of the world
        Vector3 targetWorld = this.gameObject.transform.TransformPoint(targetLocal); // Now convert to world location 

        // Finally Seek the target location
        Seek(targetWorld);
    }


    void BehavoiurCooldown()
    {
        behaviorOnCooldown = false;
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
        seenTargetCooldown = false;
    }

    // ============================================================
    //                           Boid Rules 
    // ============================================================

    void ApplyFlockingRules()
    {
        int totalGroupSize= 0;
        int groupSize = 0;
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        // Create a sphere at the boid's position and detect other boids within the detection radius
        Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, FlockManager.FM.neighbourDistance, boidLayer);

        foreach (Collider boidCollider in nearbyBoids)
        {
            if (boidCollider.gameObject != this.gameObject) // Ignore self
            {
                // Perform a raycast to check if there is an obstacle between the current boid and the nearby boid (ignoring other boids as obstacles)
                Vector3 directionToBoid = (boidCollider.transform.position - transform.position).normalized;
                float distanceToBoid = Vector3.Distance(transform.position, boidCollider.transform.position);

                if (!Physics.Raycast(transform.position, directionToBoid, distanceToBoid, nonBoidLayer))
                {
                    // No obstacle in the way, proceed with calculations
                    Boid_script nearbyBoid = boidCollider.GetComponent<Boid_script>();
                    if (nearbyBoid != null)
                    {
                        // Separation: Steer away from nearby boids that are too close
                        Vector3 toBoid = transform.position - nearbyBoid.transform.position;
                        separation += toBoid / toBoid.sqrMagnitude; // Inversely proportional to distance

                        totalGroupSize++;

                        // Check if the species matches! 
                        if ((this.isCow && nearbyBoid.isCow) || (this.isSheep && nearbyBoid.isSheep))
                        {
                            // Alignment: Average the direction of nearby boids
                            alignment += nearbyBoid.transform.forward;

                            // Cohesion: Move towards the average position of the group
                            cohesion += nearbyBoid.transform.position;

                            // increase species group
                            groupSize++;
                        }
                    }
                }
            }
        }

        if (groupSize > 0)
        {
            // Average the alignment direction
            alignment /= groupSize;

            // Calculate the center of the group (cohesion)
            cohesion /= groupSize;
            Vector3 toCohesion = (cohesion - transform.position).normalized;

            // Combine the three behaviors with weighting factors
            Vector3 moveDirection = (separation * FlockManager.FM.seperationWeight) + (alignment * FlockManager.FM.alignmentWeight) + (toCohesion * FlockManager.FM.cohesionWeight);

            // Normalize the result to get a final direction
            Vector3 newDestination = transform.position + moveDirection.normalized * 10f;

            // Seek the new destination
            Seek(newDestination);
        }
    }



    // ============================================================
    //                       Grass Methods! 
    // ============================================================

    private void CheckHunger()
    {
        currentHunger -= hungerRate * Time.deltaTime; // Decrease hunger over time

        // Ensure hunger stays within bounds and tick down nicely
        currentHunger = Mathf.Clamp(currentHunger, minHunger, maxHunger);

        // Display the hunger value as an integer for better readability
        displayedHunger = Mathf.RoundToInt(currentHunger);

        if (currentHunger <= hungryThreshold)
        {
            Debug.Log("Hunger low");
            isHungry = true;

            // Check if targetGrass is invalid (null, too short, or out of range)
            if (targetGrass == null || !targetGrass.IsTallEnough() || Vector3.Distance(transform.position, targetGrass.transform.position) > searchRadius)
            {
                Debug.Log("Looking for grass...");
                FindAndEatGrass(); // Look for grass when hungry and current target is invalid
            }
            else
            {
                Debug.Log("Heading to grass");
                // Use Seek method to move towards the current target grass
                Seek(targetGrass.transform.position);
                CheckArrival(); // Check if the boid has arrived at the grass
            }
        }
        else 
        {
            isHungry = false;
        }
    }



    private void FindAndEatGrass()
    {
        // Use OverlapSphere to find nearby grass
        Collider[] grassColliders = Physics.OverlapSphere(transform.position, searchRadius, grassLayer);

        if (grassColliders.Length > 0)
        {
            // Find the closest grass object that is tall enough
            Collider closestGrass = null;
            float closestDistance = float.MaxValue;

            foreach (Collider grassCollider in grassColliders)
            {
                Grass grass = grassCollider.GetComponent<Grass>();
                if (grass != null && grass.IsTallEnough())
                {
                    float distance = Vector3.Distance(transform.position, grass.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestGrass = grassCollider;
                    }
                }
            }

            if (closestGrass != null)
            {
                targetGrass = closestGrass.GetComponent<Grass>();
                Debug.Log("New grass target found: " + targetGrass.name);
                // Seek will be called in CheckHunger after setting targetGrass
            }
            else
            {
                Debug.Log("No suitable grass found within search radius.");
                targetGrass = null; // Reset target if no suitable grass is found
            }
        }
        else
        {
            Debug.Log("No grass found within search radius.");
            targetGrass = null; // Reset target if no grass is found
        }
    }


    private void CheckArrival()
    {
        if (targetGrass == null) return; // Early exit if no valid targetGrass

        Debug.Log("Checking proximity to grass...");
        // Check if the boid has reached the grass
        if (Vector3.Distance(transform.position, targetGrass.transform.position) < reqProxToGrass)
        {
            Debug.Log("Reached grass!");
            targetGrass.CutGrass(eatAmount); // Eat the grass
            currentHunger += 10; // Reset hunger
            currentHunger = Mathf.Clamp(currentHunger, minHunger, maxHunger); // Clamp after eating
            targetGrass = null; // Clear the target after eating
        }
    }





    // ============================================================
    //                       DEBUGGING METHODS
    // ============================================================

    void OnDrawGizmosSelected()
    {

        if (isFlocking)
        {
            // Set the color for the gizmos
            Gizmos.color = Color.green;

            // Create a sphere to represent the detection radius
            Gizmos.DrawWireSphere(transform.position, FlockManager.FM.neighbourDistance);

            // Draw lines to each boid being tracked for flocking
            Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, FlockManager.FM.neighbourDistance, boidLayer);

            foreach (Collider boidCollider in nearbyBoids)
            {
                if (boidCollider.gameObject != this.gameObject) // Ignore self
                {
                    Vector3 directionToBoid = (boidCollider.transform.position - transform.position).normalized;
                    float distanceToBoid = Vector3.Distance(transform.position, boidCollider.transform.position);

                    if (!Physics.Raycast(transform.position, directionToBoid, distanceToBoid, nonBoidLayer))
                    {
                        Boid_script nearbyBoid = boidCollider.GetComponent<Boid_script>();
                        if (nearbyBoid != null)
                        {
                            if ((this.isCow && nearbyBoid.isCow) || (this.isSheep && nearbyBoid.isSheep))
                            {
                                // Draw a line from the current boid to the nearby boid
                                Gizmos.DrawLine(transform.position, nearbyBoid.transform.position);
                            }
                            
                        }
                    }
                }
            }
        }

        if(isHungry)
        {
            // Set the color for the gizmos
            Gizmos.color = Color.cyan;

            // Create a sphere to represent the detection radius
            Gizmos.DrawWireSphere(transform.position, FlockManager.FM.neighbourDistance);

            // Draw lines to each nearby grass
            Collider[] nearbyGrass = Physics.OverlapSphere(transform.position, FlockManager.FM.neighbourDistance, grassLayer);

            foreach (Collider grassCollider in nearbyGrass)
            {
                // Draw a line from the current boid to the grass
                Gizmos.DrawLine(transform.position, grassCollider.transform.position);
            }
        }
    }



}
