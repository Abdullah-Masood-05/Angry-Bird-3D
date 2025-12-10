using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class BirdManager : MonoBehaviour
{
    [Header("Birds")]
    public GameObject[] birds;
    public Transform spawnPoint;
    public UIManager uiManager;
    public TextMeshProUGUI timerText;
    public float levelTimeLimit = 20f;
    private float timeRemaining;
    int totalScore = 0;
    private bool timerRunning = false;

    public float delayBeforeNextLevel = 2f;
    [Header("Transition")]
    public float transitionDuration = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Gameplay")]
    public int maxBirds = 3;
    public float destroyDelay = 5f;
    public float nextSpawnDelay = 2f;

    [Header("UI")]
    public TextMeshProUGUI winLoseText;

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    private GameObject currentBird;
    private int currentBirdIndex = 0;
    private bool isTransitioning = false;
    private bool levelComplete = false;

    void Start()
    {
        timeRemaining = levelTimeLimit;
        UpdateTimerUI();
        StartTimer();
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
    void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            Debug.Log("timeremaining: " + timeRemaining);
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                TimerExpired();
            }
        }
    }

    private void TimerExpired()
    {
        if (levelComplete) return;

        StopTimer();
        levelComplete = true;

        Debug.Log("Bird Manager: TIME'S UP!");
        StartCoroutine(WinSequence());

    }
    private void StopTimer()
    {
        timerRunning = false;
    }
    private IEnumerator WinSequence()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int lastLevelIndex = SceneManager.sceneCountInBuildSettings - 1;

        // LAST LEVEL → FINAL WIN SCREEN
        if (currentIndex == lastLevelIndex)
        {
            if (winLoseText != null)
            {
                winLoseText.text =
                    "ALL LEVELS COMPLETE!\nYOU WIN!\n\nFINAL SCORE: " + totalScore;

                winLoseText.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(delayBeforeNextLevel);
            Debug.Log("GAME FINISHED");
            yield break;
        }

        // NORMAL LEVEL WIN
        if (winLoseText != null)
        {
            winLoseText.text =
                "LEVEL COMPLETE!\n+" + " points";

            winLoseText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeNextLevel);
        SceneManager.LoadScene(currentIndex + 1);
    }

    public void SpawnNextBird()
    {

        if (currentBirdIndex >= maxBirds || currentBirdIndex >= birds.Length)
        {
            Debug.Log("BirdManager: No more birds available.");

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

        UpdateUI();
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
        currentBirdIndex++;
        UpdateUI();

        yield return new WaitForSeconds(nextSpawnDelay);

        SpawnNextBird();
    }

    private void UpdateUI()
    {
        if (uiManager != null)
            uiManager.SetBirdsLeft(maxBirds - currentBirdIndex);

        Debug.Log("Birds Left: " + (maxBirds - currentBirdIndex));
    }
    private void LoseGame()
    {
        Debug.Log("GAME OVER — YOU LOSE");


        if (TargetManager.Instance != null)
            TargetManager.Instance.ApplyLossPenalty();

        Time.timeScale = 0.4f;
    }
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            int minutes = 0;
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);

            if (timeRemaining <= 10f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining <= 30f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    private void StartTimer()
    {
        timerRunning = true;
        Debug.Log("GameManager: Timer started - " + levelTimeLimit + " seconds");
    }
}
