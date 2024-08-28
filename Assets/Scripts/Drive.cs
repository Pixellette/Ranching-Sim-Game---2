using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Drive : MonoBehaviour
{
    [SerializeField] float speed = 10.0f;
    [SerializeField] float strafeSpeed = 7.5f; // Speed for moving left and right
    [SerializeField] float currentSpeed = 0;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false; // Disable automatic rotation to avoid conflicts with LookControls
        }
    }

    void Update()
    {
        // Get input for movement
        float moveDirection = Input.GetAxis("Vertical") * speed;
        float strafeDirection = Input.GetAxis("Horizontal") * strafeSpeed;

        // Calculate movement direction
        Vector3 move = transform.forward * moveDirection;
        Vector3 strafe = transform.right * strafeDirection;
        Vector3 movement = move + strafe;

        // Apply movement using NavMeshAgent
        if (agent != null && movement != Vector3.zero)
        {
            agent.Move(movement * Time.deltaTime);
            currentSpeed = movement.magnitude;
        }
    }
}




