using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RPGSystem;
using UnityEngine;
using UnityObservables;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Expositor expositor;
    public List<SpotPoint> spotPoints;
    [HideInInspector]
    public static bool isPreviewMode;

    void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        isPreviewMode = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) spotPoints[0].Apply();
        if (Input.GetKeyDown(KeyCode.Alpha2)) spotPoints[1].Apply();
    }
}

[Serializable]
public class SpotPoint
{
    public Texture skyTexture;

    public void Apply()
    {
        RenderSettings.skybox.SetTexture("_Tex", skyTexture);
    }
}
