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
        if (Input.GetKeyDown(KeyCode.Alpha1)) expositor.Move(-1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) expositor.Move(1);
    }
}
