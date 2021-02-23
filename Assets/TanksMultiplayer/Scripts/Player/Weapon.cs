using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Mirror;

namespace Errantastra
{
    public class Weapon : MonoBehaviour
    {
        [HideInInspector]
        public HumanPlayer myPlayer;

        /// <summary>
        /// Clip to play when a player gets hit.
        /// </summary>
        public AudioClip hitClip;

        //reference to collider component
        private BoxCollider2D myCollider;

        public Transform tip;
        public Transform back;

        public enum WeaponType
        {
            spear,
            shield
        }
        public WeaponType weaponType;

        public enum MovementState
        {
            held,
            flying,
            stuck
        }
        public MovementState movementState;

        //get component references
        protected void Awake()
        {
            myCollider = GetComponent<BoxCollider2D>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player" && movementState != MovementState.stuck)
            {
                HumanPlayer hitPlayer = collision.gameObject.GetComponent<HumanPlayer>();
                if (hitPlayer == myPlayer) return;
                myPlayer.HitPlayerWithHandWeapon(hitPlayer, this);
            }
        }
    }
}

