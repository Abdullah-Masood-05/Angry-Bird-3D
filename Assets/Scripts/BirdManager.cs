// using UnityEngine;

// public class BirdManager : MonoBehaviour
// {
//     public GameObject birdPrefab; // Assign prefab in inspector
//     public Transform spawnPoint;  // Where the bird appears
//     public int maxBirds = 3;

//     private int birdsThrown = 0;
//     private GameObject currentBird;

//     void Start()
//     {
//         SpawnBird();
//         spawnPoint.position = new Vector3(49.02f, 7.3f, -1.5225f);
//     }

//     public void SpawnBird()
//     {
//         if (birdsThrown >= maxBirds)
//         {
//             Debug.Log("All birds used. Game Over!");
//             return;
//         }

//         currentBird = Instantiate(birdPrefab, spawnPoint.position, Quaternion.identity);
//         birdsThrown++;
//     }

//     public void BirdDestroyed()
//     {
//         // Destroy current bird and spawn next one after 1 sec
//         Destroy(currentBird);
//         Invoke(nameof(SpawnBird), 1f);
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;




public class BirdManager : MonoBehaviour
{
    [Header("Birds")]
    public GameObject[] birds;  // Assign birds in the Unity editor
    public Transform spawnPoint;
    public UIManager uiManager;

    //public CinemachineCamera followCamera;

    [Header("Transition")]
    public float transitionDuration = 1f;  // Duration for bird to move to spawn point
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Gameplay")]
    public int maxBirds = 3;
    public float destroyDelay = 4f;     // seconds after launch to destroy the thrown bird
    public float nextSpawnDelay = 1f;   // delay before next bird appears

    // internals
    private Vector3[] originalPositions;
    private GameObject currentBird;
    private int currentBirdIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("BirdManager: spawnPoint is not assigned!");
            return;
        }

        if (birds == null || birds.Length == 0)
        {
            Debug.LogError("BirdManager: No birds assigned in the array!");
            return;
        }

        // Store original positions of all birds
        originalPositions = new Vector3[birds.Length];
        for (int i = 0; i < birds.Length; i++)
        {
            if (birds[i] != null)
            {
                originalPositions[i] = birds[i].transform.position;

                // Make sure birds have rigidbody set to kinematic initially
                Rigidbody rb = birds[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }
        }

        // Limit to maxBirds
        maxBirds = Mathf.Min(maxBirds, birds.Length);

        // Start with the first bird
        SpawnNextBird();


    }

    public void SpawnNextBird()
    {
        if (currentBirdIndex >= maxBirds || currentBirdIndex >= birds.Length)
        {
            Debug.Log("BirdManager: No more birds available. Game Over.");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("BirdManager: Already transitioning a bird.");
            return;
        }

        currentBird = birds[currentBirdIndex];

        if (currentBird == null)
        {
            Debug.LogError($"BirdManager: Bird at index {currentBirdIndex} is null!");
            return;
        }

        // Start smooth transition from original position to spawn point
        StartCoroutine(TransitionBirdToSpawnPoint(currentBird, originalPositions[currentBirdIndex]));

        currentBirdIndex++;
        UpdateUI();
        // if (followCamera != null)
        // {
        //     followCamera.Follow = currentBird.transform;
        //     followCamera.LookAt = currentBird.transform;
        //     Debug.Log("BirdManager: Camera now following bird " + currentBird.name);
        // }

    }

    private IEnumerator TransitionBirdToSpawnPoint(GameObject bird, Vector3 fromPosition)
    {
        isTransitioning = true;

        float elapsed = 0f;
        Vector3 targetPosition = spawnPoint.position;

        // Smoothly move bird from original position to spawn point
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // Apply animation curve for smooth easing
            float curveValue = transitionCurve.Evaluate(t);

            bird.transform.position = Vector3.Lerp(fromPosition, targetPosition, curveValue);

            yield return null;
        }

        // Ensure final position is exact
        bird.transform.position = targetPosition;
        bird.transform.rotation = Quaternion.identity;

        // Setup bird for launching
        BirdLaunch bl = bird.GetComponent<BirdLaunch>();
        if (bl != null)
        {
            bl.Setup(spawnPoint.position, this);
        }
        else
        {
            Debug.LogWarning("BirdManager: Bird lacks BirdLaunch component.");
        }

        isTransitioning = false;
    }

    // Called by BirdLaunch when a bird has been launched (user released it)
    public void OnBirdLaunched(GameObject launchedBird)
    {
        // Only act if this is the currently tracked bird
        if (launchedBird != currentBird) return;

        // Start destruction + spawn sequence
        StartCoroutine(DestroyAndSpawnNextAfterDelay(launchedBird, destroyDelay));
    }

    private IEnumerator DestroyAndSpawnNextAfterDelay(GameObject bird, float delay)
    {
        // Wait for destroyDelay seconds (bird can fly/collide meanwhile)
        yield return new WaitForSeconds(delay);

        if (bird != null)
        {
            Destroy(bird);
        }

        // optionally spawn next after tiny gap
        yield return new WaitForSeconds(nextSpawnDelay);

        SpawnNextBird();
    }
    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.SetBirdsLeft(maxBirds - currentBirdIndex);
        }
        Debug.Log("Birds Left: " + (maxBirds - currentBirdIndex));
    }

}
