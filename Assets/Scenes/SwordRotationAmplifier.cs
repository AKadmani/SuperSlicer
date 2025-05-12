using UnityEngine;

/// <summary>
/// Boosts (or damps) controller rotation before it reaches a child model.
/// Put this on the XR controller object and drag your visual model into
/// `targetTransform`.  A factor of 1 = normal, 1.5 = 50 % more bend, etc.
/// </summary>
public class SwordRotationAmplifier : MonoBehaviour
{
    [Tooltip("The transform that should display the amplified rotation.")]
    public Transform targetTransform;

    [Range(0.5f, 3f)]
    public float rotationScale = 1.5f;     // tweak in play mode

    private Quaternion neutralGrip;        // grip orientation at start

    void Awake()
    {
        if (targetTransform == null)
            Debug.LogError("SwordRotationAmplifier: assign Target Transform", this);

        neutralGrip = transform.rotation;   // remember “at-rest” wrist pose
    }

    void LateUpdate()                       // after XR updates controller
    {
        // 1. raw delta from neutral
        Quaternion delta = transform.rotation * Quaternion.Inverse(neutralGrip);

        // 2. convert to axis/angle so we can scale the angle
        delta.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (float.IsNaN(angleDeg)) return;  // rare numerical edge case

        // 3. amplify + clamp to 180° to avoid flips
        float boostedAngle = Mathf.Clamp(angleDeg * rotationScale, 0f, 179.9f);

        // 4. re-assemble the boosted rotation
        Quaternion boostedDelta = Quaternion.AngleAxis(boostedAngle, axis);

        // 5. final orientation = boostedDelta * neutral
        targetTransform.rotation = boostedDelta * neutralGrip;

        // position stays unchanged (the XR system already updates it)
        targetTransform.position = transform.position;
    }
}
