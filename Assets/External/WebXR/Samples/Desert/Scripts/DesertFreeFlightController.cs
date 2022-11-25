using System.Threading;
using Cysharp.Threading.Tasks;
using RPGSystem;
using UnityEngine;
using WebXR;

[RequireComponent(typeof(Camera))]
public class DesertFreeFlightController : MonoBehaviour
{
    #region RotationParameters
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
    #endregion

    public LayerMask clickRayLayerMask;
    public float dragVsClickTime = 0.1f;
    public float timeForClickBehavior = 0.6f;
    float lastTimeClick = -9f;
    Vector3 lastPositionClick;

    public IExpositionable activeExposition;
    CancellationTokenSource sourceClickToMove = new();
    public static DesertFreeFlightController Instance;
    public Expositor expositor;

    void Start()
    {
        Instance = this;
        originalRotation = transform.localRotation;
        attachedCamera = GetComponent<Camera>();
        activeExposition = expositor;
    }

    public bool IsBusy()
    {
        return expositor.IsBusy() || activeExposition != null && activeExposition.IsBusy();
    }

    void Update()
    {
        attachedCamera.focalLength = Mathf.Clamp(attachedCamera.focalLength + Input.mouseScrollDelta.y * 2, 12, 48);

        if (IsBusy()) return;
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
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (timeForClickBehavior + lastTimeClick < Time.time) return;
            if (lastPositionClick == Input.mousePosition)
            {
                sourceClickToMove.Cancel();
                ClickInteraction();
                return;
            }
            if (lastTimeClick + dragVsClickTime < Time.time) return;
            sourceClickToMove.Cancel();
            ClickInteraction();
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

    void ClickInteraction()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, Mathf.Infinity, clickRayLayerMask);

        if (hits.Length > 0 && hits[0].transform != null)
        {
            if (hits[0].transform.TryGetComponent<IExpositionable>(out var expo))
            {
                activeExposition = expo;
                expo.Preview();
            }

            if (hits[0].transform.TryGetComponent<RPGEvent>(out var rpgEvent))
            {
                rpgEvent.TriggerEvent();
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
