// using System.Collections;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class TargetManager : MonoBehaviour
// {
//     public static TargetManager Instance;

//     private int totalTargets;
//     private int destroyedTargets = 0;
//     private bool levelComplete = false;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         // Delay one frame so Unity can finish instantiating everything
//         StartCoroutine(CountTargetsNextFrame());
//     }

//     private IEnumerator CountTargetsNextFrame()
//     {
//         yield return null; // wait 1 frame

//         totalTargets = FindObjectsOfType<Target>().Length;
//         Debug.Log("Targets found: " + totalTargets);
//     }

//     public void TargetDestroyed()
//     {
//         if (levelComplete) return;

//         destroyedTargets++;

//         if (destroyedTargets >= totalTargets && totalTargets > 0)
//         {
//             levelComplete = true;
//             Debug.Log("All targets destroyed! Loading next level...");
//             SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
//         }
//     }
// }


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
//         yield return null;  // wait one frame so Unity finishes spawning objects

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
//         // Show win text
//         if (winLoseText != null)
//         {
//             winLoseText.text = "YOU WIN!";
//             winLoseText.gameObject.SetActive(true);
//         }

//         yield return new WaitForSeconds(delayBeforeNextLevel);

//         // Load next level
//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
//     }

//     // BirdManager uses this to check loss condition
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

    private int totalTargets;
    private int destroyedTargets = 0;
    private bool levelComplete = false;

    [Header("UI")]
    public TextMeshProUGUI winLoseText;

    [Header("Level Transition")]
    public float delayBeforeNextLevel = 2f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(CountTargetsNextFrame());
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

        if (destroyedTargets >= totalTargets && totalTargets > 0)
        {
            levelComplete = true;
            StartCoroutine(WinSequence());
        }
    }

    private IEnumerator WinSequence()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int lastLevelIndex = SceneManager.sceneCountInBuildSettings - 1;

        // If this is the LAST LEVEL
        if (currentIndex == lastLevelIndex)
        {
            if (winLoseText != null)
            {
                winLoseText.text = "ALL LEVELS COMPLETE!\nYOU WIN!";
                winLoseText.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(delayBeforeNextLevel);
            
            Debug.Log("All levels completed! Game finished!");
            yield break;  // STOP HERE
        }

        // NORMAL WIN → Load next scene
        if (winLoseText != null)
        {
            winLoseText.text = "LEVEL COMPLETE!";
            winLoseText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeNextLevel);

        SceneManager.LoadScene(currentIndex + 1);
    }

    public bool AllTargetsDestroyed()
    {
        return destroyedTargets >= totalTargets && totalTargets > 0;
    }
}
