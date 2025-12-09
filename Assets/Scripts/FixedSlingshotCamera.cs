using UnityEngine;

/// <summary>
/// Camera that stays fixed during aiming, follows bird cinematically after launch,
/// and returns to original position when bird stops.
/// </summary>
public class FixedSlingshotCamera : MonoBehaviour
{
    [Header("Original Camera Position")]
    [Tooltip("Camera's fixed position during aiming")]
    public Vector3 originalPosition;

    [Tooltip("What the camera looks at during aiming")]
    public Vector3 originalLookAtPoint;

    [Header("Follow Settings")]
    [Tooltip("The bird to follow after launch")]
    public Transform birdToFollow;

    [Tooltip("Camera offset from bird during follow (relative position)")]
    public Vector3 followOffset = new Vector3(0f, 5f, -8f);

    [Tooltip("How smoothly camera follows bird (higher = smoother, slower)")]
    [Range(0.1f, 20f)]
    public float followSmoothTime = 0.3f;

    [Tooltip("How smoothly camera rotates to look at bird")]
    [Range(1f, 20f)]
    public float lookAtSmoothness = 8f;

    [Tooltip("How smoothly camera returns to original position")]
    [Range(0.1f, 20f)]
    public float returnSmoothTime = 1.5f;

    [Tooltip("Delay before camera starts returning (seconds)")]
    public float returnDelay = 0.5f;

    [Header("Camera Settings")]
    [Tooltip("Field of view")]
    public float fieldOfView = 70f;

    [Tooltip("Minimum bird velocity to consider it moving")]
    public float minVelocityThreshold = 0.5f;

    // States
    private enum CameraState { Fixed, Following, Returning }
    private CameraState currentState = CameraState.Fixed;

    private Rigidbody birdRigidbody;
    private bool birdLaunched = false;

    // Smooth damping velocities
    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;

    // Return delay timer
    private float returnTimer = 0f;
    private bool waitingToReturn = false;

    void Start()
    {
        // Store current position as original if not set
        if (originalPosition == Vector3.zero)
            originalPosition = transform.position;

        if (originalLookAtPoint == Vector3.zero)
            originalLookAtPoint = transform.position + transform.forward * 15f;

        // Set camera position and rotation
        transform.position = originalPosition;
        transform.LookAt(originalLookAtPoint);

        // Set field of view
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }
    }

    void LateUpdate()
    {
        switch (currentState)
        {
            case CameraState.Fixed:
                // Stay at original position, looking at aim point
                transform.position = originalPosition;
                transform.LookAt(originalLookAtPoint);

                // Check if bird has been launched
                CheckForBirdLaunch();
                break;

            case CameraState.Following:
                // Follow the bird cinematically
                FollowBird();

                // Check if bird has stopped moving
                CheckIfBirdStopped();
                break;

            case CameraState.Returning:
                // Return to original position
                ReturnToOriginal();
                break;
        }
    }

    void CheckForBirdLaunch()
    {
        if (birdToFollow == null) return;

        // Get rigidbody if we don't have it
        if (birdRigidbody == null)
        {
            birdRigidbody = birdToFollow.GetComponent<Rigidbody>();
        }

        // Check if bird is moving (launched)
        if (birdRigidbody != null && !birdRigidbody.isKinematic)
        {
            if (birdRigidbody.linearVelocity.magnitude > minVelocityThreshold)
            {
                currentState = CameraState.Following;
                birdLaunched = true;
            }
        }
    }

    void FollowBird()
    {
        if (birdToFollow == null) return;

        // Calculate desired camera position relative to bird's current position
        // This creates a smooth follow that maintains offset
        Vector3 targetPosition = birdToFollow.position + followOffset;

        // Use SmoothDamp in LateUpdate for buttery smooth following
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            followSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        // Calculate smooth look-at rotation
        Vector3 directionToBird = birdToFollow.position - transform.position;

        if (directionToBird.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToBird);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                lookAtSmoothness * Time.deltaTime
            );
        }
    }

    void CheckIfBirdStopped()
    {
        if (birdToFollow == null || birdRigidbody == null) return;

        // Check if bird has stopped moving or hit the ground
        bool hasStopped = birdRigidbody.linearVelocity.magnitude < minVelocityThreshold;

        // Also check if bird is very close to ground (y position low)
        bool isOnGround = birdToFollow.position.y < 2f;

        if ((hasStopped || isOnGround) && !waitingToReturn)
        {
            // Start waiting before returning
            waitingToReturn = true;
            returnTimer = 0f;
        }

        // Count down the return delay
        if (waitingToReturn)
        {
            returnTimer += Time.deltaTime;
            if (returnTimer >= returnDelay)
            {
                currentState = CameraState.Returning;
                waitingToReturn = false;
                returnTimer = 0f;
            }
        }
    }

    void ReturnToOriginal()
    {
        // Use SmoothDamp for smooth, natural return movement with explicit deltaTime
        transform.position = Vector3.SmoothDamp(
            transform.position,
            originalPosition,
            ref positionVelocity,
            returnSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        // Smoothly rotate back to look at original point
        Vector3 directionToTarget = originalLookAtPoint - transform.position;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                5f * Time.deltaTime
            );
        }

        // Check if we're close enough to snap back to fixed state
        float distanceToOriginal = Vector3.Distance(transform.position, originalPosition);
        if (distanceToOriginal < 0.05f && positionVelocity.magnitude < 0.01f)
        {
            // Snap to exact position and rotation
            transform.position = originalPosition;
            transform.LookAt(originalLookAtPoint);

            // Reset velocities
            positionVelocity = Vector3.zero;
            rotationVelocity = Vector3.zero;

            // Return to fixed state
            currentState = CameraState.Fixed;
            birdLaunched = false;
        }
    }

    /// <summary>
    /// Call this method to set the bird that camera should follow
    /// Typically called by BirdManager when a new bird is spawned
    /// </summary>
    public void SetBirdToFollow(Transform bird)
    {
        birdToFollow = bird;
        birdRigidbody = bird != null ? bird.GetComponent<Rigidbody>() : null;
        currentState = CameraState.Fixed;
        birdLaunched = false;

        // Reset velocities and timers
        positionVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
        waitingToReturn = false;
        returnTimer = 0f;
    }

    /// <summary>
    /// Manually trigger camera to return to original position
    /// </summary>
    public void ReturnToOriginalPosition()
    {
        currentState = CameraState.Returning;
    }

    // Helper: Visualize camera states in editor
    void OnDrawGizmos()
    {
        // Draw original position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(originalPosition, 0.5f);
        Gizmos.DrawLine(originalPosition, originalLookAtPoint);
        Gizmos.DrawWireSphere(originalLookAtPoint, 0.3f);

        // Draw current target if following
        if (currentState == CameraState.Following && birdToFollow != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, birdToFollow.position);
            Gizmos.DrawWireSphere(birdToFollow.position, 0.5f);
        }
    }
}