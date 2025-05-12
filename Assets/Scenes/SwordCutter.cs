using UnityEngine;

/// <summary>
/// Detects fast passes through “Ball” triggers and tells the target
/// exactly where the slice plane is.
/// </summary>
/// 
[RequireComponent(typeof(Collider))]
public class SwordCutter : MonoBehaviour
{
    [Header("Cut-detection")]
    [Tooltip("Minimum sword speed (m/s) required to register a slice.")]
    public float velocityThreshold = 1.0f;

    private Vector3 previousPos;
    private Vector3 currentVelocity;
    private Collider swordCol;

    // Store where we entered the target so we can build the plane.
    private Vector3? entryWorldPt;

    // Choose which local axis of the sword model is the flat of the blade.
    // For most FBX imports the right axis points across the blade.
    [SerializeField] private Vector3 bladeNormalLocal = Vector3.right;

    void Start()
    {
        previousPos = transform.position;
        swordCol = GetComponent<Collider>();
    }

    void Update()
    {
        currentVelocity = (transform.position - previousPos) / Time.deltaTime;
        previousPos = transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        if (currentVelocity.magnitude < velocityThreshold) return;

        // Get the exact world point where our collider first touched the sphere.
        entryWorldPt = swordCol.ClosestPoint(other.transform.position);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        if (!entryWorldPt.HasValue) return;
        if (currentVelocity.magnitude < velocityThreshold) { entryWorldPt = null; return; }

        // Build the plane from the entry/exit mid-point and the sword’s blade orientation.
        Vector3 exitWorldPt = swordCol.ClosestPoint(other.transform.position);
        if (Vector3.Distance(entryWorldPt.Value, exitWorldPt) < 0.01f) { entryWorldPt = null; return; }

        Vector3 planePoint = (entryWorldPt.Value + exitWorldPt) * 0.5f;
        Vector3 planeNormal = transform.TransformDirection(bladeNormalLocal).normalized;

        // Optional: visual debug
        Debug.DrawRay(planePoint, planeNormal * 0.25f, Color.magenta, 1.5f);
        Debug.DrawLine(entryWorldPt.Value, exitWorldPt, Color.cyan, 1.5f);

        // Tell the ball to slice itself.
        if (other.TryGetComponent(out ThrowableBall ball))
        {
            ball.HandleCut(planePoint, planeNormal, currentVelocity);
        }

        entryWorldPt = null;
    }
}