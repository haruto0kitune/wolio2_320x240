﻿using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

namespace Wolio.Actor.Player.Attacks.NormalAttacks.CrouchingAttacks
{
    public class PlayerCrouchingHighAttack : MonoBehaviour
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
        GameObject PlayerCrouchingHighAttackHitBox;
        BoxCollider2D HitBox;
        [SerializeField]
        GameObject PlayerCrouchingHighAttackHurtBox;
        BoxCollider2D HurtBox;
        [SerializeField]
        int damageValue;
        [SerializeField]
        int hitRecovery;
        [SerializeField]
        int Startup;
        [SerializeField]
        int Active;
        [SerializeField]
        int Recovery;

        void Awake()
        {
            Animator = Player.GetComponent<Animator>();
            ObservableStateMachineTrigger = Animator.GetBehaviour<ObservableStateMachineTrigger>();
            PlayerState = Player.GetComponent<PlayerState>();
            PlayerRigidbody2D = Player.GetComponent<Rigidbody2D>();
            Key = Player.GetComponent<Key>();
            BoxCollider2D = GetComponent<BoxCollider2D>();
            HitBox = PlayerCrouchingHighAttackHitBox.GetComponent<BoxCollider2D>();
            HurtBox = PlayerCrouchingHighAttackHurtBox.GetComponent<BoxCollider2D>();
        }

        void Start()
        {
            //Animation
            #region CrouchingLightAttack
            ObservableStateMachineTrigger
                 .OnStateEnterAsObservable()
                 .Where(x => x.StateInfo.IsName("Base Layer.CrouchingHighAttack"))
                 .Subscribe(_ => Animator.speed = 0);
            #endregion

            //Collision
            this.ObserveEveryValueChanged(x => Animator.GetBool("IsCrouchingHighAttack"))
                .Where(x => x)
                .Subscribe(_ => StartCoroutine(Attack()));

            // Damage
            PlayerCrouchingHighAttackHitBox.OnTriggerEnter2DAsObservable()
                .Where(x => x.gameObject.tag == "Enemy/HurtBox")
                .ThrottleFirstFrame(hitRecovery)
                .Subscribe(_ =>
                {
                    _.gameObject.GetComponent<DamageManager>().ApplyDamage(damageValue, hitRecovery);
                    HitBox.enabled = false;
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
            for (int i = 0; i < Recovery; i++)
            {
                yield return null;
            }

            BoxCollider2D.enabled = false;
            HurtBox.enabled = false;
            #endregion
            #region CrouchingLightAttack->Stand|Crouch
            if(Key.Vertical.Value != -1)
            {
                Animator.SetBool("IsStanding", true);
                Animator.SetBool("IsCrouchingHighAttack", false);
            }
            else if(Key.Vertical.Value == -1)
            {
                Animator.SetBool("IsCrouching", true);
                Animator.SetBool("IsCrouchingHighAttack", false);
            }
            #endregion
        }
    }
}