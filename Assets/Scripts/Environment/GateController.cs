using UnityEngine;

public class GateController : MonoBehaviour
{
    [SerializeField] GameObject leftGatePart;  // One half of the gate
    [SerializeField] GameObject rightGatePart; // The other half
    [SerializeField] float openAngle = 90f;    // Angle to open the gate
    [SerializeField] float openSpeed = 3f;     // Speed at which the gate opens/closes

    private bool isOpen = false; // Track whether the gate is open or closed
    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;
    private Quaternion leftOpenRotation;
    private Quaternion rightOpenRotation;

    void Start()
    {
        // Store initial rotations
        leftClosedRotation = leftGatePart.transform.localRotation;
        rightClosedRotation = rightGatePart.transform.localRotation;

        // Calculate open rotations
        leftOpenRotation = leftClosedRotation * Quaternion.Euler(0, openAngle, 0);
        rightOpenRotation = rightClosedRotation * Quaternion.Euler(0, -openAngle, 0);
    }

    public void ToggleGate()
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        // Smoothly rotate the gate parts to the target rotations
        Quaternion targetLeftRotation = isOpen ? leftOpenRotation : leftClosedRotation;
        Quaternion targetRightRotation = isOpen ? rightOpenRotation : rightClosedRotation;

        leftGatePart.transform.localRotation = Quaternion.Slerp(leftGatePart.transform.localRotation, targetLeftRotation, Time.deltaTime * openSpeed);
        rightGatePart.transform.localRotation = Quaternion.Slerp(rightGatePart.transform.localRotation, targetRightRotation, Time.deltaTime * openSpeed);
    }
}
