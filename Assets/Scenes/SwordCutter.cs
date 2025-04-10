using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Need this for velocity

public class SwordCutter : MonoBehaviour
{
    public float velocityThreshold = 1.0f; // Minimum speed to register a cut

    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    private Collider swordCollider; // Reference to its own collider

    // Store points for potential cut calculation
    private Vector3? entryPoint = null;

    void Start()
    {
        previousPosition = transform.position;
        swordCollider = GetComponent<Collider>();
        if (swordCollider == null)
        {
            Debug.LogError("SwordCutter needs a Collider component on the same GameObject!", this);
        }
    }

    void Update()
    {
        // Calculate velocity manually (XR controller velocity can be tricky/noisy sometimes)
        currentVelocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if we hit a ball and are moving fast enough
        if (other.CompareTag("Ball") && currentVelocity.magnitude > velocityThreshold)
        {
            // Store the entry point *relative to the ball*
            entryPoint = other.transform.InverseTransformPoint(swordCollider.ClosestPoint(other.transform.position));
            // Alternative: use world space point
            // entryPoint = swordCollider.ClosestPoint(other.transform.position);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if we are exiting a ball we previously entered, and are still fast enough
        if (entryPoint.HasValue && other.CompareTag("Ball") && currentVelocity.magnitude > velocityThreshold)
        {
            ThrowableBall ball = other.GetComponent<ThrowableBall>();
            if (ball != null)
            {
                // Calculate exit point
                Vector3 exitPoint = other.transform.InverseTransformPoint(swordCollider.ClosestPoint(other.transform.position));
                // Alternative: use world space point
                // Vector3 exitPoint = swordCollider.ClosestPoint(other.transform.position);

                // Tell the ball it's been cut
                // Pass world space points and velocity for easier calculation in the ball script
                ball.HandleCut(
                    other.transform.TransformPoint(entryPoint.Value), // Convert entry back to world
                    other.transform.TransformPoint(exitPoint),        // Convert exit back to world
                    currentVelocity);

                // Reset entry point
                entryPoint = null;
            }
        }
        // else if (other.CompareTag("Ball")) // Reset if exiting ball regardless of cut success
        // {
        //     entryPoint = null; // Reset if we exit the trigger slowly or it wasn't a ball
        // }
    }

    // Important: Change the Ball's Collider to be a Trigger!
    // Go back to the ThrowableBall prefab and check "Is Trigger" on its Sphere Collider.
}