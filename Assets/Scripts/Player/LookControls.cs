using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookControls : MonoBehaviour
{
    [SerializeField] float zoneWidthPercentage = 0.33f; // Percentage for Zone One and Two widths
    [SerializeField] float zoneHeightPercentage = 0.33f; // Percentage for Zone One and Two heights
    [SerializeField] float slowRotationSpeed = 10.0f; // Slow rotation speed for Zone One
    [SerializeField] float fastRotationSpeed = 40.0f; // Fast rotation speed for Zone Two
    [SerializeField] float sensitivity = 0.003f; // Mouse sensitivity for X and Y rotation
    [SerializeField] float centerDeadZoneWidth = 30.0f; // Center dead zone width in pixels for X axis
    [SerializeField] float centerDeadZoneHeight = 30.0f; // Center dead zone height in pixels for Y axis
    [SerializeField] float pitchClampTop = 80.0f; // Max pitch angle to look up (degrees)
    [SerializeField] float pitchClampBottom = -80.0f; // Max pitch angle to look down (degrees)

    private float xZoneOneThreshold;
    private float xZoneTwoThreshold;
    private float yZoneOneThreshold;
    private float yZoneTwoThreshold;

    private Transform playerBody; // Reference to the player body
    private Transform cameraTransform; // Reference to the camera

    void Start()
    {
        // Calculate screen width and height in pixels
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Calculate thresholds based on percentages of screen size
        xZoneOneThreshold = screenWidth * zoneWidthPercentage;
        xZoneTwoThreshold = screenWidth * (zoneWidthPercentage * 2);
        yZoneOneThreshold = screenHeight * zoneHeightPercentage;
        yZoneTwoThreshold = screenHeight * (zoneHeightPercentage * 2);
        
        // Assign player body and camera references
        playerBody = transform; // Assuming this script is on the player body
        cameraTransform = Camera.main.transform; // Assuming the main camera is used
    }

    void Update()
    {
        // Get the mouse position
        Vector3 mousePosition = Input.mousePosition;

        // Calculate center of the screen
        float screenCenterX = Screen.width / 2;
        float screenCenterY = Screen.height / 2;

        // Determine mouse position relative to the screen center
        float deltaX = mousePosition.x - screenCenterX;
        float deltaY = mousePosition.y - screenCenterY;

        // Determine the X and Y zone for the mouse
        float xRotationSpeed = 0;
        float yRotationSpeed = 0;

        // Adjust X and Y rotation speeds based on mouse position
        if (Mathf.Abs(deltaX) > centerDeadZoneWidth)
        {
            if (Mathf.Abs(deltaX) < xZoneOneThreshold)
            {
                // X is in Zone One
                xRotationSpeed = slowRotationSpeed;
            }
            else if (Mathf.Abs(deltaX) < xZoneTwoThreshold)
            {
                // X is in Zone Two
                xRotationSpeed = fastRotationSpeed;
            }
            else
            {
                xRotationSpeed = fastRotationSpeed;
            }
        }
        
        if (Mathf.Abs(deltaY) > centerDeadZoneHeight)
        {
            if (Mathf.Abs(deltaY) < yZoneOneThreshold)
            {
                // Y is in Zone One
                yRotationSpeed = slowRotationSpeed;
            }
            else if (Mathf.Abs(deltaY) < yZoneTwoThreshold)
            {
                // Y is in Zone Two
                yRotationSpeed = fastRotationSpeed;
            }
            else
            {
                yRotationSpeed = fastRotationSpeed;
            }
        }

        // Apply X rotation to the player body (left/right)
        if (xRotationSpeed > 0)
        {
            float xRotation = deltaX * xRotationSpeed * Time.deltaTime * sensitivity;
            playerBody.Rotate(Vector3.up, xRotation);
        }

        // Apply Y rotation to the camera (up/down)
        if (yRotationSpeed > 0)
        {
            // Get the current rotation in local Euler angles
            Vector3 cameraRotation = cameraTransform.localEulerAngles;

            // Calculate the new pitch
            float yRotation = deltaY * yRotationSpeed * Time.deltaTime * sensitivity;
            cameraRotation.x -= yRotation; // Pitch camera up/down

            // Adjust camera rotation to ensure smooth vertical movement
            // Convert from 0-360 range to -180 to 180
            if (cameraRotation.x > 180)
            {
                cameraRotation.x -= 360;
            }
            
            // Clamp the pitch to avoid flipping
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, pitchClampBottom, pitchClampTop);

            // Apply the rotation
            cameraTransform.localEulerAngles = new Vector3(cameraRotation.x, cameraTransform.localEulerAngles.y, 0);
        }
    }


}



