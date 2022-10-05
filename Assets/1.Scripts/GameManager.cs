using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
    }

    async void Start2()
    {
        await UniTask.Delay(1000);
    }

    void Update()
    {

    }
}
