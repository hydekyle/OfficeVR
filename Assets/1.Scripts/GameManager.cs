using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RPGSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public RPGManager rpgManager = new();

    private void Awake()
    {
        if (Instance != null) Destroy(this.gameObject);
        else Instance = this;
    }
}
