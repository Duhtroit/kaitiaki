using Unity.VisualScripting;
using UnityEngine;

public class cameraController : MonoBehaviour
{

    [Header("Camera Settings")]
    [SerializeField]
    private Vector3 subject;
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


    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetPosition;

    Vector3 posOffset;

    private bool zoomed = false;

    public void onPlayerPositionChange(Vector3 pos)
    {
        subject = pos;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        posOffset = zoomed ? positionOffsetZoomed: positionOffset;

        targetPosition = subject + posOffset;
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, targetPosition, ref currentVelocity, smoothing);

    }

    public void setZoomed(bool value)
    {
        zoomed = value;
    }

}
