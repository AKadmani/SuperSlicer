using UnityEngine;
using EzySlice;

/// <summary>
/// Lets the ball slice itself using the plane supplied by SwordCutter.
/// Handles physics and clean-up.
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(Renderer))]
public class ThrowableBall : MonoBehaviour
{
    [Header("Materials")]
    public Material crossSectionMaterial;

    [Header("Timing")]
    public float despawnDelayGround = 2.0f;
    public float despawnDelayCut = 1.5f;

    [Header("Slice physics")]
    [Tooltip("Scale factor applied to sword speed when kicking the halves apart.")]
    public float cutImpulseFactor = 0.5f;     // tweak to taste

    private bool hasBeenCut = false;
    private bool hitGround = false;

    void OnCollisionEnter(Collision col)
    {
        if (!hasBeenCut && !hitGround && col.gameObject.CompareTag("Ground"))
        {
            hitGround = true;
            Invoke(nameof(Despawn), despawnDelayGround);
        }
    }

    /// <summary>
    /// Called by SwordCutter with a ready-made plane.
    /// </summary>
    public void HandleCut(Vector3 planePoint, Vector3 planeNormal, Vector3 swordVelocity)
    {
        if (hasBeenCut) return;

        // ----- try the slice first -----
        SlicedHull hull = gameObject.Slice(planePoint, planeNormal, crossSectionMaterial);
        if (hull == null)
        {
            // slice failed – leave the ball exactly as it was
            return;
        }

        hasBeenCut = true;

        // Hide / freeze the original
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        GameObject lower = hull.CreateLowerHull(gameObject, crossSectionMaterial);
        GameObject upper = hull.CreateUpperHull(gameObject, crossSectionMaterial);
        SetupPiece(lower);
        SetupPiece(upper);

        float impulseMag = swordVelocity.magnitude * cutImpulseFactor;

        if (lower.TryGetComponent(out Rigidbody rbL))
            rbL.AddForce(-planeNormal * impulseMag, ForceMode.Impulse);

        if (upper.TryGetComponent(out Rigidbody rbU))
            rbU.AddForce(planeNormal * impulseMag, ForceMode.Impulse);

        GameManager.Instance?.IncrementScore();

        Destroy(lower, despawnDelayCut);
        Destroy(upper, despawnDelayCut);
        Destroy(gameObject, 0.1f);      // remove the disabled original
    }

    private void SetupPiece(GameObject piece)
    {
        piece.transform.SetPositionAndRotation(transform.position, transform.rotation);

        var col = piece.AddComponent<MeshCollider>();
        col.convex = true;

        piece.AddComponent<Rigidbody>().mass = 1f;
    }

    void Despawn() => Destroy(gameObject);

    void Update()
    {
        if (transform.position.y < -10f) Destroy(gameObject);
    }
}
