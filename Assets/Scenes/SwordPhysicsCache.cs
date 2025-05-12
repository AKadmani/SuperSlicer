using UnityEngine;

/// <summary>
/// Computes and caches the sword’s smoothed kinematic data each physics step.
/// Attach to the same GameObject that holds SwordCutter (the controller).
/// </summary>
public class SwordPhysicsCache : MonoBehaviour
{
    /* ---------- tuning knobs ---------- */
    [Header("Filtering")]
    [Range(0f, 1f)] public float alpha = 0.15f;      // low-pass weight (0 = max smooth)

    [Header("Debug")]
    public bool drawVectors = false;                 // draws gizmos for inspection

    /* ---------- public read-only props ---------- */
    public static Vector3 CurrentLinearAcceleration { get; private set; }   // m/s²
    public static Vector3 CurrentAngularVelocity { get; private set; }   // rad/s
    public static Vector3 CurrentAngularAcceleration { get; private set; }   // rad/s²

    /* ---------- private state ---------- */
    Vector3 prevPos, prevVel;
    Quaternion prevRot;
    Vector3 prevAngVel;

    void Awake()
    {
        prevPos = transform.position;
        prevRot = transform.rotation;
        prevVel = Vector3.zero;
        prevAngVel = Vector3.zero;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;                       // ~0.02 s at 50 Hz

        /* ----- linear motion ----- */
        Vector3 vel = (transform.position - prevPos) / dt;
        Vector3 accel = (vel - prevVel) / dt;

        /* ---------- angular motion ---------- */
        Quaternion deltaQ = transform.rotation * Quaternion.Inverse(prevRot);
        deltaQ.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;               // shortest path
        Vector3 angVel = axis.normalized * Mathf.Deg2Rad * angleDeg / dt;   // rad/s
        Vector3 angAccel = (angVel - prevAngVel) / dt;

        /* ---------- low-pass filter ---------- */
        CurrentLinearAcceleration = Vector3.Lerp(CurrentLinearAcceleration, accel, alpha);
        CurrentAngularVelocity = Vector3.Lerp(CurrentAngularVelocity, angVel, alpha);
        CurrentAngularAcceleration = Vector3.Lerp(CurrentAngularAcceleration, angAccel, alpha);

        /* ---------- optional gizmos ---------- */
        if (drawVectors)
        {
            Debug.DrawRay(transform.position, CurrentLinearAcceleration * 0.01f, Color.red, dt);
            Debug.DrawRay(transform.position, CurrentAngularVelocity * 0.05f, Color.blue, dt);
        }

        /* ---------- roll state forward ---------- */
        prevPos = transform.position;
        prevVel = vel;
        prevRot = transform.rotation;
        prevAngVel = angVel;
    }
}
