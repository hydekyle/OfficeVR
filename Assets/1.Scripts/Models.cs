using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IItem
{
    public void Drop();
}

interface IUsableItem : IItem
{
    public void Use();
}

interface IReadableItem : IItem
{
    public void Read();
}

public class Pinga : MonoBehaviour, IReadableItem
{
    public void Read()
    {

    }

    public void Drop()
    {

    }

}