using UnityEngine;

public class FixedSlingshotCamera : MonoBehaviour
{
    [Header("Original Camera Position")]
    public Vector3 originalPosition;
    public Vector3 originalLookAtPoint;

    [Header("Follow Settings")]
    public Transform[] birdsToFollow;

    private int currentBirdIndex = 0;
    private Transform currentBird;

    public Vector3 followOffset = new Vector3(0f, 5f, -8f);

    [Range(0.1f, 20f)]
    public float followSmoothTime = 0.3f;

    [Range(1f, 20f)]
    public float lookAtSmoothness = 8f;

    [Range(0.1f, 20f)]
    public float returnSmoothTime = 1.5f;

    public float returnDelay = 0.5f;

    [Header("Camera Settings")]
    public float fieldOfView = 70f;
    public float minVelocityThreshold = 0.5f;

    private enum CameraState { Fixed, Following, Returning }
    private CameraState currentState = CameraState.Fixed;

    private Rigidbody birdRigidbody;
    private bool birdLaunched = false;

    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;

    private float returnTimer = 0f;
    private bool waitingToReturn = false;

    void Start()
    {
        if (originalPosition == Vector3.zero)
            originalPosition = transform.position;

        if (originalLookAtPoint == Vector3.zero)
            originalLookAtPoint = transform.position + transform.forward * 15f;

        transform.position = originalPosition;
        transform.LookAt(originalLookAtPoint);

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }

        if (birdsToFollow != null && birdsToFollow.Length > 0)
        {
            currentBirdIndex = 0;
            currentBird = birdsToFollow[0];
            if (currentBird != null)
            {
                birdRigidbody = currentBird.GetComponent<Rigidbody>();
                Debug.Log($"FixedSlingshotCamera: Initialized with first bird: {currentBird.name}");
            }
        }
        else
        {
            Debug.LogWarning("FixedSlingshotCamera: No birds assigned in birdsToFollow array!");
        }
    }

    void LateUpdate()
    {
        switch (currentState)
        {
            case CameraState.Fixed:
                transform.position = originalPosition;
                transform.LookAt(originalLookAtPoint);
                CheckForBirdLaunch();
                break;

            case CameraState.Following:
                FollowBird();
                CheckIfBirdStopped();
                break;

            case CameraState.Returning:
                ReturnToOriginal();
                break;
        }
    }

    void CheckForBirdLaunch()
    {
        if (currentBird == null)
        {
            Debug.LogWarning("FixedSlingshotCamera: currentBird is null, cannot check for launch");
            return;
        }

        if (birdRigidbody == null)
        {
            birdRigidbody = currentBird.GetComponent<Rigidbody>();
        }

        if (birdRigidbody != null && !birdRigidbody.isKinematic)
        {
            if (birdRigidbody.linearVelocity.magnitude > minVelocityThreshold)
            {
                currentState = CameraState.Following;
                birdLaunched = true;
                Debug.Log($"FixedSlingshotCamera: Now following bird {currentBirdIndex}: {currentBird.name}");
            }
        }
    }

    void FollowBird()
    {
        if (currentBird == null) return;

        Vector3 targetPosition = currentBird.position + followOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            followSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        Vector3 directionToBird = currentBird.position - transform.position;

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
        if (currentBird == null || birdRigidbody == null) return;

        bool hasStopped = birdRigidbody.linearVelocity.magnitude < minVelocityThreshold;
        bool isOnGround = currentBird.position.y < 2f;

        if ((hasStopped || isOnGround) && !waitingToReturn)
        {
            waitingToReturn = true;
            returnTimer = 0f;
        }

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
        transform.position = Vector3.SmoothDamp(
            transform.position,
            originalPosition,
            ref positionVelocity,
            returnSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

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

        float distanceToOriginal = Vector3.Distance(transform.position, originalPosition);
        if (distanceToOriginal < 0.05f && positionVelocity.magnitude < 0.01f)
        {
            transform.position = originalPosition;
            transform.LookAt(originalLookAtPoint);

            positionVelocity = Vector3.zero;
            rotationVelocity = Vector3.zero;

            currentState = CameraState.Fixed;
            birdLaunched = false;

            Debug.Log("FixedSlingshotCamera: Returned to original position, ready for next bird");
        }
    }

    void AdvanceToNextBird()
    {
        if (birdsToFollow == null || birdsToFollow.Length == 0)
        {
            Debug.Log("FixedSlingshotCamera: No bird array, staying on current bird");
            return;
        }

        currentBirdIndex++;

        if (currentBirdIndex < birdsToFollow.Length)
        {
            currentBird = birdsToFollow[currentBirdIndex];
            birdRigidbody = currentBird != null ? currentBird.GetComponent<Rigidbody>() : null;
            Debug.Log($"FixedSlingshotCamera: Advanced to bird {currentBirdIndex}: {(currentBird != null ? currentBird.name : "null")}");
        }
        else
        {
            currentBird = null;
            birdRigidbody = null;
            Debug.Log("FixedSlingshotCamera: No more birds in sequence");
        }
    }

    public void SetBirdToFollow(Transform bird)
    {
        if (bird == null)
        {
            Debug.LogWarning("FixedSlingshotCamera: Attempting to set null bird");
            return;
        }

        currentBird = bird;
        birdRigidbody = bird.GetComponent<Rigidbody>();

        currentState = CameraState.Fixed;
        birdLaunched = false;

        positionVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
        waitingToReturn = false;
        returnTimer = 0f;

        Debug.Log($"FixedSlingshotCamera: Set current bird to: {currentBird.name}");
    }

    public void InitializeBirds(Transform[] birds)
    {
        birdsToFollow = birds;
        currentBirdIndex = 0;

        if (birds != null && birds.Length > 0)
        {
            currentBird = birds[0];
            birdRigidbody = currentBird != null ? currentBird.GetComponent<Rigidbody>() : null;
        }

        currentState = CameraState.Fixed;
        birdLaunched = false;
        positionVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
        waitingToReturn = false;
        returnTimer = 0f;
    }

    public void ReturnToOriginalPosition()
    {
        currentState = CameraState.Returning;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(originalPosition, 0.5f);
        Gizmos.DrawLine(originalPosition, originalLookAtPoint);
        Gizmos.DrawWireSphere(originalLookAtPoint, 0.3f);

        if (currentState == CameraState.Following && currentBird != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentBird.position);
            Gizmos.DrawWireSphere(currentBird.position, 0.5f);
        }
    }
}
