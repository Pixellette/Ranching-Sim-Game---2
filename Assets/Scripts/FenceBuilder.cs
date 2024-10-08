using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class FenceBuilder : MonoBehaviour
{
    [Header("Fence Settings")]
        [SerializeField] GameObject fencePrefab;
        [SerializeField] GameObject ghostFencePrefab;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] LayerMask fenceLayer;
        [SerializeField] float fenceSegmentLength = 1.0f;

    [Header("Gate Settings")]
        [SerializeField] GameObject gatePrefab;
        [SerializeField] GameObject ghostGatePrefab;
        [SerializeField] Button fencePlacementButton;
        [SerializeField] Button gatePlacementButton;

    [Header("Player and Camera Settings")]
        [SerializeField] GameObject playerBody;
        [SerializeField] Camera overheadCamera;
        [SerializeField] Camera playerCamera;
        [SerializeField] float moveSpeed = 20f;
        [SerializeField] float overheadHeight = 20f;
        [SerializeField] float zoomSpeed = 20f;
        [SerializeField] float maxZoom = 100f;
        [SerializeField] float minZoom = 20f;

    [Header("UI Panels")]
        [SerializeField] GameObject buildModeUIPanel; // UI Panel for Build Mode
        [SerializeField] GameObject gameplayUIPanel;  // UI Panel for Normal Gameplay

    [Header("UI Buttons")]
        [SerializeField] Button placementModeButton;
        [SerializeField] Button deletionModeButton;


    // ============================== Hidden Variables ==============================

    private bool isBuildModeActive = false;
    private bool isPlacementMode = true; // True for placement, false for deletion
    private Vector3? placementPoint = null;
    private GameObject ghostFenceSegment;
    private float currentRotation = 0f;

    // Selection variables for deletion
    private bool isSelecting = false;
    private Vector3 selectionStart;
    private Vector3 selectionEnd;
    private List<GameObject> selectedFences = new List<GameObject>();

    private bool isPlacingGate = false; // True if the user wants to place a gate, false for fence
    private GameObject ghostGateSegment;

    // ============================================================
    //                           METHODS 
    // ============================================================
    void Start()
    {
        overheadCamera.gameObject.SetActive(false);
        ghostFenceSegment = Instantiate(ghostFencePrefab);
        ghostFenceSegment.SetActive(false);
        ghostGateSegment = Instantiate(ghostGatePrefab);
        ghostGateSegment.SetActive(false);

        // Ensure that the correct UI panels are correctly set on start
        buildModeUIPanel.SetActive(false);
        gameplayUIPanel.SetActive(true);
        UpdateButtonColors();
        UpdatePlacementButtonColors();
    }

    void Update()
    {
        // Toggle Build Mode off and on using B
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }

        if (isBuildModeActive)
        {
            HandleBuildModeInput();

            if (!isPlacementMode)
            {
                HandleFenceSelection();
            }
        }
    }

    // ============================================================
    //                       Switching Modes
    // ============================================================


    // Method linked to HUD button for Placement Mode
    public void ActivatePlacementMode()
    {
        isPlacementMode = true;
        ghostFenceSegment.SetActive(true);
        selectedFences.Clear(); // Clear any selections if switching from delete mode
        UpdateButtonColors();
        Debug.Log("Switched to Placement Mode");
    }

    // Method linked to HUD button for Deletion Mode
    public void ActivateDeletionMode()
    {
        isPlacementMode = false;
        ghostFenceSegment.SetActive(false);
        UpdateButtonColors();
        Debug.Log("Switched to Deletion Mode");
    }


    void ToggleBuildMode()
    {
        isBuildModeActive = !isBuildModeActive;

        if (isBuildModeActive)
        {
            PositionOverheadCamera();  // Set the initial position based on the player
            overheadCamera.gameObject.SetActive(true);
            playerCamera.gameObject.SetActive(false);
            
            // Set PlayerMovements to build mode
            playerBody.GetComponent<PlayerMovement>().inBuildMode = true;

            ghostFenceSegment.SetActive(isPlacementMode);

            // Enable Build Mode UI and disable Gameplay UI
            buildModeUIPanel.SetActive(true);
            gameplayUIPanel.SetActive(false);

            // Pause the game (excluding UI and overhead camera)
            Time.timeScale = 0f;
        }
        else
        {
            overheadCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
            
            // Set PlayerMovements to normal mode
            playerBody.GetComponent<PlayerMovement>().inBuildMode = false;

            ghostFenceSegment.SetActive(false);
            placementPoint = null;
            selectedFences.Clear();

            // Disable Build Mode UI and enable Gameplay UI
            buildModeUIPanel.SetActive(false);
            gameplayUIPanel.SetActive(true);

            // Resume the game
            Time.timeScale = 1f;
        }
    }

    void PositionOverheadCamera()
    {
        // Set the overhead camera position to match the player's current position
        Vector3 playerPosition = playerBody.transform.position;
        overheadCamera.transform.position = new Vector3(playerPosition.x, overheadHeight, playerPosition.z);
        overheadCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void UpdateButtonColors()
    {
        Color activeColor = Color.white; // Light color for active state
        Color inactiveColor = Color.gray; // Dark color for inactive state

        if (isPlacementMode)
        {
            placementModeButton.GetComponent<Image>().color = activeColor;
            deletionModeButton.GetComponent<Image>().color = inactiveColor;
        }
        else
        {
            placementModeButton.GetComponent<Image>().color = inactiveColor;
            deletionModeButton.GetComponent<Image>().color = activeColor;
        }
    }

    public void ActivateFencePlacementMode()
    {
        isPlacingGate = false;
        isPlacementMode = true;
        ghostFenceSegment.SetActive(true);
        ghostGateSegment.SetActive(false);
        selectedFences.Clear(); // Clear any selections if switching from delete mode
        UpdateButtonColors();
        UpdatePlacementButtonColors();
        Debug.Log("Switched to Fence Placement Mode");
    }

    public void ActivateGatePlacementMode()
    {
        isPlacingGate = true;
        isPlacementMode = true;
        ghostFenceSegment.SetActive(false);
        ghostGateSegment.SetActive(true);
        selectedFences.Clear(); // Clear any selections if switching from delete mode
        UpdateButtonColors();
        UpdatePlacementButtonColors();
        Debug.Log("Switched to Gate Placement Mode");
    }

    void UpdatePlacementButtonColors()
    {
        Color activeColor = Color.white; // Light color for active state
        Color inactiveColor = Color.gray; // Dark color for inactive state

        if (isPlacingGate)
        {
            gatePlacementButton.GetComponent<Image>().color = activeColor;
            fencePlacementButton.GetComponent<Image>().color = inactiveColor;
        }
        else
        {
            gatePlacementButton.GetComponent<Image>().color = inactiveColor;
            fencePlacementButton.GetComponent<Image>().color = activeColor;
        }
    }




    // ============================================================
    //                       Placement Mode
    // ============================================================

    void HandleBuildModeInput()
    {
        if (isBuildModeActive)
        {
            // Camera movement using WASD keys 
            float moveX = 0f;
            float moveZ = 0f;

            if (Input.GetKey(KeyCode.W))
            {
                moveZ = 1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                moveZ = -1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                moveX = -1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                moveX = 1f;
            }

            // Calculate movement vector
            Vector3 move = new Vector3(moveX, 0, moveZ).normalized * moveSpeed * Time.unscaledDeltaTime;

            // Apply movement directly to the camera
            overheadCamera.transform.position += move;

            // Zoom in and out using R/F keys and mouse scroll wheel
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            float zoomInput = 0;

            if (Input.GetKey(KeyCode.F))
            {
                zoomInput = 1;
            }
            else if (Input.GetKey(KeyCode.R))
            {
                zoomInput = -1;
            }

            // Calculate and apply zoom amount
            float zoomAmount = (scrollInput + zoomInput) * zoomSpeed * Time.unscaledDeltaTime;
            Vector3 newPosition = overheadCamera.transform.position + new Vector3(0, zoomAmount, 0);
            newPosition.y = Mathf.Clamp(newPosition.y, minZoom, maxZoom);
            overheadCamera.transform.position = newPosition;

            // If in placement mode, handle ghost fence/gate placement and rotation
            if (isPlacementMode)
            {
                UpdateGhostFence();

                // Rotate the ghost segment using Q and E keys
                if (Input.GetKey(KeyCode.Q))
                {
                    currentRotation -= 90f * Time.unscaledDeltaTime;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    currentRotation += 90f * Time.unscaledDeltaTime;
                }

                // Apply rotation to the correct ghost segment (either fence or gate)
                if (isPlacingGate)
                {
                    ghostGateSegment.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
                }
                else
                {
                    ghostFenceSegment.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
                }

                // Place Fence or Gate on Left Click
                if (Input.GetMouseButtonDown(0))
                {
                    PlaceFence();
                }
            }
            else
            {
                HandleFenceSelection();
            }
        }
    }


    void HandleFenceSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            selectionStart = GetMouseWorldPosition();
        }

        if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            isSelecting = false;
            selectionEnd = GetMouseWorldPosition();
            SelectFencesInArea();
            DeleteSelectedFences();
        }

        if (isSelecting)
        {
            DrawSelectionBox();
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = overheadCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    void UpdateGhostFence()
    {
        Ray ray = overheadCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            placementPoint = hit.point;
            if (isPlacingGate)
            {
                ghostGateSegment.transform.position = placementPoint.Value;
            }
            else
            {
                ghostFenceSegment.transform.position = placementPoint.Value;
            }
        }
    }



    void PlaceFence()
{
    if (placementPoint.HasValue)
    {
        // Determine whether placing a fence or gate and get the correct ghost segment
        GameObject ghostSegment = isPlacingGate ? ghostGateSegment : ghostFenceSegment;
        GameObject prefabToPlace = isPlacingGate ? gatePrefab : fencePrefab;

        // Calculate start and end positions of the segment (fence or gate)
        Vector3 startPosition = ghostSegment.transform.position - ghostSegment.transform.forward * (fenceSegmentLength / 2);
        Vector3 endPosition = ghostSegment.transform.position + ghostSegment.transform.forward * (fenceSegmentLength / 2);

        // Perform raycasts at the start and end positions to determine the terrain slope
        Ray startRay = new Ray(startPosition + Vector3.up * 10, Vector3.down);
        Ray endRay = new Ray(endPosition + Vector3.up * 10, Vector3.down);

        Vector3 averagePosition = Vector3.zero;
        Vector3 averageNormal = Vector3.up;

        if (Physics.Raycast(startRay, out RaycastHit startHit, Mathf.Infinity, groundLayer) &&
            Physics.Raycast(endRay, out RaycastHit endHit, Mathf.Infinity, groundLayer))
        {
            // Average the position and normal to determine the average slope
            averagePosition = (startHit.point + endHit.point) / 2;
            averageNormal = (startHit.normal + endHit.normal).normalized;
        }
        else if (Physics.Raycast(startRay, out startHit, Mathf.Infinity, groundLayer))
        {
            // If only the start point hit, use that
            averagePosition = startHit.point;
            averageNormal = startHit.normal;
        }
        else if (Physics.Raycast(endRay, out endHit, Mathf.Infinity, groundLayer))
        {
            // If only the end point hit, use that
            averagePosition = endHit.point;
            averageNormal = endHit.normal;
        }

        // Set the segment rotation to align with the average normal of the terrain
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, averageNormal) * ghostSegment.transform.rotation;

        // Instantiate the new segment at the average position and with the calculated rotation
        GameObject newSegment = Instantiate(prefabToPlace, averagePosition, rotation);
        newSegment.tag = isPlacingGate ? "Gate" : "Fence";

        // Add a NavMeshObstacle component if it does not exist
        NavMeshObstacle obstacle = newSegment.GetComponent<NavMeshObstacle>();
        if (obstacle == null)
        {
            newSegment.AddComponent<NavMeshObstacle>().carving = true;
        }
    }
}





    // ============================================================
    //                       Deletion Mode
    // ============================================================

    void SelectFencesInArea()
    {
        Vector3 min = Vector3.Min(selectionStart, selectionEnd);
        Vector3 max = Vector3.Max(selectionStart, selectionEnd);
        Collider[] colliders = Physics.OverlapBox((min + max) / 2, (max - min) / 2, Quaternion.identity, fenceLayer);

        selectedFences.Clear();
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Fence") || collider.CompareTag("Gate"))
            {
                selectedFences.Add(collider.gameObject);
            }
        }

        Debug.Log($"Selected {selectedFences.Count} fences for deletion.");
    }

    // For Debugging only 
    void DrawSelectionBox()
    {
        Debug.DrawLine(selectionStart, new Vector3(selectionStart.x, selectionStart.y, selectionEnd.z), Color.red);
        Debug.DrawLine(selectionStart, new Vector3(selectionEnd.x, selectionStart.y, selectionStart.z), Color.red);
        Debug.DrawLine(selectionEnd, new Vector3(selectionEnd.x, selectionStart.y, selectionEnd.z), Color.red);
        Debug.DrawLine(selectionEnd, new Vector3(selectionStart.x, selectionStart.y, selectionEnd.z), Color.red);
    }

    void DeleteSelectedFences()
    {
        foreach (GameObject fence in selectedFences)
        {
            Destroy(fence);
        }
        selectedFences.Clear();
    }

}
