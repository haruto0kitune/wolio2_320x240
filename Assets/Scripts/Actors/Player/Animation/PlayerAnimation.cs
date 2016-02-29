﻿using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

public partial class PlayerAnimation : MonoBehaviour
{
    private Animator Animator;
    private ObservableStateMachineTrigger ObservableStateMachineTrigger;
    private Key Key;

    void Awake()
    {
        Animator = GetComponent<Animator>();
        ObservableStateMachineTrigger = Animator.GetBehaviour<ObservableStateMachineTrigger>();
        Key = GetComponent<Key>();
    }

    void Start()
    {
        Stand();
        Run();
    }
}