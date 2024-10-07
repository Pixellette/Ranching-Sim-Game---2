using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class FenceBuilder : MonoBehaviour
{
    [Header("Fence Settings")]
        public GameObject fencePrefab;
        public GameObject ghostFencePrefab;
        public LayerMask groundLayer;
        public LayerMask fenceLayer;
        public float fenceSegmentLength = 1.0f;

    [Header("Player and Camera Settings")]
        public GameObject playerBody;
        public Camera overheadCamera;
        public Camera playerCamera;
        public float moveSpeed = 20f;
        public float overheadHeight = 20f;
        public float zoomSpeed = 20f;
        public float maxZoom = 100f;
        public float minZoom = 20f;

    [Header("UI Panels")]
        public GameObject buildModeUIPanel; // UI Panel for Build Mode
        public GameObject gameplayUIPanel;  // UI Panel for Normal Gameplay

    [Header("UI Buttons")]
        public Button placementModeButton;
        public Button deletionModeButton;



    // Hidden Variables 
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

    void Start()
    {
        overheadCamera.gameObject.SetActive(false);
        ghostFenceSegment = Instantiate(ghostFencePrefab);
        ghostFenceSegment.SetActive(false);

        // Ensure that the correct UI panels are initially visible
        buildModeUIPanel.SetActive(false);
        gameplayUIPanel.SetActive(true);
        UpdateButtonColors();
    }

    void Update()
    {
        // Toggle Build Mode with B
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
            PositionOverheadCamera();
            overheadCamera.gameObject.SetActive(true);
            playerCamera.gameObject.SetActive(false);
            playerBody.GetComponent<CharacterController>().enabled = false;
            ghostFenceSegment.SetActive(isPlacementMode);

            // Enable Build Mode UI and disable Gameplay UI
            buildModeUIPanel.SetActive(true);
            gameplayUIPanel.SetActive(false);
        }
        else
        {
            overheadCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
            playerBody.GetComponent<CharacterController>().enabled = true;
            ghostFenceSegment.SetActive(false);
            placementPoint = null;
            selectedFences.Clear();

            // Disable Build Mode UI and enable Gameplay UI
            buildModeUIPanel.SetActive(false);
            gameplayUIPanel.SetActive(true);
        }
    }

    void PositionOverheadCamera()
    {
        Vector3 playerPosition = playerBody.transform.position;
        overheadCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y + overheadHeight, playerPosition.z);
        overheadCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void HandleBuildModeInput()
    {
        // Camera movement
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        overheadCamera.transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        // Zoom in and out
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float zoomAmount = scrollInput * zoomSpeed * Time.deltaTime;
        Vector3 newPosition = overheadCamera.transform.position + new Vector3(0, zoomAmount, 0);
        newPosition.y = Mathf.Clamp(newPosition.y, minZoom, maxZoom);
        overheadCamera.transform.position = newPosition;

        if (isPlacementMode)
        {
            UpdateGhostFence();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceFence();
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

    void SelectFencesInArea()
    {
        Vector3 min = Vector3.Min(selectionStart, selectionEnd);
        Vector3 max = Vector3.Max(selectionStart, selectionEnd);
        Collider[] colliders = Physics.OverlapBox((min + max) / 2, (max - min) / 2, Quaternion.identity, fenceLayer);

        selectedFences.Clear();
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Fence"))
            {
                selectedFences.Add(collider.gameObject);
            }
        }

        Debug.Log($"Selected {selectedFences.Count} fences for deletion.");
    }

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

    void UpdateGhostFence()
    {
        Ray ray = overheadCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            placementPoint = hit.point;
            ghostFenceSegment.transform.position = placementPoint.Value;
        }
    }

    void PlaceFence()
    {
        if (placementPoint.HasValue)
        {
            GameObject newFence = Instantiate(fencePrefab, ghostFenceSegment.transform.position, ghostFenceSegment.transform.rotation);
            newFence.tag = "Fence";
            NavMeshObstacle obstacle = newFence.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
            {
                newFence.AddComponent<NavMeshObstacle>().carving = true;
            }
        }
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



}
