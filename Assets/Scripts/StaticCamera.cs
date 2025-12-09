using UnityEngine;

public class StaticCamera : MonoBehaviour
{
    [Header("Fixed Camera Transform")]
    public Vector3 position = new Vector3(0f, 8f, -12f);
    public Vector3 rotation = new Vector3(30f, 0f, 0f);
    public float fieldOfView = 60f;

    void Start()
    {
        transform.position = position;
        transform.eulerAngles = rotation;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }

        // Disable this script after setup - camera never updates
        enabled = false;
    }
}