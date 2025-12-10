

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class BirdLaunch : MonoBehaviour
{
    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 startPosition;
    private BirdManager manager;
    private LineRenderer lineRenderer;
    public int linePoints = 30;              // amount of points in line

    [Header("Dynamic Angle")]
    public float minAngle = 8f;
    public float maxAngle = 45f;

    public float timeBetweenPoints = 0.1f;

    public LayerMask targetLayer;
    private bool willHitTarget = false;

    [Header("Slingshot")]
    public SlingshotController slingshot;

    [Header("Launch (velocity-based)")]
    public float launchPower = 45f;
    public float launchAngle = 15f;
    public float maxDragDistance = 5f;
    public float maxLaunchSpeed = 40f;

    [Header("Sound Effects")]
    public AudioClip launchSound;   // 🔊 Play once on release
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        if (rb == null)
        {
            Debug.LogError("BirdLaunch requires a Rigidbody.");
        }

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // 🔊 AudioSource setup
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = 1f;
    }

    public void Setup(Vector3 spawnPos, BirdManager mgr)
    {
        manager = mgr;
        startPosition = spawnPos;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.position = startPosition;
    }

    void OnMouseDown()
    {
        if (rb == null || manager == null) return;
        isDragging = true;

        if (lineRenderer != null)
            lineRenderer.enabled = true;

        if (slingshot != null)
            slingshot.ShowBands(true);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        Launch();
    }

    void Update()
    {
        if (isDragging)
        {
            Drag();
            DrawTrajectory();
        }
    }

    private void Drag()
    {
        Plane plane = new Plane(Vector3.up, startPosition);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 hit = ray.GetPoint(enter);

            Vector3 offset = hit - startPosition;

            if (offset.magnitude > maxDragDistance)
                offset = offset.normalized * maxDragDistance;

            transform.position = startPosition + offset;

            if (slingshot != null)
                slingshot.UpdateBands(transform.position);
        }
    }

    private void Launch()
    {
        if (rb == null) return;

        // 🔊 PLAY LAUNCH SOUND ON RELEASE
        if (launchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(launchSound);
            Debug.Log("Launch sound played!");
        }

        // Convert drag into velocity
        Vector3 launchVelocity = CalculateLaunchVelocity();

        rb.isKinematic = false;
        rb.linearVelocity = launchVelocity;

        if (slingshot != null)
            slingshot.AnimateReset(0.15f);

        if (manager != null)
            manager.OnBirdLaunched(gameObject);

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 pull = startPosition - transform.position;
        float pullDist = pull.magnitude;

        float speed = Mathf.Clamp(pullDist * launchPower, 0f, maxLaunchSpeed);

        float t = Mathf.Clamp01(pullDist / maxDragDistance);
        float dynamicAngle = Mathf.Lerp(minAngle, maxAngle, t);
        float rad = dynamicAngle * Mathf.Deg2Rad;

        Vector3 flatDir = new Vector3(pull.x, 0f, pull.z).normalized;

        Vector3 velocity =
            flatDir * (speed * Mathf.Cos(rad)) +
            Vector3.up * (speed * Mathf.Sin(rad));

        return velocity;
    }

    void DrawTrajectory()
    {
        Vector3 velocity = CalculateLaunchVelocity();

        lineRenderer.positionCount = linePoints;
        willHitTarget = false;

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;

            Vector3 point =
                transform.position +
                velocity * time +
                0.5f * Physics.gravity * time * time;

            lineRenderer.SetPosition(i, point);

            if (Physics.Raycast(point, Vector3.down, out _, 0.5f, targetLayer))
                willHitTarget = true;
        }

        Color targetColor = willHitTarget ? Color.red : Color.white;
        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
    }
}
