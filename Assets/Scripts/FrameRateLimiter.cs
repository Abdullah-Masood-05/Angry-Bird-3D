using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
    [Header("Frame Rate Settings")]
    public int targetFrameRate = 60;

    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }
}