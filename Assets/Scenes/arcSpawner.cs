using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns balls in a 100-degree horizontal arc in front of the player,
/// always within reach and travelling slightly toward the torso.
/// </summary>
public class arcSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject ballPrefab;
    public Transform xrOrigin;           // usually the XR Origin root

    [Header("Spawn timing")]
    public float minInterval = 0.8f;     // seconds
    public float maxInterval = 1.8f;

    [Header("Arc & distance")]
    [Range(10f, 180f)] public float arcDegrees = 100f;
    public float minDistance = 1.0f;     // metres from player
    public float maxDistance = 2.0f;

    [Header("Vertical bounds (relative to player)")]
    public float minHeight = 0.5f;       // waist
    public float maxHeight = 1.8f;       // head-ish

    [Header("Launch velocity")]
    public float forwardSpeed = 3.0f;    // towards player
    public float upwardSpeed = 2.0f;    // gentle lob

    public void StartSpawning() => enabled = true;   // enabling starts the coroutine
    public void StopSpawning() => enabled = false;  // disabling stops the coroutine
    void Awake() => enabled = false;   // start disabled; GameManager decides when

    void OnEnable() => StartCoroutine(SpawnLoop());
    void OnDisable() => StopAllCoroutines();

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);           // small startup delay
        while (true)
        {
            SpawnBall();
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    void SpawnBall()
    {
        // 1. pick a random azimuth in the arc centred on player.forward
        float halfArc = arcDegrees * 0.5f;
        float angle = Random.Range(-halfArc, halfArc);
        Quaternion yaw = Quaternion.AngleAxis(angle, Vector3.up);

        // 2. pick a random distance and vertical offset
        float dist = Random.Range(minDistance, maxDistance);
        float yOff = Random.Range(minHeight, maxHeight);

        // 3. compute world spawn position
        Vector3 dir = yaw * xrOrigin.forward;          // horizontal direction
        Vector3 spawnPos = xrOrigin.position + dir * dist;
        spawnPos.y += yOff;

        // 4. instantiate ball
        GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

        // 5. give it an initial velocity that aims roughly at the player’s torso
        if (ball.TryGetComponent(out Rigidbody rb))
        {
            Vector3 target = xrOrigin.position + Vector3.up * 1.2f;  // 1.2 m ≈ chest
            Vector3 vel = (target - spawnPos).normalized * forwardSpeed;
            vel.y += upwardSpeed;   // add arc
            rb.linearVelocity = vel;
        }
    }
}
