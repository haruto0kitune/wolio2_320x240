﻿using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

namespace Wolio.Actor.Player
{
    public class PlayerStandingUpFromProneKnockdown : MonoBehaviour
    {
        [SerializeField]
        GameObject Player;
        Animator Animator;
        ObservableStateMachineTrigger ObservableStateMachineTrigger;
        PlayerState PlayerState;
        Key Key;
        BoxCollider2D BoxCollider2D;
        bool isFinished;
        [SerializeField]
        int knockdownRecovery;
        Coroutine coroutineStore;

        void Awake()
        {
            Animator = Player.GetComponent<Animator>();
            ObservableStateMachineTrigger = Animator.GetBehaviour<ObservableStateMachineTrigger>();
            PlayerState = Player.GetComponent<PlayerState>();
            Key = Player.GetComponent<Key>();
            BoxCollider2D = GetComponent<BoxCollider2D>();
        }

        void Start()
        {
            //Animation
            #region EnterStandingUpFromProneKnockdown
            ObservableStateMachineTrigger
                .OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Base Layer.StandUpFromProneKnockdown"))
                .Subscribe(_ => coroutineStore = StartCoroutine(StandingUpFromProneKnockdownCoroutine()));

            #endregion
            #region StandingUpFromProneKnockdown->Stand
            ObservableStateMachineTrigger
                .OnStateUpdateAsObservable()
                .Where(x => x.StateInfo.IsName("Base Layer.StandUpFromProneKnockdown"))
                .Where(x => isFinished)
                .Subscribe(_ =>
                {
                    Animator.SetBool("IsStandingUpFromProneKnockdown", false);
                    Animator.SetBool("IsStanding", true);
                    isFinished = false;
                });
            #endregion

            //Collision
            this.ObserveEveryValueChanged(x => Animator.GetBool("IsStandingUpFromProneKnockdown"))
                .Where(x => x)
                .Subscribe(_ => BoxCollider2D.enabled = true);

            this.ObserveEveryValueChanged(x => Animator.GetBool("IsStandingUpFromProneKnockdown"))
                .Where(x => !x)
                .Subscribe(_ => BoxCollider2D.enabled = false);
        }

        IEnumerator StandingUpFromProneKnockdownCoroutine()
        {
            // Recovery
            for (int i = 0; i < knockdownRecovery; i++)
            {
                yield return null;
            }

            isFinished = true;
        }
    }
}