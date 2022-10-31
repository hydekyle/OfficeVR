using System;
using System.Collections;
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
    [HideInInspector]
    SpotPoint actualSpotPoint;
    Observable<int> moveIndex = new() { Value = -1 };
    public RepositionManager rManager;

    void Awake()
    {
        if (Instance) Destroy(this.gameObject);
        else Instance = this;
        actualSpotPoint = new() { skyTexture = RenderSettings.skybox.GetTexture("_Tex") };
        moveIndex.OnChanged += () =>
        {
            WalkTo(spotPoints[moveIndex.Value]).Forget();
            RPGManager.GameData.AddToVariable(0, moveIndex.Value);
            //print(moveIndex.Value);
        };
    }

    void Start()
    {
        MoveIndex(0);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1)) MoveIndex(-1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) MoveIndex(1);
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

    // public void Save()
    // {
    //     var fileName = "/savedata";
    //     var savePath = String.Concat(Application.dataPath, fileName);
    //     string saveData = JsonUtility.ToJson(this, true);
    //     BinaryFormatter bf = new();
    //     FileStream file = File.Create(savePath);
    //     bf.Serialize(file, saveData);
    //     file.Close();
    // }

    // public void Load()
    // {
    //     var file = Resources.Load<TextAsset>("savedata.txt");
    //     print(file);
    //     JsonUtility.FromJsonOverwrite(file.text, this);
    // }
}

[Serializable]
public class SpotPoint
{
    public int index;
    public Texture skyTexture;
}

[Serializable]
public class RepositionManager
{
    public List<RepositionableInteractables> repositionableList = new();

    public void Apply(int index)
    {
        foreach (var repo in repositionableList) repo.Apply(index);
    }

    public void Save(int index)
    {
        foreach (var repo in repositionableList) repo.Save(index);
    }
}

[Serializable]
public class RepositionableInteractables
{
    public Transform myTransform;
    [SerializeField]
    public List<Repositionable> mySavedPositions = new();

    public void Apply(int index)
    {
        Repositionable repositionable = mySavedPositions[index];
        myTransform.position = repositionable.position;
        myTransform.rotation = Quaternion.Euler(repositionable.rotation);
        myTransform.localScale = repositionable.scale;
    }

    public void Save(int index)
    {
        mySavedPositions[index] = new()
        {
            position = myTransform.position,
            rotation = myTransform.rotation.eulerAngles,
            scale = myTransform.localScale
        };
    }
}

[Serializable]
public struct Repositionable
{
    public Vector3 position, rotation, scale;
}