﻿using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public class PiyoStatus : MonoBehaviour
{
    [InspectorDisplay]
    public IntReactiveProperty Hp;

    void Start()
    {

    }
}