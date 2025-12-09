

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
    public float minAngle = 8f;     // angle when barely pulled
    public float maxAngle = 45f;    // angle at full pull distance

    public float timeBetweenPoints = 0.1f;   // step between trajectory points

    public LayerMask targetLayer; // Assign in Inspector
    private bool willHitTarget = false;

    [Header("Slingshot")]
    public SlingshotController slingshot; // Reference to slingshot controller

    [Header("Launch (velocity-based)")]
    public float launchPower = 45f;      // Speed multiplier (tweak in inspector)
    public float launchAngle = 15f;      // Fixed upward angle in degrees (parabolic)
    public float maxDragDistance = 5f;   // clamp for how far player can pull
    public float maxLaunchSpeed = 40f;   // safe cap to avoid "rocket" speeds

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        if (rb == null)
        {
            Debug.LogError("BirdLaunch requires a Rigidbody.");
        }

        // Start kinematic until launch
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Hide trajectory line initially
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    // Called by BirdManager when this bird becomes the active one
    public void Setup(Vector3 spawnPos, BirdManager mgr)
    {
        manager = mgr;
        startPosition = spawnPos;

        // freeze physics until launched
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
        // horizontal plane at the spawn y so dragging happens in XZ plane
        Plane plane = new Plane(Vector3.up, startPosition);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 hit = ray.GetPoint(enter);

            // offset = mouse hit - start (pull vector)
            Vector3 offset = hit - startPosition;

            // limit drag distance
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

        // Convert drag into a velocity (parabolic, fixed angle)
        Vector3 launchVelocity = CalculateLaunchVelocity();

        rb.isKinematic = false;

        // Directly set velocity (velocity-based launch -> predictable parabola)
        rb.linearVelocity = launchVelocity;

        if (slingshot != null)
            slingshot.AnimateReset(0.15f);

        if (manager != null)
            manager.OnBirdLaunched(gameObject);
    }

    /// <summary>
    /// Calculate launch velocity using a fixed launchAngle (parabolic arc).
    /// Speed is proportional to pull distance, clamped by maxLaunchSpeed.
    /// </summary>
    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 pull = startPosition - transform.position;
        float pullDist = pull.magnitude;

        float speed = Mathf.Clamp(pullDist * launchPower, 0f, maxLaunchSpeed);

        // Normalize angle based on pull distance
        float t = Mathf.Clamp01(pullDist / maxDragDistance);
        float dynamicAngle = Mathf.Lerp(minAngle, maxAngle, t);
        float rad = dynamicAngle * Mathf.Deg2Rad;

        // Flatten direction on XZ
        Vector3 flatDir = new Vector3(pull.x, 0f, pull.z).normalized;

        // Build velocity vector
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
