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
    [SerializeField] LayerMask groundLayer;

    [SerializeField] bool behaviorOnCooldown = false;
    [SerializeField] bool fleeCooldown = false; 
    [SerializeField] bool breedable = false;

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
        [SerializeField] bool isMating = false; // For DEBUG only 


    [Header ("Wander Settings")]
        [SerializeField] Vector3 wanderTarget = Vector3.zero; // cannot be local as needs to remember between calls

    
    [Header ("Species")]
        [SerializeField] bool isCattle;
        [SerializeField] bool isCow;
        [SerializeField] bool isCalf;
        [SerializeField] bool isBull;

        [SerializeField] bool isSheep;
        [SerializeField] bool isEwe;
        [SerializeField] bool isLamb;
        [SerializeField] bool isRam;


    [Header ("Hunger")]
        [SerializeField] float searchTimer;
        [SerializeField] float eatingTimer;
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

    [Header ("Breeding Settings")]
        [SerializeField] bool isMateable = false;
        [SerializeField] float mateDistance = 20;
        [SerializeField] float babyChance = 0.1f;
        [SerializeField] int breedingCooldownTimer = 60;
        [SerializeField] bool stillGrowing = false; // Is the boid still a growing baby
        [SerializeField] float ageUpTimer = 50;

    [Header ("Death Settings")] // Will add other ways to die in future! 
        [SerializeField] bool starvingToDeath = false;
        [SerializeField] float starveTime = 120;


    [Header ("Other Settings")]
        // Name of the child object that contains the body versions
        [SerializeField] private string bodyHolderName = "BodyHolder";

        [SerializeField] float fenceAvoidanceNum = 1.5f;
        


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
            isCattle = true;
            isSheep = false;

            if (isCow) ChooseRandomBody(1, 13);
            else if (isBull) ChooseRandomBody(1, 6);
            else if (isCalf) ChooseRandomBody(1, 11);
            else Debug.LogError("No gender selected! Req for variant spawning");
            
        }
        else if (this.gameObject.CompareTag("Sheep"))
        {
            reqProxToGrass = 3f;
            currentSpeed = walkSpeedSheep;
            isCattle = false;
            isSheep = true;
            ChooseRandomBody(1, 3);
        }
        else
        {
            Debug.LogError("Boid has an unknown tag: " + this.gameObject.name);
        }

        if (IsFemale()) // don't breed immediately
        {
            breedable = false;
            Invoke("BreedingCooldown", 80);
        }

        if (isCalf || isLamb) // Start growing up
        {
            // Start a timer
            stillGrowing = true;
            Invoke("StopGrowing", ageUpTimer);
        }
    }

    void Update()
    {
        UpdateHungerState();
        CheckStarving();
        ChooseBehaviour();

        UpdateAnimationState();
        SetSpeed();

        AlignWithTerrainAndDirection();  // Call this to adjust boid body orientation

        if (CanMate()) isMateable = true;
        else isMateable = false;

        if ((isCalf || isLamb) && !stillGrowing)
        {
            GrowUp();
        }
    }



    // ============================================================
    //                       Behaviour Tree
    // ============================================================
    
    void ChooseBehaviour() 
    {
        if (FleeBehaviourCheck()) // Flee
        {
            Flee(target.transform.position);

            // Turn off other behaviour
            isFlocking = false;
            isWandering = false;
            lookingForFood = false;
            currentlyEating = false;
            isMating = false; 
        }
        else if (currentlyEating) // Continue to Eat
        {
            // Eating. Don't update behaviour. 
        }
        else if (EatBehaviourCheck() ) // Start Eat (or look for food)
        {
            StartEatingBehavior();

            // Stop other behaviours 
            CancelInvoke("BehaviourCooldown");
            isFlocking = false;
            isWandering = false;
            isMating = false;
        }
        else if (!behaviorOnCooldown && breedable && IsFemale() && isMateable && SearchForMate())
        {
            ApplyMatingBehaviour();
        }
        else // Movement
        {
            if(!behaviorOnCooldown)
            {
                ChooseMovementBehavior();
            }
            
            // ELSE do not update, finish timer. 
        }
    }

    

    bool FleeBehaviourCheck()
    {
        if (isFleeing) // Already fleeing
        {
            // Check if the boid should still flee
            if (TargetInRange(FlockManager.FM.viewRange) || TargetInRange(FlockManager.FM.senseRange))// Can still see threat - refresh timer! 
            {
                // Stop old timer
                CancelInvoke("StopFleeing");

                // Restart cooldown 
                Invoke("StopFleeing", 5);
                return true;
            }
            else // no immediate threat, wait for timer 
            {
                // Continue to flee till timer runs out
                return true;
            }
        }
        else if (CanSeeTarget() || TargetInRange(FlockManager.FM.senseRange)) // Detect Predator 
        {
            // Start fleeing
            isFleeing = true; 
            Invoke("StopFleeing", 5);
            return true;
        }
        else 
        {
            // Not fleeing
            return false; 
        }
    }

    bool EatBehaviourCheck()
    {
        // Check if eating already
        if (lookingForFood)
        {
            // Continue to look until found OR timer runs out 
            return true;
        }
        else if (behaviorOnCooldown)
        {
            // Finish current behaviour first! 
            return false;
        }
        else // Not already eating
        {
            if (isStarving)
            {
                if (UnityEngine.Random.Range(0, 100) < starvingChance) // High chance to eat
                {
                    // Start eating - start timer
                    lookingForFood = true;
                    Invoke("StopSearchingForFood", searchTimer);
                    GetComponent<NavMeshAgent>().ResetPath();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (isHungry)
            {
                if (UnityEngine.Random.Range(0, 100) < hungryChance) // Moderate chance to eat
                {
                    // Start eating - start timer
                    lookingForFood = true;
                    Invoke("StopSearchingForFood", searchTimer);
                    GetComponent<NavMeshAgent>().ResetPath();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (isPeckish)
            {
                if (UnityEngine.Random.Range(0, 100) < peckishChance) // Low chance to eat
                {
                    // Start eating - start timer
                    lookingForFood = true;
                    Invoke("StopSearchingForFood", searchTimer);
                    GetComponent<NavMeshAgent>().ResetPath();
                    return true;
                }
                else 
                {
                    return false; 
                }
            }
            else // Isn't hungry, move on.
            {
                return false;
            }
        }
    }


    
    private void ChooseMovementBehavior()
    {
        int cooldownTime = UnityEngine.Random.Range(FlockManager.FM.minWait, FlockManager.FM.maxWait);
        behaviorOnCooldown = true; 
        Invoke("BehavoiurCooldown", cooldownTime);

        lookingForFood = false;
        currentlyEating = false;
        isMating = false; 

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

    void StopFleeing() 
    {
        isFleeing = false;
    }

    void StopSearchingForFood()
    {
        lookingForFood = false;
    }

    bool IsFemale()
    {
        if (isCow || isEwe)
        {
            return true;
        }
        return false;
    }

    void BreedingCooldown()
    {
        breedable = true;
    }

    public void ForceFlee(float fleeDuration)
    {
        // Ensure that fleeing behavior is prioritized
        isFleeing = true;

        // Reset any other behaviors
        isFlocking = false;
        isWandering = false;
        lookingForFood = false;
        currentlyEating = false;
        isMating = false;
        CancelInvoke("BehaviourCooldown"); // Cancel other behaviors' cooldown timers

        // Start or reset the fleeing timer
        CancelInvoke("StopFleeing");
        Invoke("StopFleeing", fleeDuration);
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
            Vector3 closestPoint = hit.point - direction * fenceAvoidanceNum;

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
            if (isCattle)  currentSpeed = runSpeedCow;
            else if (isSheep) currentSpeed = runSpeedSheep;
        }
        else
        {
            if (isCattle)  currentSpeed = walkSpeedCow;
            else if (isSheep) currentSpeed = walkSpeedSheep;
        }

        agent.speed = currentSpeed;
    }

    void AlignWithTerrainAndDirection()
{
    RaycastHit hit;
    Vector3 averageNormal = Vector3.zero;
    int sampleCount = 6; // Number of samples for averaging
    float sampleRadius = 1.5f; // Radius for additional raycasting around the agent

    // Cast a ray straight down to detect the ground
    if (Physics.Raycast(agent.transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
    {
        averageNormal += hit.normal; // Start with the ground normal

        // Cast additional rays to sample around the agent
        for (int i = 0; i < sampleCount; i++)
        {
            float angle = (i / (float)sampleCount) * Mathf.PI * 2;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * sampleRadius;
            Vector3 samplePosition = agent.transform.position + offset;

            // Perform raycast at the sample position
            if (Physics.Raycast(samplePosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                averageNormal += hit.normal; // Add the normal from this ray
                // Draw a debug line to visualize the ray
                Debug.DrawLine(samplePosition, hit.point, Color.red, 0.1f);
            }
        }

        // Normalize the average normal vector
        averageNormal.Normalize();

        // Find the boid's body (adjust name to 'BodyHolder')
        Transform bodyHolder = transform.Find("BodyHolder");
        if (bodyHolder != null)
        {
            // Calculate the terrain-aligned rotation using the average normal
            Quaternion terrainRotation = Quaternion.FromToRotation(Vector3.up, averageNormal);

            // Get the boid's current movement direction (ignore Y-axis)
            Vector3 moveDirection = agent.velocity.normalized;
            moveDirection.y = 0; // Ignore Y to maintain level for facing direction
            if (moveDirection.magnitude > 0.1f) // Ensure there is some movement
            {
                Quaternion facingRotation = Quaternion.LookRotation(moveDirection);
                // Combine the terrain alignment with facing direction
                Quaternion adjustedRotation = Quaternion.Euler(0f, facingRotation.eulerAngles.y, 0f) * terrainRotation;

                // Directly set the rotation to avoid compounding errors
                bodyHolder.rotation = adjustedRotation;
            }
            else
            {
                // Optionally, keep the bodyHolder's rotation aligned with the terrain when stationary
                bodyHolder.rotation = terrainRotation;
            }
        }
    }
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
                        if ((this.isCattle && nearbyBoid.isCattle) || (this.isSheep && nearbyBoid.isSheep))
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
        // TODO: check - should only do if STARTing behaviour? 
        // Clear the current pathfinding destination
        // GetComponent<NavMeshAgent>().ResetPath();

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
                // Debug.Log("Didn't find grass - none suitable");
                lookingForFood = false;
            }
        }
        else
        {
            targetGrass = null; // Reset target if no grass is found

            // Didn't find grass so keep moving
            // Debug.Log("Didn't find grass - none in range");
            lookingForFood = false;
        }
    }

    private void CheckArrival()
    {
        if (targetGrass == null) return; // Early exit if no valid targetGrass

        // Check if the boid has reached the grass
        if (Vector3.Distance(transform.position, targetGrass.transform.position) < reqProxToGrass) 
        {  
            // Stop timer for searching for food as task complete
            CancelInvoke("StopSearchingForFood");
            lookingForFood = false;

            // Start Eating timer (also restart movement) + bool 
            currentlyEating = true;
            Invoke("FinishEating", 5.0f);

            // Arrived at grass - start eating 
            StartEating();
        }
    }

    private void StartEating()
    {
        // Stop the Boid before starting the eating animation
        agent.isStopped = true;

        if (targetGrass != null)
        {
            // Eat the grass
            targetGrass.CutGrass(eatAmount); // Eat the grass

            // Update hunger
            currentHunger += hungerAdded;
            currentHunger = Mathf.Clamp(currentHunger, minHunger, maxHunger); // Clamp after eating
            displayedHunger = Mathf.RoundToInt(currentHunger);

        }
        else // Grass failed somehow - reset! 
        {
            // Set Bools 
            currentlyEating = false;

            // Allow boid to begin moving again 
            agent.isStopped = false; 

            // Clear grass target 
            targetGrass = null; // Clear the target after eating
        }
    }

    void FinishEating() // Invoke method
    {
        // Set Bools 
        currentlyEating = false;

        // Allow boid to begin moving again 
        agent.isStopped = false; 
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
                // Debug.Log("isFleeing = true");
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

    

    // ============================================================
    //                       Start Up Methods
    // ============================================================

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
            // Debug.Log("BodyHolder '" + bodyHolderName + "' successfully found.");
        }

        // Check the number of children under the bodyHolder
        int totalBodies = bodyHolder.childCount;
        // Debug.Log("BodyHolder has " + totalBodies + " child objects.");

        // If the range is invalid or the bodyHolder doesn't have enough children, log and return
        if (totalBodies == 0 || bodyStartIndex < 0 || bodyEndIndex >= totalBodies || bodyStartIndex > bodyEndIndex)
        {
            Debug.LogError("Invalid body index range or no body objects found. Check that bodyStartIndex and bodyEndIndex are correct.");
            return;
        }

        // Get the number of body options in the specified range
        int bodyCount = bodyEndIndex - bodyStartIndex + 1;

        // Debug log to confirm how many bodies are in the valid range
        // Debug.Log("Valid body range: " + bodyCount + " bodies between indices " + bodyStartIndex + " and " + bodyEndIndex + ".");

        // Choose a random index within the valid range
        int randomIndex = UnityEngine.Random.Range(bodyStartIndex, bodyEndIndex + 1);
        // Debug.Log("Random body chosen at index: " + randomIndex);

        // Loop through the bodyHolder's children and activate the randomly chosen one
        for (int i = bodyStartIndex; i <= bodyEndIndex; i++)
        {
            bool isActive = i == randomIndex;
            bodyHolder.GetChild(i).gameObject.SetActive(isActive);
            // Debug.Log((isActive ? "Activated " : "Deactivated ") + "body at index " + i);
        }
    }


    // ============================================================
    //                       Breeding Methods
    // ============================================================

    bool CanMate() 
    {
        if (isLamb || isCalf) // too young to breed! TODO: Add babies of other species
        {
            return false;
        }
        else if (currentHunger > 65) // Not hungry TODO: change later to happiness? 
        {
            return true; 
        }
        else // isn't baby but is hungry
        {
            return false;
        }
        
    }

    bool SearchForMate()
    {
        Vector3 currentPosition = transform.position;
        GameObject thisBoid = this.gameObject;

        // Check for Boids? 
        Collider[] nearbyBoids = Physics.OverlapSphere(transform.position, mateDistance, boidLayer);
        if (nearbyBoids.Length == 0) return false;

        foreach (Collider boidCollider in nearbyBoids)
        {
            if (boidCollider.gameObject != thisBoid)
            {
                Boid_script nearbyBoid = boidCollider.GetComponent<Boid_script>();
                if (nearbyBoid != null)
                {
                    float distanceToBoidSqr = (boidCollider.transform.position - currentPosition).sqrMagnitude;
                    float mateDistanceSqr = mateDistance * mateDistance;

                    if (distanceToBoidSqr < mateDistanceSqr &&
                            !Physics.Raycast(currentPosition, (boidCollider.transform.position - currentPosition).normalized, Mathf.Sqrt(distanceToBoidSqr), fenceLayer))
                    {
                        if (IsCompatibleMate(nearbyBoid) && nearbyBoid.isMateable) // Check Gender match up! 
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    bool IsCompatibleMate(Boid_script nearbyBoid)
    {
        // Sheep-Ram compatibility check
        if (this.isSheep && nearbyBoid.isRam)
            return true;

        // Cow-Bull compatibility check
        if (this.isCow && nearbyBoid.isBull)
            return true;

        // Add other species checks here in future 

        // If no match is found, return false
        return false;
    }

    void ApplyMatingBehaviour()
    {
        // Mating behaviour here 
        // Debug.Log("Found a suitable mate");
        isMating = true; 
        isFlocking = false;
        isWandering = false;
        lookingForFood = false;
        currentlyEating = false;

        // Chance for baby 
        if (UnityEngine.Random.Range(0,100) < babyChance)
        {
            Vector3 agentPosition = transform.position; // Get the agent's current position
            NavMeshHit hit;

            // Sample the closest point on the NavMesh within a certain radius
            if (NavMesh.SamplePosition(agentPosition, out hit, 10.0f, NavMesh.AllAreas)) 
            {
                string species = null; 
                if (isSheep) species = "sheep";
                else if (isCattle) species = "cattle"; 
                else Debug.LogError("Invalid species while creating baby");
                if (species != null)
                {
                    Vector3 spawnLocation = hit.position; // Get the closest valid point on the NavMesh
                    FlockManager.FM.SpawnAnimal(species, 2, spawnLocation); // varientIndex of 2 = baby verion of animal
                    breedable = false;
                    Invoke("BreedingCooldown", breedingCooldownTimer);
                    if(isSheep && UnityEngine.Random.Range(0,10) <= 5) // Sheep often have twins
                    {
                        FlockManager.FM.SpawnAnimal(species, 2, spawnLocation); // varientIndex of 2 = baby verion of animal
                        Debug.Log("TWIN Baby " + species + " born!");
                    }
                    else Debug.Log("Baby " + species + " born!");
                }
                
            }
            else Debug.LogError("No where to spawn baby");
        }

        // wait a moment then Cont to wander? 
        ChooseMovementBehavior();
    }

    void StopGrowing()
    {
        stillGrowing = false;
        Debug.Log("A baby should now be grown up");
    }

    void GrowUp()
    {
        /*
            Check species 
            spawn adult version at location 
            destroy self 
        */
        int gender = 1;
        if (UnityEngine.Random.Range(0,100) < 20) gender = 0;

        if (isLamb)
        {
            FlockManager.FM.SpawnAnimal("sheep", gender, transform.position);
            FlockManager.FM.RemoveAnimal(gameObject, "sheep");
        }
        else if (isCalf)
        {
            FlockManager.FM.SpawnAnimal("cattle", gender, transform.position);
            FlockManager.FM.RemoveAnimal(gameObject, "cattle");
        }
        else Debug.LogError("Couldn't match baby type to spawn correct adult version");
    }


    // ============================================================
    //                       Death Methods
    // ============================================================

    void StarvingTimer() // Invoke Method
    {
        // Animal dies now
        Debug.Log("Animal Death");
        if(isSheep) FlockManager.FM.RemoveAnimal(gameObject, "sheep");
        else if (isCattle) FlockManager.FM.RemoveAnimal(gameObject, "cattle");
        else Debug.LogError("Couldn't find species for animal starving death");
    }

    void CheckStarving()
    {
        if (!starvingToDeath && currentHunger == 0)
        {
            starvingToDeath = true; 
            Invoke("StarvingTimer", starveTime);
        }
        else if (starvingToDeath && currentHunger > 0)
        {
            CancelInvoke("StarvingTimer");
            starvingToDeath = false;
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
                            if ((this.isCattle && nearbyBoid.isCattle) || (this.isSheep && nearbyBoid.isSheep))
                            {
                                // Draw a line from the current boid to the nearby boid
                                Gizmos.DrawLine(transform.position, nearbyBoid.transform.position);
                            }
                            
                        }
                    }
                }
            }

            // Draw a red line to the predator if actively fleeing because of predator visibility or proximity
            if (isFleeing && (TargetInRange(FlockManager.FM.viewRange) || TargetInRange(FlockManager.FM.senseRange)))
            {
                Gizmos.color = Color.red; // Red color for threat
                if (target != null)
                {
                    Gizmos.DrawLine(transform.position, target.transform.position);
                }
            }
        }

        if(lookingForFood)
        {
            // Set the color for the gizmos
            Gizmos.color = Color.cyan;

            // Create a sphere to represent the detection radius
            Gizmos.DrawWireSphere(transform.position, searchRadius);

            // Draw lines to each nearby grass
            Collider[] nearbyGrass = Physics.OverlapSphere(transform.position, searchRadius, grassLayer);

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
