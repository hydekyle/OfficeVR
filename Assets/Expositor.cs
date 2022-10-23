using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expositor : MonoBehaviour
{
    public float rotationVelocity = 10f;
    public float radius = 1f;
    public Transform rotatorT;
    public Transform previewT;

    List<Transform> items = new();
    int index = 0;
    int previewIndex = 0;

    void Start()
    {
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
    }

    public void Move(int moveIndex)
    {
        index += moveIndex;
        previewIndex += moveIndex;
        if (previewIndex > items.Count - 1) previewIndex = 0;
        if (previewIndex < 0) previewIndex = items.Count - 1;
    }

    public void PreviewSelected()
    {
        var selected = items[previewIndex];
        selected.SetParent(previewT);
        selected.localPosition = Vector3.zero;
    }
}
