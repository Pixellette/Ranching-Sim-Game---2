using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header ("Movement")]
        [SerializeField] float speed;
        [SerializeField] float gravity = -9.18f;
        [SerializeField] float jumpHeight;

    [Header ("Ground")]
        [SerializeField] Transform groundCheck;
        [SerializeField] float groundDistance = 0.4f;
        [SerializeField] LayerMask groundMask;
    
    [Header ("PlayMode")]
        public bool inBuildMode = false; // PUBLIC for FenceBuilder

    [Header ("Boid Settings")]
        [SerializeField] float  fleeDuration = 5;
        [SerializeField] float fleeRadius = 10f;  // Radius within which Boids will flee
        [SerializeField] LayerMask boidLayerMask; 
    
    [Header ("Gate Interaction")]
        [SerializeField] float gateIntDist;

// ============================== Hidden Variables ==============================
    Vector3 velocity;
    bool isGrounded;

    // ============================================================
    //                           METHODS
    // ============================================================
    void Update()
    {
        if (!inBuildMode)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            if (Input.GetKey("left shift") && isGrounded)
            {
                speed = 20;
            }
            else
            {
                speed = 10;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);

            // Check for right mouse click
            if (Input.GetMouseButtonDown(1))
            {
                // Trigger the ForceFlee method
                TriggerFlee();
            }

            // Check for interaction key (e.g., "E")
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithGate();
            }
        }
    }


    void TriggerFlee()
    {
        // Get all colliders in the specified radius that are part of the boid layer
        Collider[] boidsInRadius = Physics.OverlapSphere(transform.position, fleeRadius, boidLayerMask);

        // Loop through each collider and apply ForceFlee to the boids
        foreach (Collider col in boidsInRadius)
        {
            Boid_script boid = col.GetComponent<Boid_script>();
            if (boid != null)
            {
                boid.ForceFlee(fleeDuration);
            }
        }
    }

    void InteractWithGate()
    {
        // Define the number of rays to cast and the angle offset for the spread
        int numberOfRays = 5;
        float angleOffset = 1.5f; // Degrees to offset each ray from the center

        // Cast multiple rays to improve the detection
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate the direction of each ray with a slight spread
            Vector3 rayDirection = Camera.main.transform.forward;

            // Slightly adjust the ray direction for additional rays
            if (i == 1)
            {
                rayDirection = Quaternion.Euler(angleOffset, 0, 0) * rayDirection; // Slightly upward
            }
            else if (i == 2)
            {
                rayDirection = Quaternion.Euler(-angleOffset, 0, 0) * rayDirection; // Slightly downward
            }
            else if (i == 3)
            {
                rayDirection = Quaternion.Euler(0, angleOffset, 0) * rayDirection; // Slightly to the right
            }
            else if (i == 4)
            {
                rayDirection = Quaternion.Euler(0, -angleOffset, 0) * rayDirection; // Slightly to the left
            }

            // Perform the raycast using the adjusted ray direction
            Ray ray = new Ray(Camera.main.transform.position, rayDirection);
            RaycastHit hit;

            // Raycast within a certain range, e.g., 3 units
            if (Physics.Raycast(ray, out hit, gateIntDist))
            {
                Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

                // Ensure that the hit object is tagged as "Gate"
                if (hit.collider.CompareTag("Gate"))
                {
                    // Get the GateController component from the parent of the hit collider
                    GateController gate = hit.collider.GetComponentInParent<GateController>();

                    // Check if the gate component is not null before calling ToggleGate()
                    if (gate != null)
                    {
                        gate.ToggleGate();
                        return; // Stop checking if we've already interacted with the gate
                    }
                    else
                    {
                        Debug.LogWarning("Gate object is missing GateController component.");
                    }
                }
            }
        }
    }






    // ============================================================
    //                          Debug Methods
    // ============================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
    }
}