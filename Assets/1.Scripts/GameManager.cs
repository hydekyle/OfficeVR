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
    int moveIndex = 0;

    void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        isPreviewMode = false;
        actualSpotPoint = new() { skyTexture = RenderSettings.skybox.GetTexture("_Tex") };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) WalkTo(spotPoints[MoveIndex(-1)]).Forget();
        if (Input.GetKeyDown(KeyCode.Alpha2)) WalkTo(spotPoints[MoveIndex(1)]).Forget();
    }

    int MoveIndex(int adition)
    {
        moveIndex = Mathf.Clamp(moveIndex + adition, 0, spotPoints.Count - 1);
        return moveIndex;
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
            print("vaya");
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
