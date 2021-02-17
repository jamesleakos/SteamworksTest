using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Mirror;

namespace Errantastra
{
    public class Spear : MonoBehaviour
    {
        private HumanPlayer myPlayer;

        /// <summary>
        /// Clip to play when a player gets hit.
        /// </summary>
        public AudioClip hitClip;

        //reference to collider component
        private BoxCollider2D myCollider;

        public Transform tip;
        public Transform back;

        public enum SpearState
        {
            held,
            flying,
            stuck
        }
        public SpearState spearState;

        //get component references
        protected void Awake()
        {
            myCollider = GetComponent<BoxCollider2D>();
        }

        protected void Start()
        {
            myPlayer = gameObject.GetComponentInParent<HumanPlayer>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                HumanPlayer hitPlayer = collision.gameObject.GetComponent<HumanPlayer>();
                if (hitPlayer == myPlayer) return;
                myPlayer.CmdHitPlayerWithHandWeapon(hitPlayer);
            }
        }
    }
}

