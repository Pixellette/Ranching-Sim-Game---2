using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class Boid_script : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;  
    Animator animator;


    public LayerMask boidLayer; 
    public LayerMask nonBoidLayer;
    public LayerMask fenceLayer;

    [SerializeField] bool behaviorOnCooldown = false;
    [SerializeField] bool seenTargetCooldown = false; 

    [Header ("Speed settings")]

        [SerializeField] float currentSpeed; 
        [SerializeField] float walkSpeedSheep;
        [SerializeField] float walkSpeedCow;
        [SerializeField] float runSpeedSheep;
        [SerializeField] float runSpeedCow;
    
    [Header ("Behaviours Active")]
        [SerializeField] bool isFleeing = false;
        [SerializeField] bool isWandering = false; // For DEBUG only
        [SerializeField] bool isFlocking = false; // For DEBUG only
        // [SerializeField] bool isEating = false; // For DEBUG only 
        [SerializeField] bool lookingForFood = false;
        [SerializeField] bool currentlyEating = false; // For anims


    [Header ("Wander Settings")]
        [SerializeField] Vector3 wanderTarget = Vector3.zero; // cannot be local as needs to remember between calls

    
    [Header ("Species")]
        [SerializeField] bool isCow;
        [SerializeField] bool isSheep;


    [Header ("Hunger")]
        [SerializeField] float currentHunger; 
        [SerializeField] int displayedHunger;
        [SerializeField] float maxHunger = 100f;
        [SerializeField] float minHunger = 0f;
        public float hungerRate = 2f; // Rate at which hunger decreases
        public float eatAmount = 0.5f; // Amount of grass to eat
        [SerializeField] float hungerAdded = 25; // Amount of hunger added when it eats 

        [SerializeField] bool isPeckish = false;
        [SerializeField] bool isHungry = false;
        [SerializeField] bool isStarving = false;

        [SerializeField] int peckishThreshold = 80;
        [SerializeField] int hungryThreshold = 50; 
        [SerializeField] int starvingThreshold = 20;

        [SerializeField] int peckishChance = 10;
        [SerializeField] int hungryChance = 40;
        [SerializeField] int starvingChance = 90;

        [SerializeField] float searchRadius = 30f; // Radius to search for grass
        [SerializeField] float reqProxToGrass;

        public LayerMask grassLayer; // Layer mask to identify grass
        private Grass targetGrass; // Current target grass for the boid

        // TODO: 
        // // Define the range of child objects that are valid body options
        // [SerializeField] private int bodyStartIndex = 1;
        // [SerializeField] private int bodyEndIndex = 14; // Adjust this to match the index range of the body objects

        // Name of the child object that contains the body versions
        [SerializeField] private string bodyHolderName = "BodyHolder";
        


    // ============================================================
    //                         Methods 
    // ============================================================

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        // animator = transform.Find("TFP_Sheep_01A").GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the specified child object.");
        }

        target = GameObject.FindWithTag("Predator");

        boidLayer = LayerMask.GetMask("Boid");
        nonBoidLayer = ~boidLayer; 

        currentHunger = UnityEngine.Random.Range(70,100);
        displayedHunger = Mathf.RoundToInt(currentHunger);


        if (this.gameObject.CompareTag("Cow"))
        {
            currentSpeed = walkSpeedCow;
            reqProxToGrass = 4f;
            isCow = true;
            isSheep = false;
            ChooseRandomBody(1, 13);
        }
        else if (this.gameObject.CompareTag("Sheep"))
        {
            reqProxToGrass = 3f;
            currentSpeed = walkSpeedSheep;
            isCow = false;
            isSheep = true;
            ChooseRandomBody(1, 3);
        }
        else
        {
            Debug.LogError("Boid has an unknown tag: " + this.gameObject.name);
        }
    }

    void Update()
    {
        UpdateHungerState();
        ChooseBehaviour();

        UpdateAnimationState();
        SetSpeed();
    }



    // ============================================================
    //                       Behaviour Tree
    // ============================================================
    
    void ChooseBehaviour() 
    {
        if (!seenTargetCooldown)
        {
            if (CanSeeTarget()) // Target is within SIGHT range; flee
            { 
                Flee(target.transform.position);
                seenTargetCooldown = true; 
                isFleeing = true;
                isFlocking = false;
                isWandering = false;
                lookingForFood = false;
                Invoke("ViewBehaviourCooldown", 5);
            }
            else if (TargetInRange(FlockManager.FM.senseRange)) // Target is within SENSE range; flee
            {
                Flee(target.transform.position);
                seenTargetCooldown = true; 
                isFleeing = true;
                isFlocking = false;
                isWandering = false;
                lookingForFood = false;
                Invoke("ViewBehaviourCooldown", 5);
            }
            else if (isFleeing && (TargetInRange(FlockManager.FM.viewRange) || TargetInRange(FlockManager.FM.senseRange))) // If currently fleeing and still within range KEEP fleeing
            {
                Flee(target.transform.position);
                isFlocking = false;
                isWandering = false;
                lookingForFood = false;
            }
            else if (isFleeing) // IF target is fleeing but no longer in range then STOP fleeing 
            {
                isFleeing = false;
            }
            else // Not currently Fleeing
            {
                if (!behaviorOnCooldown) // Check if already behaving 
                {
                    if (!currentlyEating)
                    {
                        // Check hunger state first before considering flocking or wandering
                    if (isStarving)
                    {
                        if (UnityEngine.Random.Range(0, 100) < starvingChance) // High chance to eat
                        {
                            StartEatingBehavior();
                        }
                        else
                        {
                            ChooseOtherBehavior();
                        }
                    }
                    else if (isHungry)
                    {
                        if (UnityEngine.Random.Range(0, 100) < hungryChance) // Moderate chance to eat
                        {
                            StartEatingBehavior();
                        }
                        else
                        {
                            ChooseOtherBehavior();
                        }
                    }
                    else if (isPeckish)
                    {
                        if (UnityEngine.Random.Range(0, 100) < peckishChance) // Low chance to eat
                        {
                            StartEatingBehavior();
                        }
                        else
                        {
                            ChooseOtherBehavior();
                        }
                    }
                    else
                    {
                        ChooseOtherBehavior();
                    }

                    // int cooldownTime = Random.Range(FlockManager.FM.minWait, FlockManager.FM.maxWait);
                    // behaviorOnCooldown = true; 
                    // Invoke("BehavoiurCooldown", cooldownTime);
                    }
                    
                }
            }
        }
        else 
        {
            Flee(target.transform.position);

            if (!TargetInRange(FlockManager.FM.viewRange)) // If they've left range we can stop fleeing 
            {
                isFleeing = false;
            }
        }
    }
    
    private void ChooseOtherBehavior()
    {
        int cooldownTime = UnityEngine.Random.Range(FlockManager.FM.minWait, FlockManager.FM.maxWait);
        behaviorOnCooldown = true; 
        Invoke("BehavoiurCooldown", cooldownTime);
        lookingForFood = false;
        if (UnityEngine.Random.Range(0, 100) < FlockManager.FM.flockingChance) // Apply flocking Chance. Flock
        {
            ApplyFlockingRules(Vector3.zero);
            isFlocking = true;
            isWandering = false;
        }
        else
        {
            Wander();
            isWandering = true;
            isFlocking = false;
        }
    }

    void BehavoiurCooldown()
    {
        behaviorOnCooldown = false;
    }

    void ViewBehaviourCooldown() 
    {
        seenTargetCooldown = false;
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
        if (Physics.Raycast(agent.transform.position, direction, out hit, distance, fenceLayer))
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
        Vector3 fleeVector = location - this.transform.position;
        Vector3 fleeLocation = this.transform.position - fleeVector;

        // Seek(fleeLocation);
        ApplyFlockingRules(fleeLocation);
    }


    void Wander()
    {
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


    void SetSpeed()
    {
        if(isFleeing)
        {
            if (isCow)  currentSpeed = runSpeedCow;
            else if (isSheep) currentSpeed = runSpeedSheep;
        }
        else
        {
            if (isCow)  currentSpeed = walkSpeedCow;
            else if (isSheep) currentSpeed = walkSpeedSheep;
        }

        agent.speed = currentSpeed;
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


    // ============================================================
    //                           Boid Rules 
    // ============================================================

    void ApplyFlockingRules(Vector3 fleeLocation)
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

                if (!Physics.Raycast(transform.position, directionToBoid, distanceToBoid, fenceLayer))
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
            // Choose weights based on whether the boid is fleeing
            float alignmentWeight = isFleeing ? FlockManager.FM.fleeAlignmentWeight : FlockManager.FM.alignmentWeight;
            float cohesionWeight = isFleeing ? FlockManager.FM.fleeCohesionWeight : FlockManager.FM.cohesionWeight;

            // Average the alignment direction
            alignment /= groupSize;

            // Calculate the center of the group (cohesion)
            cohesion /= groupSize;
            Vector3 toCohesion = (cohesion - transform.position).normalized;

            // Combine the three behaviors with weighting factors
            Vector3 moveDirection = (separation * FlockManager.FM.separationWeight) + (alignment * alignmentWeight) + (toCohesion * cohesionWeight);

            if(isFleeing)
            {
                Vector3 fleeDirection = (fleeLocation - transform.position).normalized;
                moveDirection += fleeDirection * FlockManager.FM.fleeWeight;
            }

            // Normalize the result to get a final direction
            Vector3 newDestination = transform.position + moveDirection.normalized * 10f;

            // Seek the new destination
            Seek(newDestination);
        }
        else 
        {
            if(isFleeing)
            {
                Vector3 fleeDirection = (fleeLocation - transform.position).normalized;
                Vector3 moveDirection = fleeDirection;

                // Normalize the result to get a final direction
                Vector3 newDestination = transform.position + moveDirection.normalized * 10f;   
                Seek(newDestination);
            }
            else 
            {
                Wander();
                isWandering = true;
                isFlocking = false;
            }
            
        }
    }



    // ============================================================
    //                       Hunger Methods! 
    // ============================================================

    private void UpdateHungerState()
    {
        currentHunger -= hungerRate * Time.deltaTime; // Decrease hunger over time

        // Ensure hunger stays within bounds and tick down nicely
        currentHunger = Mathf.Clamp(currentHunger, minHunger, maxHunger);

        // Update hunger levels
        isStarving = currentHunger <= starvingThreshold;
        isHungry = currentHunger <= hungryThreshold && currentHunger > starvingThreshold;
        isPeckish = currentHunger <= peckishThreshold && currentHunger > hungryThreshold;
    }

    private void StartEatingBehavior()
    {
        // Set bools 
        behaviorOnCooldown = true;
        lookingForFood = true; 
        isFlocking = false; 
        isWandering = false; 

        Invoke("BehavoiurCooldown", 3);

        // Clear the current pathfinding destination
        GetComponent<NavMeshAgent>().ResetPath();

        if (targetGrass == null || !targetGrass.IsTallEnough() || Vector3.Distance(transform.position, targetGrass.transform.position) > searchRadius)
        {
            FindGrass(); // Look for grass when hungry and current target is invalid
        }
        else
        {
            Seek(targetGrass.transform.position);
            CheckArrival(); // Check if the boid has arrived at the grass
        }
    }

    private void FindGrass()
    {
        // Check nearby
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
                    Vector3 directionToGrass = grass.transform.position - transform.position;
                    float distance = directionToGrass.magnitude;

                    // Perform a raycast to check for obstacles between the Boid and the grass
                    if (Physics.Raycast(transform.position, directionToGrass.normalized, out RaycastHit hit, distance, fenceLayer))
                    {
                        // If the raycast hits something that is not the grass, discard this grass
                        if (hit.collider != grassCollider)
                        {
                            continue;
                        }
                    }

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
                // Seek will be called in CheckHunger after setting targetGrass
            }
            else
            {
                targetGrass = null; // Reset target if no suitable grass is found

                // Didn't find grass so keep moving
                Debug.Log("Didn't find grass (A)");
                lookingForFood = false;
            }
        }
        else
        {
            targetGrass = null; // Reset target if no grass is found

            // Didn't find grass so keep moving
            Debug.Log("Didn't find grass (B)");
            lookingForFood = false;
        }
    }

    private void CheckArrival()
    {
        if (targetGrass == null) return; // Early exit if no valid targetGrass

        // Check if the boid has reached the grass
        if (Vector3.Distance(transform.position, targetGrass.transform.position) < reqProxToGrass) 
        {  
            // Arrived at grass - start eating 
            StartEating();
        }
    }

    private void StartEating()
    {
        // Apply Bools 
        lookingForFood = false;
        currentlyEating = true;

        // Stop the Boid before starting the eating animation
        agent.isStopped = true;

        // Invoke a method to resume movement after the eating animation
        Invoke("FinishEating", 5.0f);
    }

    void FinishEating()
    {
        if (targetGrass != null)
        {
            // Eat the grass
            targetGrass.CutGrass(eatAmount); // Eat the grass

            // Update hunger
            currentHunger += hungerAdded;
            currentHunger = Mathf.Clamp(currentHunger, minHunger, maxHunger); // Clamp after eating
            displayedHunger = Mathf.RoundToInt(currentHunger);

            // Set Bools 
            currentlyEating = false;
            behaviorOnCooldown = false; 

            // Allow boid to begin moving again 
            agent.isStopped = false; 

            // Clear grass target 
            targetGrass = null; // Clear the target after eating
        }
        else // Grass failed somehow - reset! 
        {
            // Set Bools 
            currentlyEating = false;
            behaviorOnCooldown = false; 

            // Allow boid to begin moving again 
            agent.isStopped = false; 

            // Clear grass target 
            targetGrass = null; // Clear the target after eating
        }
    }


    // ============================================================
    //                       Animation Methods
    // ============================================================

    void UpdateAnimationState()
    {
        // Check if the boid is moving
        bool isMoving = agent.velocity.magnitude > 0.01f; // Use a small threshold to determine movement

        // If the boid is eating, prioritize the eating animation
        if (currentlyEating)
        {
            animator.SetBool("isEating", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
        else if (isMoving) // If the boid is moving, set the walking animation
        {
            if (isFleeing)
            {
                Debug.Log("isFleeing = true");
                animator.SetBool("isRunning", true);
                animator.SetBool("isWalking", false);
                animator.SetBool("isEating", false);
            }
            else 
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
                animator.SetBool("isEating", false);
            }
        }
        else // Otherwise, use the idle animation
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isEating", false);
        }
    }

    


    void ChooseRandomBody(int bodyStartIndex, int bodyEndIndex)
    {
        // Attempt to find the GameObject that holds the body variations
        Transform bodyHolder = transform.Find(bodyHolderName);

        // Debug to check if the bodyHolder is found
        if (bodyHolder == null)
        {
            Debug.LogError("BodyHolder with name '" + bodyHolderName + "' not found under the parent GameObject '" + gameObject.name + "'. Please check if the name is correct and the object exists.");
            return;
        }
        else
        {
            Debug.Log("BodyHolder '" + bodyHolderName + "' successfully found.");
        }

        // Check the number of children under the bodyHolder
        int totalBodies = bodyHolder.childCount;
        Debug.Log("BodyHolder has " + totalBodies + " child objects.");

        // If the range is invalid or the bodyHolder doesn't have enough children, log and return
        if (totalBodies == 0 || bodyStartIndex < 0 || bodyEndIndex >= totalBodies || bodyStartIndex > bodyEndIndex)
        {
            Debug.LogError("Invalid body index range or no body objects found. Check that bodyStartIndex and bodyEndIndex are correct.");
            return;
        }

        // Get the number of body options in the specified range
        int bodyCount = bodyEndIndex - bodyStartIndex + 1;

        // Debug log to confirm how many bodies are in the valid range
        Debug.Log("Valid body range: " + bodyCount + " bodies between indices " + bodyStartIndex + " and " + bodyEndIndex + ".");

        // Choose a random index within the valid range
        int randomIndex = UnityEngine.Random.Range(bodyStartIndex, bodyEndIndex + 1);
        Debug.Log("Random body chosen at index: " + randomIndex);

        // Loop through the bodyHolder's children and activate the randomly chosen one
        for (int i = bodyStartIndex; i <= bodyEndIndex; i++)
        {
            bool isActive = i == randomIndex;
            bodyHolder.GetChild(i).gameObject.SetActive(isActive);
            Debug.Log((isActive ? "Activated " : "Deactivated ") + "body at index " + i);
        }
    }


    // ============================================================
    //                       DEBUGGING METHODS
    // ============================================================

    void OnDrawGizmosSelected()
    {

        if (isFlocking || isFleeing)
        {
            // Set the color for the gizmos
            Gizmos.color = Color.green;

            // Create a sphere to represent the detection radius
            if (FlockManager.FM != null)
            {
                Gizmos.DrawWireSphere(transform.position, FlockManager.FM.neighbourDistance);
            }
            else
            {
                Debug.LogWarning("FlockManager.FM is null, unable to draw Gizmos.");
            }

            // Draw lines to each boid being tracked for flocking
            Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, FlockManager.FM.neighbourDistance, boidLayer);

            foreach (Collider boidCollider in nearbyBoids)
            {
                if (boidCollider.gameObject != this.gameObject) // Ignore self
                {
                    Vector3 directionToBoid = (boidCollider.transform.position - transform.position).normalized;
                    float distanceToBoid = Vector3.Distance(transform.position, boidCollider.transform.position);

                    if (!Physics.Raycast(transform.position, directionToBoid, distanceToBoid, fenceLayer))
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

        if(lookingForFood)
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

        // Draw a line to the piece of grass the boid is currently eating
        if (currentlyEating && targetGrass != null) // Assuming 'targetGrass' is the reference to the grass being eaten
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetGrass.transform.position); // Draw a line from the boid to the grass

            // Optionally, you can draw a sphere at the grass position to highlight it
            Gizmos.DrawWireSphere(targetGrass.transform.position, 0.5f);
        }
    }

}
