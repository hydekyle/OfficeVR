using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExpositionable
{
    public void Preview();
    public void EscapePreview();
    public bool IsBusy();
}
