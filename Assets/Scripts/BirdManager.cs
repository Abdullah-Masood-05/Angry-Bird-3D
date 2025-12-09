using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using TMPro;

public class BirdManager : MonoBehaviour
{
    [Header("Birds")]
    public GameObject[] birds;
    public Transform spawnPoint;
    public UIManager uiManager;

    [Header("Camera")]
    public FixedSlingshotCamera slingshotCamera;

    [Header("Transition")]
    public float transitionDuration = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Gameplay")]
    public int maxBirds = 3;
    public float destroyDelay = 5f;
    public float nextSpawnDelay = 2f;

    [Header("UI")]
    public TextMeshProUGUI winLoseText;   // Drag your WinLose TMP text here

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

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
            Debug.LogError("BirdManager: No birds assigned!");
            return;
        }

        // Store original transforms
        originalPositions = new Vector3[birds.Length];
        originalRotations = new Quaternion[birds.Length];

        for (int i = 0; i < birds.Length; i++)
        {
            if (birds[i] != null)
            {
                originalPositions[i] = birds[i].transform.position;
                originalRotations[i] = birds[i].transform.rotation;

                Rigidbody rb = birds[i].GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
        }

        maxBirds = Mathf.Min(maxBirds, birds.Length);
        SpawnNextBird();
    }

    public void SpawnNextBird()
    {
        // OUT OF BIRDS → CHECK LOSS
        if (currentBirdIndex >= maxBirds || currentBirdIndex >= birds.Length)
        {
            Debug.Log("BirdManager: No more birds available.");

            // LOSS CONDITION
            if (TargetManager.Instance != null && !TargetManager.Instance.AllTargetsDestroyed())
            {
                LoseGame();
            }

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
            Debug.LogError("BirdManager: Bird is null!");
            return;
        }

        StartCoroutine(TransitionBirdToSpawnPoint(
            currentBird,
            originalPositions[currentBirdIndex],
            originalRotations[currentBirdIndex]
        ));

        UpdateUI(); // Update UI without incrementing the count here
    }

    private IEnumerator TransitionBirdToSpawnPoint(GameObject bird, Vector3 fromPosition, Quaternion fromRotation)
    {
        isTransitioning = true;

        float elapsed = 0f;

        Vector3 targetPosition = spawnPoint.position;
        Quaternion targetRotation = spawnPoint.rotation;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);

            bird.transform.position = Vector3.Lerp(fromPosition, targetPosition, t);
            bird.transform.rotation = Quaternion.Slerp(fromRotation, targetRotation, t);

            yield return null;
        }

        bird.transform.position = targetPosition;
        bird.transform.rotation = targetRotation;

        BirdLaunch bl = bird.GetComponent<BirdLaunch>();
        if (bl != null)
        {
            bl.Setup(spawnPoint.position, this);
        }

        isTransitioning = false;
    }

    public void OnBirdLaunched(GameObject launchedBird)
    {
        if (launchedBird != currentBird) return;

        StartCoroutine(DestroyAndSpawnNextAfterDelay(launchedBird, destroyDelay));
    }

    private IEnumerator DestroyAndSpawnNextAfterDelay(GameObject bird, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bird != null)
            Destroy(bird);

        // ---- FIX: decrement bird count AFTER destruction ----
        currentBirdIndex++;
        UpdateUI();
        // -----------------------------------------------------

        yield return new WaitForSeconds(nextSpawnDelay);

        SpawnNextBird();
    }

    private void UpdateUI()
    {
        if (uiManager != null)
            uiManager.SetBirdsLeft(maxBirds - currentBirdIndex);

        Debug.Log("Birds Left: " + (maxBirds - currentBirdIndex));
    }

    // private void LoseGame()
    // {
    //     Debug.Log("GAME OVER — YOU LOSE");

    //     if (winLoseText != null)
    //     {
    //         winLoseText.text = "YOU LOSE!";
    //         winLoseText.gameObject.SetActive(true);
    //     }

    //     Time.timeScale = 0.4f; // Optional dramatic slow-motion
    // }
    private void LoseGame()
    {
        Debug.Log("GAME OVER — YOU LOSE");

        // APPLY SCORE PENALTY
        if (TargetManager.Instance != null)
            TargetManager.Instance.ApplyLossPenalty();

        Time.timeScale = 0.4f; // dramatic slow-motion
    }

}
