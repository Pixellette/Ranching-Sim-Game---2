using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 5;
    public float gravity = -9.18f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public bool inBuildMode = false;

    Vector3 velocity;
    bool isGrounded;

    // public Boid_script boid; // Reference to the boid script where ForceFlee is implemented
    [SerializeField] float  fleeDuration = 5;
    [SerializeField] float fleeRadius = 10f;  // Radius within which Boids will flee
    [SerializeField] LayerMask boidLayerMask; // LayerMask for Boids

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
                speed = 10;
            }
            else
            {
                speed = 5;
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

    // Optionally, visualize the radius in the editor for easier tuning
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
    }
}