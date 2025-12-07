// using UnityEngine;

// public class BirdLaunch : MonoBehaviour
// {
//     private Rigidbody rb;
//     private bool isDragging = false;
//     public float launchPower = 500f;

//     Vector3 startPosition;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         startPosition = transform.position;
//         rb.isKinematic = true; // Bird stays still until shot
//     }

//     void OnMouseDown()
//     {
//         isDragging = true;
//     }

//     void OnMouseUp()
//     {
//         isDragging = false;
//         LaunchBird();
//     }

//     void Update()
//     {
//         if (isDragging)
//         {
//             DragBird();
//         }
//     }

//     void DragBird()
//     {
//         Vector3 mouseWorldPos = GetMouseWorldPosition();
//         transform.position = mouseWorldPos;
//     }

//     Vector3 GetMouseWorldPosition()
//     {
//         Plane plane = new Plane(Vector3.up, startPosition);
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//         float distance;
//         plane.Raycast(ray, out distance);

//         return ray.GetPoint(distance);
//     }

//     void LaunchBird()
//     {
//         rb.isKinematic = false;
//         Vector3 direction = startPosition - transform.position;
//         rb.AddForce(direction * launchPower);
//     }
// }



using UnityEngine;

public class BirdLaunch : MonoBehaviour
{
    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 startPosition;
    private BirdManager manager;
    private LineRenderer lineRenderer;
    public int linePoints = 30;          // amount of points in line
    public float timeBetweenPoints = 0.1f;  // time step between each point

    public LayerMask targetLayer; // Assign in Inspector
    private bool willHitTarget = false;

    [Header("Slingshot")]
    public SlingshotController slingshot; // Reference to slingshot controller

    [Header("Launch")]
    public float launchPower = 500f;
    public float maxDragDistance = 5f; // optional limit for how far player can pull

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        if (rb == null)
        {
            Debug.LogError("BirdLaunch requires a Rigidbody.");
        }
        rb.isKinematic = true;

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

        // place at spawn pos (manager already positioned it, but do it again to be safe)
        transform.position = startPosition;
    }

    void OnMouseDown()
    {
        // only allow dragging if this is the active bird (rb should be kinematic)
        if (rb == null || manager == null) return;
        isDragging = true;

        // Show trajectory line and slingshot bands
        if (lineRenderer != null)
            lineRenderer.enabled = true;

        if (slingshot != null)
        {
            slingshot.ShowBands(true);
            // Trigger Tension animation when bird is grabbed
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // Hide trajectory line
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

            // limit drag distance
            Vector3 offset = hit - startPosition;
            if (offset.magnitude > maxDragDistance)
                offset = offset.normalized * maxDragDistance;

            transform.position = startPosition + offset;

            // Update slingshot bands to follow bird position
            // Tension animation is triggered in UpdateBands method in SlingshotController
            if (slingshot != null)
            {
                slingshot.UpdateBands(transform.position);
            }
        }
    }

    private void Launch()
    {
        if (rb == null) return;

        rb.isKinematic = false;

        // direction is from current position back to start (like pulling rubber band)
        Vector3 direction = startPosition - transform.position;
        rb.AddForce(direction * launchPower);

        // Reset slingshot bands with animation
        // This triggers Release animation, which will transition to Idle automatically
        if (slingshot != null)
        {
            slingshot.AnimateReset(0.15f);
        }

        // notify manager so it can schedule destroy + next spawn
        if (manager != null)
        {
            manager.OnBirdLaunched(gameObject);
        }
    }
    // void DrawTrajectory()
    // {
    //     Vector3 direction = startPosition - transform.position; // same as launch direction
    //     Vector3 velocity = direction * launchPower / rb.mass;

    //     lineRenderer.positionCount = linePoints;
    //     for (int i = 0; i < linePoints; i++)
    //     {
    //         float time = i * timeBetweenPoints;
    //         Vector3 point = transform.position +
    //                         velocity * time +
    //                         Physics.gravity * time * time / 2f;
    //         lineRenderer.SetPosition(i, point);
    //     }
    // }
    void DrawTrajectory()
    {
        Vector3 direction = startPosition - transform.position;
        Vector3 velocity = direction * launchPower / rb.mass;

        lineRenderer.positionCount = linePoints;
        willHitTarget = false; // Reset flag

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = transform.position +
                            velocity * time +
                            Physics.gravity * time * time / 2f;

            lineRenderer.SetPosition(i, point);

            // ---- NEW: Check if hitting a target ----
            if (Physics.Raycast(point, Vector3.down, out RaycastHit hit, 0.5f, targetLayer))
            {
                willHitTarget = true;
            }
        }

        // Change color based on target detection
        Color targetColor = willHitTarget ? Color.red : Color.white;
        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
    }

}










// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(LineRenderer))]
// public class BirdLaunch : MonoBehaviour
// {
//     private Rigidbody rb;
//     private bool isDragging = false;
//     private Vector3 startPosition;
//     private BirdManager manager;
//     private LineRenderer lineRenderer;
//     public int linePoints = 30;              // amount of points in line
//     public float timeBetweenPoints = 0.1f;   // step between trajectory points

