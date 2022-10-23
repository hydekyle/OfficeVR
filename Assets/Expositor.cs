using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WebXR;

public class Expositor : MonoBehaviour
{
    public DesertFreeFlightController cameraController;
    public float rotationVelocity = 10f;
    public float radius = 1f;
    public Transform rotatorT;
    public Transform previewT, spawnT;
    List<Transform> items = new();
    int index = 0;
    int previewIndex = 0;

    #region ItemRotationController
    [Tooltip("Mouse sensitivity")]
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

    void Start()
    {
        attachedCamera = Camera.main;
        originalRotation = previewT.localRotation;
        foreach (Transform t in transform.Find("Items")) items.Add(t);
        float angle = 0f;
        for (var x = 0; x < items.Count; x++)
        {
            angle += 360 / items.Count;
            Vector3 pos = transform.position;
            pos.x += radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.z += radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos = transform.rotation * pos;
            items[x].position = pos;
            items[x].LookAt(transform);
        }
        rotatorT.position = new Vector3(items[0].position.x, rotatorT.position.y, items[0].position.z);
        rotatorT.parent = null;
        previewT.SetParent(Camera.main.transform);
        previewT.localPosition = Vector3.forward * 1.5f;
    }

    void Update()
    {
        var targetRot = (360 / items.Count) * index;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Vector3.up * targetRot), Time.deltaTime * rotationVelocity);
        RotatePreview();
    }

    public async void Move(int moveIndex)
    {
        if (isAnimating) return;
        if (GameManager.isPreviewMode) await EscapePreview();
        index += moveIndex;
        previewIndex += moveIndex;
        if (previewIndex > items.Count - 1) previewIndex = 0;
        if (previewIndex < 0) previewIndex = items.Count - 1;
    }
    bool isAnimating = false;

    public async void PreviewSelected()
    {
        if (isAnimating) return;
        if (GameManager.isPreviewMode)
        {
            EscapePreview().Forget();
            return;
        }
        rotationX = 0f;
        rotationY = 0f;
        var selected = items[previewIndex].GetChild(0);
        childPreview = selected;
        selected.position = spawnT.position;
        GameManager.isPreviewMode = isAnimating = true;
        while (selected.position != previewT.position || selected.rotation.eulerAngles != Vector3.zero)
        {
            selected.position = Vector3.MoveTowards(selected.position, previewT.position, Time.deltaTime * rotationVelocity);
            selected.rotation = Quaternion.Lerp(selected.rotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotationVelocity);
            await UniTask.DelayFrame(1);
        }
        isAnimating = false;
        //selected.position = spawnT.position;
    }

    public async UniTask EscapePreview()
    {
        GameManager.isPreviewMode = false;
        var selected = childPreview;
        isAnimating = true;
        while (selected.position != spawnT.position)
        {
            selected.position = Vector3.MoveTowards(selected.position, spawnT.position, Time.deltaTime * rotationVelocity);
            selected.rotation = Quaternion.Lerp(selected.rotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * rotationVelocity);
            await UniTask.DelayFrame(1);
        }
        isAnimating = false;
        selected.localPosition = Vector3.zero;
    }

    Transform childPreview;
    void RotatePreview()
    {
        if (GameManager.isPreviewMode)
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

                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

                childPreview.rotation = originalRotation * xQuaternion * yQuaternion;
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
