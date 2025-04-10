using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab; // Assign ThrowableBall prefab in Inspector
    public float minSpawnDelay = 1.0f;
    public float maxSpawnDelay = 3.0f;
    public float launchForceMin = 5f;
    public float launchForceMax = 6f;
    public float horizontalSpread = 0.3f; // How much side-to-side variation

    private bool isSpawning = false;

    void Start()
    {
        // Optionally start spawning immediately, or wait for GameManager
        // StartSpawning();
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            Invoke(nameof(SpawnBall), Random.Range(minSpawnDelay, maxSpawnDelay));
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        CancelInvoke(nameof(SpawnBall)); // Stop any pending spawns
    }


    void SpawnBall()
    {
        if (!isSpawning || ballPrefab == null) return; // Stop if spawning turned off or no prefab

        // Calculate spawn position with some horizontal variation
        Vector3 spawnPos = transform.position + Vector3.right * Random.Range(-horizontalSpread, horizontalSpread);

        GameObject newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        Rigidbody rb = newBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Launch upwards with slight randomness
            float launchForce = Random.Range(launchForceMin, launchForceMax);
            Vector3 forceDirection = Vector3.up + Vector3.right * Random.Range(-0.1f, 0.1f) + Vector3.forward * Random.Range(-0.05f, 0.05f); // Slight angle variation
            rb.AddForce(forceDirection.normalized * launchForce, ForceMode.Impulse);
            // Optional: Add some random torque
            rb.AddTorque(Random.insideUnitSphere * 5f);
        }

        // Schedule the next spawn
        if (isSpawning) // Check again in case StopSpawning was called meanwhile
        {
            Invoke(nameof(SpawnBall), Random.Range(minSpawnDelay, maxSpawnDelay));
        }
    }
}