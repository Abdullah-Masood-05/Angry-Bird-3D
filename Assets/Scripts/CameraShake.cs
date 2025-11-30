using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.2f;
    private Vector3 initialPosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = initialPosition;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
