using UnityEngine;

public class BirdLaunch : MonoBehaviour
{
    private Rigidbody rb;
    private bool isDragging = false;
    public float launchPower = 500f;

    Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        rb.isKinematic = true; // Bird stays still until shot
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
        LaunchBird();
    }

    void Update()
    {
        if (isDragging)
        {
            DragBird();
        }
    }

    void DragBird()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        transform.position = mouseWorldPos;
    }

    Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, startPosition);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float distance;
        plane.Raycast(ray, out distance);

        return ray.GetPoint(distance);
    }

    void LaunchBird()
    {
        rb.isKinematic = false;
        Vector3 direction = startPosition - transform.position;
        rb.AddForce(direction * launchPower);
    }
}
