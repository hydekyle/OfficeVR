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
    [HideInInspector]
    SpotPoint actualSpotPoint;

    void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        isPreviewMode = false;
        actualSpotPoint = new() { skyTexture = RenderSettings.skybox.GetTexture("_Tex") };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) WalkTo(spotPoints[0]).Forget();
        if (Input.GetKeyDown(KeyCode.Alpha2)) WalkTo(spotPoints[1]).Forget();
    }

    async UniTaskVoid WalkTo(SpotPoint spotPoint)
    {
        Camera camera = Camera.main;
        // var startFocalLenght = camera.focalLength;
        // var targetFocalLenght = startFocalLenght + camera.focalLength / 2;
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        RenderSettings.skybox.SetTexture("_Tex", actualSpotPoint.skyTexture);
        RenderSettings.skybox.SetTexture("_OverlayTex", spotPoint.skyTexture);
        var t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp(t + Time.deltaTime * 2, 0f, 1f);
            RenderSettings.skybox.SetFloat("_Blend", t);
            //camera.focalLength = Mathf.Lerp(camera.focalLength, targetFocalLenght, t);
            await UniTask.DelayFrame(1);
        }
        RenderSettings.skybox.SetTexture("_Tex", spotPoint.skyTexture);
        RenderSettings.skybox.SetTexture("_OverlayTex", actualSpotPoint.skyTexture);
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        //camera.focalLength = startFocalLenght;
        actualSpotPoint = spotPoint;
    }
}

[Serializable]
public class SpotPoint
{
    public Texture skyTexture;
}
