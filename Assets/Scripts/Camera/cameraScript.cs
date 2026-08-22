using Unity.VisualScripting;
using UnityEngine;

public class cameraController : MonoBehaviour
{

    [Header("Camera Settings")]
    [SerializeField]
    private GameObject subject;
    [SerializeField]
    private Camera camera;
    [SerializeField]
    private Vector3 positionOffset = new Vector3(0, 8, -7);
    [SerializeField]
    private Vector3 rotationOffset = new Vector3(38, 0, 0);
    [SerializeField]
    private float smoothing = 0.1f;

    [Header("Camera Zoom Settings")]
    [SerializeField]
    private Vector3 positionOffsetZoomed = new Vector3(0, 4, -4);
    [SerializeField]
    private Vector3 rotationOffsetZoomed = new Vector3(30, 0, 0);

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        targetPosition = subject.transform.position + positionOffset;
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, targetPosition, ref currentVelocity, smoothing);

    }
}
