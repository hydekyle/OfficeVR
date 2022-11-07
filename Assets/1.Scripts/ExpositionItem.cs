using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExpositionItem : MonoBehaviour, IExpositionable
{
    public Transform previewT;
    Vector3 originPosition;
    Quaternion originRotation;
    bool isAnimating = false;
    bool isPreviewModeActive = false;

    #region ItemRotationController
    [Tooltip("Rotation Sensitivity")]
    public float mouseSensitivity = 1f;

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

    // This is called when this component is added to a GameObject to save time for devs
    void Reset()
    {
        if (!TryGetComponent<Collider>(out var col))
        {
            var newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.isTrigger = true;
            this.previewT = GameObject.Find("[Preview]").transform;
        }
    }

    void Start()
    {
        attachedCamera = Camera.main;
        originalRotation = previewT.localRotation;
    }

    void Update()
    {
        RotatePreview();
        if (isPreviewModeActive && Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) EscapePreview();
        // TODO: move at button keep pressed, exit when not
    }

    public void Move(int moveIndex)
    {
        if (isAnimating) return;
        if (isPreviewModeActive) EscapePreview();
    }

    public void Preview()
    {
        if (!isPreviewModeActive && !isAnimating) _PreviewSelected().Forget();
    }

    async UniTaskVoid _PreviewSelected()
    {
        isPreviewModeActive = isAnimating = true;
        originPosition = transform.position;
        originRotation = transform.rotation;
        var t = 0f;
        while (t < 1f)
        {
            transform.position = Vector3.Lerp(originPosition, previewT.position, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Vector3.zero), t);
            t += Time.deltaTime * 2;
            await UniTask.DelayFrame(1);
        }
        transform.position = previewT.position;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        ResetMouseRotationValues();
        isAnimating = false;
    }

    void ResetMouseRotationValues()
    {
        rotationX = 0f;
        rotationY = 0f;
        axisLastFrame = attachedCamera.ScreenToViewportPoint(Input.mousePosition);
    }

    public void EscapePreview()
    {
        _EscapePreview().Forget();
    }

    async UniTask _EscapePreview()
    {
        if (isAnimating || !isPreviewModeActive) return;
        isAnimating = true;
        var t = 0f;
        do
        {
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime * 2);
            transform.position = Vector3.Lerp(previewT.position, originPosition, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, originRotation, t);
            await UniTask.DelayFrame(1);
        } while (t < 1f);
        isAnimating = false;
        isPreviewModeActive = false;
    }

    void RotatePreview()
    {
        if (isPreviewModeActive && !isAnimating)
        {
            if (Input.GetMouseButtonDown(0))
            {
                axisLastFrame = attachedCamera.ScreenToViewportPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                axis = attachedCamera.ScreenToViewportPoint(Input.mousePosition);
                axisDelta = (axisLastFrame - axis) * 90f;
                axisLastFrame = axis;

                rotationX += axisDelta.x * mouseSensitivity;
                rotationY += axisDelta.y * mouseSensitivity;

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

                transform.rotation = originalRotation * xQuaternion * yQuaternion;
            }
        }
    }

    public bool IsBusy()
    {
        return isAnimating || isPreviewModeActive;
    }
}
