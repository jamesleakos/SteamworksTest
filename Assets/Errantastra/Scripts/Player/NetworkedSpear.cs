using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Mirror;

namespace Errantastra
{
    public class NetworkedSpear : RaycastController
    {
        private HumanPlayer myPlayer;

        public float moveSpeed = 20;
        Animator animator;
        public Vector3 velocity;

        /// <summary>
        /// Delay until despawned automatically when nothing gets hit.
        /// </summary>
        public float despawnDelay = 1f;

        /// <summary>
        /// Clip to play when a player gets hit.
        /// </summary>
        public AudioClip hitClip;

        /// <summary>
        /// Clip to play when this projectile gets despawned.
        /// </summary>
        public AudioClip explosionClip;

        //reference to collider component
        private BoxCollider2D myCollider;

        public Transform tip;
        public Transform back;

        public float maxFlightTime = 1.0f;
        private float endFlightTime;

        public CollisionInfo collisions;


        /// <summary>
        /// Player gameobject that spawned this projectile.
        /// </summary>
        [HideInInspector]
        [SyncVar]
        public GameObject owner;


        public enum SpearState
        {
            held,
            flying,
            stuck
        }
        public SpearState spearState;


        //get component references
        protected override void Awake()
        {
            base.Awake();
            myCollider = GetComponent<BoxCollider2D>();
        }

        protected override void Start()
        {
            base.Start();
            myPlayer = gameObject.GetComponentInParent<HumanPlayer>();
        }

        private void Update()
        {
            if (Time.time > endFlightTime)
            {
                spearState = SpearState.stuck;
            }
            if (spearState == SpearState.flying && isServer)
            {
                RaycastCollisionDetection();
                Move(velocity * Time.deltaTime);
            }

            if (velocity == new Vector3(0, 0, 0) && spearState != SpearState.stuck) StopFlying();
        }

        void RaycastCollisionDetection()
        {
            float rayLength = Mathf.Abs(velocity.x * Time.deltaTime);
            Vector2 rayOrigin = tip.position;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, tip.position - back.position, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, (tip.position - back.position).normalized * rayLength, Color.red);

            if (hit)
            {
                if (gameObject.transform.lossyScale.x < 0)
                {
                    velocity.x = -1 * hit.distance / Time.deltaTime;
                }
                else
                {
                    velocity.x = hit.distance / Time.deltaTime;
                }
                velocity.y = 0;
            }
        }

        public void Move(Vector3 velocity)
        {
            UpdateRaycastOrigins();
            collisions.Reset();
            collisions.velocityOld = velocity;

            if (velocity.x != 0)
            {
                collisions.faceDir = (int)Mathf.Sign(velocity.x);
            }

            transform.Translate(velocity);
        }

        [ServerCallback]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            //Debug.Log("Spear hit " + collision.gameObject.name);
            //if (collision.tag == "Spear") return;
            //if (collision.tag == "Player") return;
            //{
            //    var p = collision.gameObject.GetComponent<HumanPlayer>();
            //    if (p == myPlayer) return;
            //    Debug.Log("myPlayer = " + myPlayer + ". EnemyPlayer = " + p + "weapon = " + gameObject.GetComponent<Weapon>());
            //    myPlayer.HitPlayerWithHandWeapon(p, gameObject.GetComponent<Weapon>());
            //}
            //StopFlying();
        }

        //set initial travelling velocity
        IEnumerator OnSpawn()
        {
            //for whatever reason, the spawned object has the correct orientation on the host already,
            //but not on the client. Wait one frame for the rotation to be applied client-side too
            yield return new WaitForEndOfFrame();
        }

        /// <summary>
        /// On Host, add automatic despawn coroutine
        /// </summary>
        public override void OnStartServer()
        {
            //PoolManager.Despawn(gameObject, despawnDelay);
        }

        public void StartFlying()
        {
            velocity = moveSpeed * Vector2.right;
            endFlightTime = Time.time + maxFlightTime;
        }

        private void StopFlying()
        {
            if (spearState == SpearState.flying)
            {
                spearState = SpearState.stuck;
                velocity.x = 0;
                velocity.y = 0;
            }
            Debug.Log("Stop Flying");
            gameObject.GetComponent<Weapon>().movementState = Weapon.MovementState.stuck;

            NetworkManager.Destroy(gameObject, despawnDelay);
        }

        //set despawn effects and reset variables
        void OnDespawn()
        {
            //skip for non-hosts
            if (!isServer) return;
            //server despawned this instance, despawn it for the network too
            NetworkServer.UnSpawn(gameObject);
        }
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public Vector2 direction;
        public Vector3 velocityOld;
        public int faceDir;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}

