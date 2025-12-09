
// using System.Collections;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using TMPro;

// public class TargetManager : MonoBehaviour
// {
//     public static TargetManager Instance;

//     private int totalTargets;
//     private int destroyedTargets = 0;
//     private bool levelComplete = false;

//     [Header("UI")]
//     public TextMeshProUGUI winLoseText;

//     [Header("Level Transition")]
//     public float delayBeforeNextLevel = 2f;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         StartCoroutine(CountTargetsNextFrame());
//     }

//     private IEnumerator CountTargetsNextFrame()
//     {
//         yield return null;

//         Target[] targets = FindObjectsOfType<Target>();
//         totalTargets = targets.Length;

//         Debug.Log("TargetManager: Found " + totalTargets + " targets.");
//     }

//     public void TargetDestroyed()
//     {
//         if (levelComplete) return;

//         destroyedTargets++;

//         if (destroyedTargets >= totalTargets && totalTargets > 0)
//         {
//             levelComplete = true;
//             StartCoroutine(WinSequence());
//         }
//     }

//     private IEnumerator WinSequence()
//     {
//         int currentIndex = SceneManager.GetActiveScene().buildIndex;
//         int lastLevelIndex = SceneManager.sceneCountInBuildSettings - 1;

//         // If this is the LAST LEVEL
//         if (currentIndex == lastLevelIndex)
//         {
//             if (winLoseText != null)
//             {
//                 winLoseText.text = "ALL LEVELS COMPLETE!\nYOU WIN!";
//                 winLoseText.gameObject.SetActive(true);
//             }
//             yield return new WaitForSeconds(delayBeforeNextLevel);

//             Debug.Log("All levels completed! Game finished!");
//             yield break;  // STOP HERE
//         }

//         // NORMAL WIN → Load next scene
//         if (winLoseText != null)
//         {
//             winLoseText.text = "LEVEL COMPLETE!";
//             winLoseText.gameObject.SetActive(true);
//         }

//         yield return new WaitForSeconds(delayBeforeNextLevel);

//         SceneManager.LoadScene(currentIndex + 1);
//     }

//     public bool AllTargetsDestroyed()
//     {
//         return destroyedTargets >= totalTargets && totalTargets > 0;
//     }
// }



using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    // ⬇ SCORE VARIABLES
    public static int totalScore = 0;        // persists across scenes
    public static int lastLevelScore = 0;    // tracks per-level score

    private int totalTargets;
    private int destroyedTargets = 0;
    private bool levelComplete = false;

    [Header("UI")]
    public TextMeshProUGUI winLoseText;      // assign TMP text
    public TextMeshProUGUI scoreText;        // NEW: assign TMP score text

    [Header("Level Transition")]
    public float delayBeforeNextLevel = 2f;

    void Awake()
    {
        Instance = this;
        lastLevelScore = 0; // reset level score
    }

    void Start()
    {
        StartCoroutine(CountTargetsNextFrame());
        UpdateScoreUI();
    }

    private IEnumerator CountTargetsNextFrame()
    {
        yield return null;

        Target[] targets = FindObjectsOfType<Target>();
        totalTargets = targets.Length;

        Debug.Log("TargetManager: Found " + totalTargets + " targets.");
    }

    public void TargetDestroyed()
    {
        if (levelComplete) return;

        destroyedTargets++;

        // ADD SCORE
        totalScore += 100;
        lastLevelScore += 100;

        UpdateScoreUI();

        if (destroyedTargets >= totalTargets && totalTargets > 0)
        {
            levelComplete = true;
            StartCoroutine(WinSequence());
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + totalScore;
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
                "LEVEL COMPLETE!\n+" + lastLevelScore + " points";

            winLoseText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeNextLevel);
        SceneManager.LoadScene(currentIndex + 1);
    }

    public void ApplyLossPenalty()
    {
        totalScore -= 200;
        if (totalScore < 0) totalScore = 0; // prevent negative

        UpdateScoreUI();

        if (winLoseText != null)
        {
            winLoseText.text =
                "YOU LOSE!\n-200 POINTS\nTOTAL SCORE: " + totalScore;

            winLoseText.gameObject.SetActive(true);
        }
    }

    public bool AllTargetsDestroyed()
    {
        return destroyedTargets >= totalTargets && totalTargets > 0;
    }
}
