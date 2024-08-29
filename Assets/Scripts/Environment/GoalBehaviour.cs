using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalBehaviour : MonoBehaviour
{
    [SerializeField] private int goalBoidCount = 20; // The goal number of Boids
    public int currentBoidCount = 0; // Current number of Boids in the zone
    public LayerMask boidLayer;
    public TMP_Text goalText;


    void UpdateText()
    {
        goalText.text = "Goal: " + currentBoidCount + "/" + goalBoidCount;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered");
        // Check if the entering object is a Boid by checking the tag or another identifier
        if (isBoidLayer(other.gameObject))
        {
            Debug.Log("Its a boid!");
            currentBoidCount++;
            UpdateText();
            CheckGoalReached();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is a Boid
        if (isBoidLayer(other.gameObject))
        {
            currentBoidCount--;
            UpdateText();
        }
    }

    private void CheckGoalReached()
    {
        if (currentBoidCount >= goalBoidCount)
        {
            Debug.Log("Goal reached! Boids in zone: " + currentBoidCount);
            // Additional actions can be added here, like triggering an event or starting a new behavior
        }
    }

    private bool isBoidLayer(GameObject obj)
    {
        return (boidLayer.value & 1 << obj.layer) > 0;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the goal zone in the editor
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
