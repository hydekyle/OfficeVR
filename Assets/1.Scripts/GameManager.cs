using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RPGSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Expositor expositor;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) expositor.Move(-1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) expositor.Move(1);
    }
}
