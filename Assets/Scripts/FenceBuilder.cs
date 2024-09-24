using System;
using UnityEngine;
using UnityEngine.AI;

public class FenceBuilder : MonoBehaviour
{
    [Header("Fence Settings")]
        public GameObject fencePrefab; // The fence or wall prefab to instantiate
        public GameObject ghostFencePrefab; // The "ghost" fence prefab to visualize placement
        public LayerMask groundLayer; // LayerMask to identify ground
        public LayerMask fenceLayer; 
        public float fenceSegmentLength = 1.0f; // Length of each fence segment

    [Header("Player and Camera Settings")]
        public GameObject playerBody; // The player's body to freeze during build mode
        public Camera overheadCamera; // The camera used for build mode
        public Camera playerCamera; // The player's main camera
        public float moveSpeed = 20f; // Speed at which the camera moves in build mode
        public float overheadHeight = 20f; // Adjustable height for the overhead camera
        public float zoomSpeed = 20f;
        public float maxZoom = 100f;
        public float minZoom = 20f;

    private bool isBuildModeActive = false;
    private Vector3? placementPoint = null;
    private GameObject ghostFenceSegment; // Current ghost fence segment
    private float currentRotation = 0f; // Rotation angle for the fence segments

    void Start()
    {
        overheadCamera.gameObject.SetActive(false);
        ghostFenceSegment = Instantiate(ghostFencePrefab);
        ghostFenceSegment.SetActive(false);
    }

    void Update()
    {
        // Toggle Build Mode
        if (Input.GetKeyDown(KeyCode.B)) // Press B to toggle build mode
        {
            ToggleBuildMode();
        }

        if (isBuildModeActive)
        {
            HandleBuildModeInput();

            // Handle Fence Deletion
            HandleFenceDeletion();
        }
    }

    void ToggleBuildMode()
    {
        isBuildModeActive = !isBuildModeActive;

        if (isBuildModeActive)
        {
            // Enter build mode
            PositionOverheadCamera();
            overheadCamera.gameObject.SetActive(true);
            playerCamera.gameObject.SetActive(false);
            playerBody.GetComponent<CharacterController>().enabled = false; // Freeze player movement
            ghostFenceSegment.SetActive(true);
        }
        else
        {
            // Exit build mode
            overheadCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
            playerBody.GetComponent<CharacterController>().enabled = true; // Unfreeze player movement
            ghostFenceSegment.SetActive(false);
            placementPoint = null;
        }
    }

    void PositionOverheadCamera()
    {
        // Position the overhead camera above the player's current position at the specified height
        Vector3 playerPosition = playerBody.transform.position;
        overheadCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y + overheadHeight, playerPosition.z);
        overheadCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Point camera straight down
    }

    void HandleBuildModeInput()
    {
        // WASD camera movement
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // Move the camera based on WASD input
        overheadCamera.transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        // Zoom in and out using mouse scroll wheel and arrow keys
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float zoomInput = 0;

        // Check if up or down arrow keys are pressed
        if (Input.GetKey(KeyCode.F))
        {
            zoomInput = 1;
        }
        else if (Input.GetKey(KeyCode.R))
        {
            zoomInput = -1;
        }

        // Combine scroll and key input for zooming
        float zoomAmount = (scrollInput + zoomInput) * zoomSpeed * Time.deltaTime;

        // Adjust the camera's position on the Y-axis to zoom in or out
        Vector3 newPosition = overheadCamera.transform.position + new Vector3(0, zoomAmount, 0);

        // Clamp the Y position to ensure it stays within the desired zoom range
        newPosition.y = Mathf.Clamp(newPosition.y, minZoom, maxZoom);

        // Apply the clamped position back to the camera
        overheadCamera.transform.position = newPosition;

        UpdateGhostFence();

        // Rotate the fence using Q and E
        if (Input.GetKey(KeyCode.Q))
        {
            currentRotation -= 90f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentRotation += 90f * Time.deltaTime;
        }

        // Apply rotation to the ghost fence
        ghostFenceSegment.transform.rotation = Quaternion.Euler(0, currentRotation, 0);

        // Place Fence on Left Click without CTRL
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
        {
            PlaceFence();
        }
    }

    void HandleFenceDeletion()
    {

        // Check if CTRL is held and left mouse button is clicked
        if (Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            Debug.Log("Holding ctrl");
            Ray ray = overheadCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast to detect fence segments
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, fenceLayer))
            {
                Debug.Log("Hit: " + hit.collider.gameObject.name + " with tag: " + hit.collider.gameObject.tag);

                // Check if the hit object is a fence segment
                if (hit.collider != null && hit.collider.CompareTag("Fence"))
                {
                    Debug.Log("found a fence");
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    void UpdateGhostFence()
    {
        Ray ray = overheadCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            placementPoint = hit.point;

            // Align ghost fence to the grid
            ghostFenceSegment.transform.position = placementPoint.Value;
        }
    }

    void PlaceFence()
    {
        if (placementPoint.HasValue)
        {
            // Instantiate the fence segment at the ghost's position and rotation
            GameObject newFence = Instantiate(fencePrefab, ghostFenceSegment.transform.position, ghostFenceSegment.transform.rotation);

            // Assign the FenceSegment tag to the new fence
            newFence.tag = "Fence";

            // Add a NavMeshObstacle component to make the fence an obstacle
            NavMeshObstacle obstacle = newFence.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
            {
                newFence.AddComponent<NavMeshObstacle>().carving = true;
            }
        }
    }
}
