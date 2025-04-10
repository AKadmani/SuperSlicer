using UnityEngine;

public class ThrowableBall : MonoBehaviour
{
    public GameObject ballHalfPrefab; // Assign the "BallHalf" prefab in Inspector
    public float despawnDelayGround = 2.0f; // Time after hitting ground to despawn
    public float cutForceMagnitude = 0.8f; // How much halves fly apart
    public float despawnDelayCut = 1.5f; // Time after being cut to despawn halves

    private bool hasBeenCut = false;
    private bool hitGround = false;

    void OnCollisionEnter(Collision collision)
    {
        // Check if it hit the ground and hasn't been cut yet
        if (!hasBeenCut && !hitGround && collision.gameObject.CompareTag("Ground"))
        {
            hitGround = true;
            // Start despawn timer only if it hits the ground *before* being cut
            Invoke(nameof(Despawn), despawnDelayGround);
        }
    }

    public void HandleCut(Vector3 cutEnterPoint, Vector3 cutExitPoint, Vector3 cutterVelocity)
    {
        if (hasBeenCut) return; // Prevent cutting multiple times
        hasBeenCut = true;

        // Disable original ball immediately
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true; // Stop physics interaction

        // Find cut direction and plane normal
        Vector3 cutDirection = (cutExitPoint - cutEnterPoint).normalized;
        Vector3 flyApartDirection = Vector3.Cross(cutterVelocity.normalized, cutDirection).normalized;
        if (flyApartDirection == Vector3.zero) // Handle case where velocity is parallel to cut
        {
            flyApartDirection = Vector3.Cross(Vector3.up, cutDirection).normalized;
            if (flyApartDirection == Vector3.zero) flyApartDirection = Vector3.right; // Failsafe
        }


        // Instantiate two halves
        GameObject half1 = Instantiate(ballHalfPrefab, transform.position, Quaternion.LookRotation(flyApartDirection));
        GameObject half2 = Instantiate(ballHalfPrefab, transform.position, Quaternion.LookRotation(-flyApartDirection));

        // Add force to make them fly apart
        Rigidbody rb1 = half1.GetComponent<Rigidbody>();
        Rigidbody rb2 = half2.GetComponent<Rigidbody>();

        if (rb1) rb1.AddForce(flyApartDirection * cutForceMagnitude, ForceMode.Impulse);
        if (rb2) rb2.AddForce(-flyApartDirection * cutForceMagnitude, ForceMode.Impulse);

        // Destroy halves after a delay
        Destroy(half1, despawnDelayCut);
        Destroy(half2, despawnDelayCut);

        // Notify GameManager about the successful cut
        GameManager.Instance?.IncrementScore();

        // Destroy the original ball object shortly after
        Destroy(gameObject, 0.1f);
    }


    void Despawn()
    {
        // Only despawn if it hasn't been cut in the meantime
        if (!hasBeenCut)
        {
            Destroy(gameObject);
        }
    }

    // Optional: Add this if you want the ball to despawn if it flies too far away
    void Update()
    {
        if (transform.position.y < -10) // Adjust threshold as needed
        {
            Destroy(gameObject);
        }
    }
}