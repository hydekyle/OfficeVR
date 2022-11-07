using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;
using RPGSystem;
using UnityEngine;
using UnityObservables;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<SpotPoint> spotPoints;
    public RepositionManager rManager;
    SpotPoint actualSpotPoint;
    Observable<int> moveIndex = new() { Value = -1 };

    void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        actualSpotPoint = new() { skyTexture = RenderSettings.skybox.GetTexture("_Tex") };
        moveIndex.OnChanged += () =>
        {
            WalkTo(spotPoints[moveIndex.Value]).Forget();
            RPGManager.GameData.AddToVariable(0, moveIndex.Value);
        };
    }

    void Start()
    {
        MoveIndex(0);
    }

    void Update()
    {
#if UNITY_EDITOR
        // if (Input.GetKeyDown(KeyCode.Alpha1)) MoveIndex(-1);
        // if (Input.GetKeyDown(KeyCode.Alpha2)) MoveIndex(1);
        if (Input.GetKeyDown(KeyCode.F1)) rManager.Save(moveIndex.Value);
        // if (Input.GetKeyDown(KeyCode.F2)) Load();
#endif
    }

    int MoveIndex(int adition)
    {
        moveIndex.Value = Mathf.Clamp(moveIndex.Value + adition, 0, spotPoints.Count - 1);
        return moveIndex.Value;
    }

    public void MoveToIndex(int index)
    {
        moveIndex.Value = index;
    }

    async UniTaskVoid WalkTo(SpotPoint spotPoint)
    {
        rManager.Apply(moveIndex.Value);
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

    public void Save()
    {
        var fileName = "/savedata";
        var savePath = String.Concat(Application.dataPath, fileName);
        string saveData = JsonUtility.ToJson(this, true);
        BinaryFormatter bf = new();
        FileStream file = File.Create(savePath);
        bf.Serialize(file, saveData);
        file.Close();
    }

    public void Load()
    {
        var fileName = "/savedata";
        var savePath = String.Concat(Application.dataPath, fileName);
        var file = Resources.Load<TextAsset>("savedata.txt");
        print(file);
        JsonUtility.FromJsonOverwrite(file.text, this);
    }
}