//     public LayerMask targetLayer; // Assign in Inspector
//     private bool willHitTarget = false;

//     [Header("Slingshot")]
//     public SlingshotController slingshot; // Reference to slingshot controller

//     [Header("Launch (velocity-based)")]
//     public float launchPower = 15f;      // Speed multiplier (tweak in inspector)
//     public float launchAngle = 45f;      // Fixed upward angle in degrees (parabolic)
//     public float maxDragDistance = 5f;   // clamp for how far player can pull
//     public float maxLaunchSpeed = 40f;   // safe cap to avoid "rocket" speeds

//     void Awake()
//     {
//         rb = GetComponent<Rigidbody>();
//         lineRenderer = GetComponent<LineRenderer>();

//         if (rb == null)
//         {
//             Debug.LogError("BirdLaunch requires a Rigidbody.");
//         }

//         // Start kinematic until launch
//         rb.isKinematic = true;
//         rb.linearVelocity = Vector3.zero;
//         rb.angularVelocity = Vector3.zero;

//         // Hide trajectory line initially
//         if (lineRenderer != null)
//         {
//             lineRenderer.enabled = false;
//         }
//     }

//     // Called by BirdManager when this bird becomes the active one
//     public void Setup(Vector3 spawnPos, BirdManager mgr)
//     {
//         manager = mgr;
//         startPosition = spawnPos;

//         // freeze physics until launched
//         if (rb != null)
//         {
//             rb.linearVelocity = Vector3.zero;
//             rb.angularVelocity = Vector3.zero;
//             rb.isKinematic = true;
//         }

//         transform.position = startPosition;
//     }

//     void OnMouseDown()
//     {
//         if (rb == null || manager == null) return;
//         isDragging = true;

//         if (lineRenderer != null)
//             lineRenderer.enabled = true;

//         if (slingshot != null)
//             slingshot.ShowBands(true);
//     }

//     void OnMouseUp()
//     {
//         if (!isDragging) return;
//         isDragging = false;

//         if (lineRenderer != null)
//             lineRenderer.enabled = false;

//         Launch();
//     }

//     void Update()
//     {
//         if (isDragging)
//         {
//             Drag();
//             DrawTrajectory();
//         }
//     }

//     private void Drag()
//     {
//         // horizontal plane at the spawn y so dragging happens in XZ plane
//         Plane plane = new Plane(Vector3.up, startPosition);
//         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//         float enter;
//         if (plane.Raycast(ray, out enter))
//         {
//             Vector3 hit = ray.GetPoint(enter);

//             // offset = mouse hit - start (pull vector)
//             Vector3 offset = hit - startPosition;

//             // limit drag distance
//             if (offset.magnitude > maxDragDistance)
//                 offset = offset.normalized * maxDragDistance;

//             transform.position = startPosition + offset;

//             if (slingshot != null)
//                 slingshot.UpdateBands(transform.position);
//         }
//     }

//     private void Launch()
//     {
//         if (rb == null) return;

//         // Convert drag into a velocity (parabolic, fixed angle)
//         Vector3 launchVelocity = CalculateLaunchVelocity();

//         rb.isKinematic = false;

//         // Directly set velocity (velocity-based launch -> predictable parabola)
//         rb.linearVelocity = launchVelocity;

//         if (slingshot != null)
//             slingshot.AnimateReset(0.15f);

//         if (manager != null)
//             manager.OnBirdLaunched(gameObject);
//     }

//     /// <summary>
//     /// Calculate launch velocity using a fixed launchAngle (parabolic arc).
//     /// Speed is derived from drag distance and launchPower, clamped by maxLaunchSpeed.
//     /// Note: horizontal direction is flattened so angle is applied relative to horizontal plane.
//     /// </summary>
//     private Vector3 CalculateLaunchVelocity()
//     {
//         // Pull vector from bird to slingshot (world space)
//         Vector3 pull = startPosition - transform.position;

//         // Raw direction from pulling
//         Vector3 direction = pull.normalized;

//         // Speed proportional to pull distance
//         float speed = pull.magnitude * launchPower;

//         // Clamp so it never becomes rocket-fast
//         speed = Mathf.Clamp(speed, 0f, maxLaunchSpeed);

//         // Final velocity
//         return direction * speed;
//     }

//     void DrawTrajectory()
//     {
//         Vector3 velocity = CalculateLaunchVelocity();

//         lineRenderer.positionCount = linePoints;
//         willHitTarget = false;

//         for (int i = 0; i < linePoints; i++)
//         {
//             float time = i * timeBetweenPoints;

//             Vector3 point =
//                 transform.position +
//                 velocity * time +
//                 0.5f * Physics.gravity * time * time;

//             lineRenderer.SetPosition(i, point);

//             if (Physics.Raycast(point, Vector3.down, out _, 0.5f, targetLayer))
//                 willHitTarget = true;
//         }

//         Color targetColor = willHitTarget ? Color.red : Color.white;
//         lineRenderer.startColor = targetColor;
//         lineRenderer.endColor = targetColor;
//     }

// }
