using UnityEngine;

public class BirdSplitAbility : MonoBehaviour
{
    [Header("Small Bird Prefab")]
    public GameObject smallBirdPrefab;

    [Header("Spread Settings")]
    public float spreadAngle = 15f;

    private bool abilityUsed = false;
    private Rigidbody rb;
    private BirdManager manager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        manager = FindFirstObjectByType<BirdManager>();
    }

    void Update()
    {
        // Only when bird is in flight (not kinematic)
        if (!abilityUsed && rb != null && !rb.isKinematic)
        {
            if (Input.GetMouseButtonDown(0)) // player tap/click
            {
                UseAbility();
            }
        }
    }

    private void UseAbility()
    {
        if (abilityUsed) return;
        abilityUsed = true;
        Debug.Log("BirdSplitAbility: Splitting bird now.");
        SplitNow();
    }

    private void SplitNow()
    {
        Vector3 originalVelocity = rb.linearVelocity;

        if (originalVelocity.magnitude < 1f)
            originalVelocity = transform.forward * 15f;

        Vector3 baseDir = originalVelocity.normalized;

        Vector3 dirLeft = Quaternion.Euler(0, -spreadAngle, 0) * baseDir;
        Vector3 dirRight = Quaternion.Euler(0, spreadAngle, 0) * baseDir;

        float speed = originalVelocity.magnitude;
        Debug.Log($"BirdSplitAbility: Spawning small birds with speed {speed:F1}");
        SpawnSmall(dirLeft, speed);
        SpawnSmall(baseDir, speed);
        SpawnSmall(dirRight, speed);

        // VERY IMPORTANT: notify manager before destroying
        if (manager != null)
            manager.OnBirdLaunched(gameObject);

        Destroy(gameObject);
    }

    private void SpawnSmall(Vector3 direction, float speed)
    {
        GameObject newBird = Instantiate(smallBirdPrefab, transform.position, Quaternion.identity);

        Rigidbody rbSmall = newBird.GetComponent<Rigidbody>();
        if (rbSmall != null)
        {
            rbSmall.isKinematic = false;
            rbSmall.linearVelocity = direction * speed;   // KEY FIX
        }

        // Make sure BirdLaunch does NOT override physics
        BirdLaunch bl = newBird.GetComponent<BirdLaunch>();
        if (bl != null)
        {
            bl.enabled = false;  // Prevent drag/launch logic from interfering
        }
    }
}
