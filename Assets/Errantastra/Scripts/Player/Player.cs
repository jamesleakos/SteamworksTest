
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using TMPro;

namespace Errantastra
{          
    /// <summary>
    /// Networked player class implementing movement control and shooting.
	/// Contains both server and client logic in an authoritative approach.
    /// </summary> 
    /// 

	public class Player : Controller2D
    {

        #region Variables

        #region Name and Team

        /// <summary>
        /// Player name synced across the network.
        /// </summary>
        [HideInInspector]
        [SyncVar]
        public string myName;

        /// <summary>
        /// Team value assigned by the server.
        /// </summary>
		[HideInInspector]
        [SyncVar]
        public int teamIndex;

        #endregion

        #region Scale and Appearance
        [Header("Body Parts")]

        public Transform bodyAndWeapons;

        [HideInInspector]
        [SyncVar(hook = "OnHeadRotation")]
        public Quaternion bodyAndWeaponsRotation;

        public float remoteRotationSpeed = 30.0f;

        //limit for sending body rotation updates
        protected float sendRate = 0.05f;
        //timestamp when next rotate update should happen
        protected float nextRotate;
        
        #endregion

        #region Last Killed Me

        /// <summary>
        /// Last player gameobject that killed this one.
        /// </summary>
        [HideInInspector]
        public GameObject killedBy;

        #endregion

        #region Movement

        public float blockSpeed = 5;
        public float walkSpeed = 12;
        public float runSpeed = 25;
        public float rollSpeed = 175;

        public float rollTime = 0.10f;
        protected float endRollTime;

        public float rollCoolDownTime = 0.5f;
        protected float endRollCoolDownTime;
        protected bool waitingToRoll;

        protected bool waitingToAttack;

        public enum MovementState
        {
            walking,
            running, 
            blocking,
            rolling
        }
        [SyncVar]
        public MovementState movementState;

        public enum AttackingState
        {
            notAttacking,
            normalAttack,
            longNormalAttack,
            shieldAttack,
            longShieldAttack,
            throwingSpear
        }
        [SyncVar]
        public AttackingState attackingState;

        [SyncVar]
        public bool takingAction = false;


        #endregion

        #endregion

        #region Start, Awake, etc. 

        //called before SyncVar updates
        protected override void Awake()
        {
            base.Awake();
            //saving maximum health value
            //before it gets overwritten by the network
            //maxHealth = health;



        }

        protected override void Start()
        {
            base.Start();
        }

        protected virtual void Update()
        {
            CheckForAnimationState();
        }

        private void CheckForAnimationState ()
        {
            if (false)
            {
                //RpcSwitchAnimationState();
            }
        }

        [ClientRpc]
        public void RpcSwitchAnimationState()
        {
            //Debug.Log("here we animate");
        }

        #endregion

        #region Appearance and Scale

        //Command telling the server the updated body rotation
        [Command]
        protected void CmdRotateHead(Quaternion value)
        {
            bodyAndWeapons.rotation = value;
            bodyAndWeaponsRotation = value;
        }

        //hook for updating body rotation locally
        protected void OnHeadRotation(Quaternion oldValue, Quaternion newValue)
        {
            //ignore value updates for our own player,
            //so we can update the rotation server-independent
            if (isLocalPlayer) return;

            bodyAndWeaponsRotation = newValue;
            Quaternion rotation = Quaternion.RotateTowards(bodyAndWeaponsRotation, newValue, remoteRotationSpeed);

            bodyAndWeapons.rotation = rotation;
        }

        #endregion

    }
}
