using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObservables;

public class myCubo : MonoBehaviour
{
    public Observable<Color> ballColor;

    private void Start()
    {
        ballColor.OnChanged += () =>
        {
            GetComponent<MeshRenderer>().material.color = ballColor.Value;
        };
    }

}
