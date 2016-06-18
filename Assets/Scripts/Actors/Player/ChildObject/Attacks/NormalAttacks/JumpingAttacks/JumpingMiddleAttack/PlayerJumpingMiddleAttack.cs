﻿using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

namespace Wolio.Actor.Player.Attacks.NormalAttacks.JumpingAttacks
{
    public class PlayerJumpingMiddleAttack : MonoBehaviour
    {
        [SerializeField]
        GameObject Player;
        Animator Animator;
        ObservableStateMachineTrigger ObservableStateMachineTrigger;
        PlayerState PlayerState;
        Rigidbody2D PlayerRigidbody2D;
        Key Key;
        BoxCollider2D BoxCollider2D;
        [SerializeField]
        GameObject PlayerJumpingMiddleAttackHitBox;
        BoxCollider2D HitBox;
        [SerializeField]
        GameObject PlayerJumpingMiddleAttackHurtBox;
        BoxCollider2D HurtBox;
        [SerializeField]
        int damageValue;
        [SerializeField]
        int hitRecovery;
        [SerializeField]
        int hitStop;
        [SerializeField]
        int Startup;
        [SerializeField]
        int Active;
        [SerializeField]
        bool isTechable;
        [SerializeField]
        bool hasKnockdownAttribute;
        [SerializeField]
        AttackAttribute attackAttribute;
        [SerializeField]
        KnockdownAttribute knockdownAttribute;
        bool wasFinished;
        bool isCancelable;
        bool wasCanceled;
        Coroutine coroutineStore;

        void Awake()
        {
            Animator = Player.GetComponent<Animator>();
            ObservableStateMachineTrigger = Animator.GetBehaviour<ObservableStateMachineTrigger>();
            PlayerState = Player.GetComponent<PlayerState>();
            PlayerRigidbody2D = Player.GetComponent<Rigidbody2D>();
            Key = Player.GetComponent<Key>();
            BoxCollider2D = GetComponent<BoxCollider2D>();
            HitBox = PlayerJumpingMiddleAttackHitBox.GetComponent<BoxCollider2D>();
            HurtBox = PlayerJumpingMiddleAttackHurtBox.GetComponent<BoxCollider2D>();
        }

        void Start()
        {
            // Animation
            #region EnterJumpingMiddleAttack
            ObservableStateMachineTrigger
                 .OnStateEnterAsObservable()
                 .Where(x => x.StateInfo.IsName("Base Layer.JumpingMiddleAttack"))
                 .Subscribe(_ => Animator.speed = 0);
            #endregion
            #region JumpingMiddleAttack->Stand
            ObservableStateMachineTrigger
                .OnStateUpdateAsObservable()
                .Where(x => x.StateInfo.IsName("Base Layer.JumpingMiddleAttack"))
                .Where(x => PlayerState.IsGrounded.Value)
                .Subscribe(_ =>
                {
                    Animator.SetBool("IsJumpingMiddleAttack", false);
                    Animator.SetBool("IsStanding", true);
                    wasFinished = false;
                });
            #endregion
            #region JumpingMiddleAttack->JumpingHighAttack
            ObservableStateMachineTrigger
                .OnStateUpdateAsObservable()
                .Where(x => x.StateInfo.IsName("Base Layer.JumpingMiddleAttack"))
                .Where(x => isCancelable)
                .Where(x => Key.C)
                .Subscribe(_ =>
                {
                    Animator.SetBool("IsJumpingMiddleAttack", false);
                    Animator.SetBool("IsJumpingHighAttack", true);
                    isCancelable = false;
                    StopCoroutine(coroutineStore);
                    wasCanceled = true;
                });
            #endregion

            // Collision
            this.ObserveEveryValueChanged(x => Animator.GetBool("IsJumpingMiddleAttack"))
                .Where(x => x)
                .Subscribe(_ => coroutineStore = StartCoroutine(Attack()));

            this.ObserveEveryValueChanged(x => wasCanceled)
                .Where(x => x)
                .Subscribe(_ => Cancel());

            // Damage
            PlayerJumpingMiddleAttackHitBox.OnTriggerEnter2DAsObservable()
                .Where(x => x.gameObject.tag == "Enemy/HurtBox")
                .Subscribe(_ =>
                {
                    _.gameObject.GetComponent<DamageManager>().ApplyDamage(damageValue, hitRecovery, hitStop, isTechable, hasKnockdownAttribute, attackAttribute, knockdownAttribute);
                    HitBox.enabled = false;
                    isCancelable = true;
                });
        }

        public IEnumerator Attack()
        {
            #region Startup
            BoxCollider2D.enabled = true;
            HurtBox.enabled = true;

            for (int i = 0; i < Startup; i++)
            {
                yield return null;
            }
            #endregion
            #region Active
            Animator.speed = 1;
            HitBox.enabled = true;

            for (var i = 0; i < Active; i++)
            {
                yield return null;
            }

            HitBox.enabled = false;
            #endregion
            #region Recovery
            while (!PlayerState.IsGrounded.Value)
            {
                yield return null;
            }
            
            // This needs to enable collision of next state.
            // First of all, It should enable to collision of next state.
            // Otherwise, Player become strange motion.
            wasFinished = true;
            yield return null;

            BoxCollider2D.enabled = false;
            HurtBox.enabled = false;
            isCancelable = false;
            #endregion
        }

        void Cancel()
        {
            StopCoroutine(coroutineStore);

            // Collision disable
            BoxCollider2D.enabled = false;
            HurtBox.enabled = false;
            
            wasCanceled = false;
        }
    }
}