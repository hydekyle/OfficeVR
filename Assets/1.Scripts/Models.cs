using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpotPoint
{
    public int index;
    public Texture skyTexture;
}

/// <summary> Load and saves position and rotation for objects to be placed in scene correctly according to active scene photo </summary>
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

/// <summary> Any Transform that has to be moved according to the active scene photo </summary>
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