using System.Threading;
using Cysharp.Threading.Tasks;
using RPGSystem;
using UnityEngine;
using WebXR;

[RequireComponent(typeof(Camera))]
public class DesertFreeFlightController : MonoBehaviour
{
    [Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
    public bool rotationEnabled = true;

    [Tooltip("Enable/disable translation control. For use in Unity editor only.")]
    public bool translationEnabled = true;

    private WebXRDisplayCapabilities capabilities;

    [Tooltip("Mouse sensitivity")]
    public float mouseSensitivity = 1f;

    [Tooltip("Straffe Speed")]
    public float straffeSpeed = 5f;

    private float minimumX = -360f;
    private float maximumX = 360f;

    private float minimumY = -90f;
    private float maximumY = 90f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    Quaternion originalRotation;

    private Camera attachedCamera;
    private Vector3 axis;
    private Vector3 axisLastFrame;
    private Vector3 axisDelta;

    RaycastHit[] hits = new RaycastHit[1];
    bool isMoving = false;
    float playerHeight = 1.8f;
    public float movementSpeed = 0.2f;
    public LayerMask clickRayLayerMask;
    public float walkToPointDelay = 1f;

    public float dragVsClickTime = 0.1f;
    float lastTimeClick = -999f;
    Vector3 lastPositionClick;
    CancellationTokenSource sourceClickToMove = new();
    IExpositionable activeExposition;

    void Start()
    {
        originalRotation = transform.localRotation;
        attachedCamera = GetComponent<Camera>();
    }

    void Update()
    {
        attachedCamera.focalLength = Mathf.Clamp(attachedCamera.focalLength + Input.mouseScrollDelta.y * 2, 12, 48);

        if (rotationEnabled && activeExposition == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                axisLastFrame = attachedCamera.ScreenToViewportPoint(Input.mousePosition);
                lastTimeClick = Time.time;
                lastPositionClick = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                axis = attachedCamera.ScreenToViewportPoint(Input.mousePosition);
                axisDelta = (axisLastFrame - axis) * 90f;
                axisLastFrame = axis;

                rotationX += axisDelta.x * mouseSensitivity;
                rotationY += axisDelta.y * mouseSensitivity;

                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

                transform.localRotation = originalRotation * xQuaternion * yQuaternion;

                if (isMoving) return;
                if (lastPositionClick == Input.mousePosition)
                {
                    sourceClickToMove.Cancel();
                    ClickToMove();
                    return;
                }
                if (lastTimeClick + dragVsClickTime < Time.time) return;
                sourceClickToMove.Cancel();
                ClickToMove();
            }
        }
    }

    public void Back()
    {
        if (activeExposition != null)
        {
            activeExposition.EscapePreview();
            activeExposition = null;
        }
    }

    void ClickToMove()
    {
        if (activeExposition != null) return;
        var lastHit = hits[0];
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.RaycastNonAlloc(ray, hits, Mathf.Infinity, clickRayLayerMask);

        if (lastHit.GetHashCode() == hits[0].GetHashCode()) return;

        if (hits[0].transform != null)
        {
            if (hits[0].transform.TryGetComponent<IExpositionable>(out var expo))
            {
                activeExposition = expo;
                expo.Preview();
            }

            if (hits[0].transform.TryGetComponent<RPGEvent>(out var rpgEvent))
            {
                rpgEvent.TriggerPageActionList();
            }
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
