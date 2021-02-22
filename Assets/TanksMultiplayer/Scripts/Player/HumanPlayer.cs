using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using HeathenEngineering.SteamApi.Networking;
using Steamworks;
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.Foundation.UI;
using UnityEngine.Serialization;

namespace Errantastra {

    public class HumanPlayer : Player
    {
        #region Variables
        // START VARIABLE REGION

        #region Steamworks Stuff

        public SteamSettings steamSettings;
        [FormerlySerializedAs("LobbySettings")]
        public SteamworksLobbySettings lobbySettings;
        public SteamUserData authorityUser;
        //public SteamUserFullIcon SteamIcon;
        /// <summary>
        /// Simply for demonstration purpses only
        /// </summary>
        [SyncVar(hook = nameof(HandleSteamIdUpdate))]
        public ulong steamId = CSteamID.Nil.m_SteamID;


        #endregion

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

        #region Camera

        Camera mainCamera;

        #endregion

        #region Animation
        public Animator animator;
        const string normalAttack = "normalAttack";
        const string longNormalAttack = "longNormalAttack";
        const string shieldAttack = "shieldAttack";
        const string longShieldAttack = "longShieldAttack";
        const string throwingSpear = "throwingSpear";
        const string block = "block";
        const string idle = "idle";

        private float normalAttackLength;
        private float longNormalAttackLength;
        private float shieldAttackLength;
        private float longShieldAttackLength;
        private float throwingAnimLength;
        private float attackPatienceBuffer = 0.1f;
        private float endAttackTime;
        
        public enum AnimationState
        {
            normalAttack,
            longNormalAttack,
            shieldAttack,
            longShieldAttack,
            throwingSpear,
            block,
            idle
        }
        public AnimationState animationState;

        #endregion

        #region Rolling

        private Vector3 mouseDownPoint;
        private Vector3 mouseUpPoint;
        private Vector3 mouseDownPointMoving;
        private Vector3 mouseUpPointMoving;
        private float mouseDownTime;

        private Vector3 rollDirection;

        #endregion

        #region Shield and Laser

        // shield and laser 
        [SyncVar(hook = "OnShieldChange")]
        public int shield = 0;

        #endregion

        #region Throwing Spear

        [HideInInspector]
        [SyncVar]
        public bool holdingSpear;

        public Transform hand;

        public GameObject networkedSpearPrefab;
        private GameObject networkedSpearClone;

        public GameObject spearPrefab;
        private GameObject spearClone;

        private HeathenCustomNetworkManager networkManager;

        #endregion

        // END VARIABLE REGION
        #endregion

        // Keep all Steamworks functions in here and eventually move them to a separate player script that can just be attached to the player
        #region Steamworks functions

        /// <summary>
        /// Note that this is only called on a client when the server has updated the SyncValue
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        private void HandleSteamIdUpdate(ulong oldId, ulong newId)
        {
            //Gets called when the steamId is being synced by the server
            Debug.Log("New steam ID recieved from the server: previous value = " + steamId.ToString() + " new value = " + newId.ToString());
            steamId = newId;
            SetSteamIconData();
        }

        /// <summary>
        /// This is called when the SteamId updates for this user
        /// The HandleSteamIdUpdate method calls it and that method is called by the network system when the steamId field is updated.
        /// </summary>
        private void SetSteamIconData()
        {
            //If its an invalid ID dont bother doign anything
            if (steamId == CSteamID.Nil.m_SteamID)
                return;

            authorityUser = steamSettings.client.GetUserData(new CSteamID(steamId));
            //SteamIcon.LinkSteamUser(authorityUser);

            Debug.Log("Linking persona data for: [" + steamId.ToString() + "] " + (string.IsNullOrEmpty(authorityUser.DisplayName) ? "Unknown User" : authorityUser.DisplayName));
        }

        /// <summary>
        /// Only called on the client that has authority over this behaviour
        /// </summary>
        public override void OnStartAuthority()
        {
            Debug.Log("Player Controller On Start Authority has been called!");

            steamId = SteamUser.GetSteamID().m_SteamID;
            SetSteamIconData();

            //Have the authority instance of this object call the server and notify it of the local users CSteamID
            CmdSetSteamId(SteamUser.GetSteamID().m_SteamID);
        }

        [Command(channel = 0)]
        void CmdSetSteamId(ulong steamId)
        {
            Debug.Log("The server received a request from connection " + connectionToClient.connectionId + " to set the SteamId of this object to " + steamId.ToString());

            this.steamId = steamId;
        }


        #endregion

        #region Start and Update, etc.
        /// <summary>
        /// Initialize synced values on every client.
        /// </summary>
        public override void OnStartClient()
        {
            //get corresponding team and colorize renderers in team color
            //Team team = GameManager.GetInstance().teams[teamIndex];
            // probably need to assign some color or something here


            // old code

            //for(int i = 0; i < renderers.Length; i++)
            //    renderers[i].material = team.material;

            //set name in label
            //label.text = myName;

            OnShieldChange(0, shield);
        }

        /// <summary>
        /// Initialize camera and input for this local client.
        /// This is being called after OnStartClient.
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            //initialized already on host migration
            if (GameManager.GetInstance().localPlayer != null)
                return;

            //set a global reference to the local player
            GameManager.GetInstance().localPlayer = this;

            mainCamera = Camera.main;
            mainCamera.GetComponent<FollowTarget>().target = gameObject.transform;

            UpdateAnimClipTimes();

            foreach (var hp in Object.FindObjectsOfType<HumanPlayer>())
            {
                if (hp.holdingSpear) hp.LoadSpear();
            }
        }

        protected override void Start()
        {
            networkManager = GameObject.FindObjectOfType<HeathenCustomNetworkManager>();
            networkedSpearPrefab = networkManager.spawnPrefabs.Find(x => x.name == "NetworkedSpear");

            CmdSpearStart();
        }

        protected override void Update()
        {
            base.Update();

            //skip further calls for remote clients
            if (!isLocalPlayer)
            {
                //keep body rotation updated for all clients
                OnHeadRotation(bodyAndWeaponsRotation, bodyAndWeaponsRotation);
                return;
            }

            DetermineMovementInputs();
            DetermineAttackingAndRollingInputs();
            MoveCharacter();
        }

        protected void LateUpdate()
        {
            DetermineAnim();
        }

        // Start and Update Helpers

        public void UpdateAnimClipTimes()
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                switch (clip.name)
                {
                    case "normalAttack":
                        normalAttackLength = clip.length;
                        break;
                    case "longNormalAttack":
                        longNormalAttackLength = clip.length;
                        break;
                    case "shieldAttack":
                        shieldAttackLength = clip.length;
                        break;
                    case "longShieldAttack":
                        longShieldAttackLength = clip.length;
                        break;
                    case "throwingSpear":
                        throwingAnimLength = clip.length;
                        break;
                }
            }
        }

        [Command]
        private void CmdEndAction()
        {
            takingAction = false;
        }

        #endregion

        #region Player Input Functions

        [Client]
        private void DetermineMovementInputs()
        {
            if (Input.GetKey(KeyCode.Space)) PreSetMovementState(MovementState.blocking);
            else if (Input.GetKey(KeyCode.LeftShift) && !takingAction && movementState != MovementState.rolling) PreSetMovementState(MovementState.running);
            else if (movementState != MovementState.rolling) PreSetMovementState(MovementState.walking);
        }

        [Client]
        private void DetermineAttackingAndRollingInputs()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                mouseDownPoint = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
                mouseDownPointMoving = mouseDownPoint - gameObject.transform.position;
                mouseDownTime = Time.time;
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                mouseUpPoint = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
                mouseUpPointMoving = mouseUpPoint - gameObject.transform.position;

                if ((mouseUpPoint - mouseDownPoint).magnitude > 1 && (mouseUpPointMoving - mouseDownPointMoving).magnitude > 1 && movementState != MovementState.blocking)
                {
                    waitingToRoll = true;
                }
                else
                {
                    endAttackTime = Time.time + throwingAnimLength;
                    ClientThrowSpear(MousePosition());
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                waitingToAttack = true;
            }

            if (movementState == MovementState.rolling)
            {
                if (movementState == MovementState.rolling)
                {
                    ContinueRoll();
                }
                return;
            }

            if (waitingToAttack && !takingAction)
            {
                if (movementState == MovementState.blocking)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        endAttackTime = Time.time + longShieldAttackLength;
                        ClientLongShieldAttack(MousePosition());
                    }
                    else
                    {
                        endAttackTime = Time.time + shieldAttackLength;
                        ClientShieldAttack(MousePosition());
                    }
                }
                else
                {
                    if (movementState == MovementState.running)
                    {
                        endAttackTime = Time.time + longNormalAttackLength;
                        ClientLongNormalAttack(MousePosition());
                    }
                    else
                    {
                        endAttackTime = Time.time + normalAttackLength;
                        ClientNormalAttack(MousePosition());
                    }
                }

                waitingToAttack = false;
            }

            if (waitingToRoll && Time.time > endRollCoolDownTime && !takingAction)
            {
                StartRoll(mouseUpPoint);
                waitingToRoll = false;
            }
        }

        [Client]
        private void MoveCharacter()
        {
            // turn the next line off to enable moving attacks - turn the move line off to just enable swinging
            if (takingAction) return;
            if (movementState == MovementState.rolling) return;

            Vector2 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            playerInput = playerInput.normalized;
            float movementSpeed;

            if (movementState == MovementState.blocking)
            {
                movementSpeed = blockSpeed;
                if (!Input.GetKey(KeyCode.Mouse1)) RotateToMouse();
            }
            else if (movementState == MovementState.running)
            {
                movementSpeed = runSpeed;
                RotateToMovementDirection(playerInput);
            }
            else
            {
                movementSpeed = walkSpeed;
                if (!Input.GetKey(KeyCode.Mouse1)) RotateToMouse();
            }

            Vector2 velocity = playerInput * (movementSpeed);
            Move(velocity * Time.deltaTime);
        }

        #endregion

        #region Movement States, Rotating, and Look Directions
        
        private void PreSetMovementState(MovementState setState)
        {
            if (movementState != setState) CmdSetMovementState(setState);
        }

        [Command]
        private void CmdSetMovementState(MovementState setState)
        {
            movementState = setState;
        }

        [ClientRpc]
        private void RpcShouldntHaveToDoThis()
        {
            takingAction = false;
        }

        private void RotateToMouse()
        {
            Vector2 mousePosition = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            Vector2 firePointPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 dir = mousePosition - firePointPosition;
            dir.Normalize();

            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            bodyAndWeapons.rotation = rotation;

            SendRotate(rotation);
        }

        private void RotateToPosition(Vector3 pos)
        {
            Vector3 firePointPosition = new Vector3(transform.position.x, transform.position.y, 0);
            Vector3 dir = pos - firePointPosition;
            dir.Normalize();

            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            bodyAndWeapons.rotation = rotation;
        }

        private void RotateToMovementDirection(Vector2 vector2)
        {
            Quaternion finalRotation = Quaternion.Euler(0, 0, Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg);
            Quaternion rotation = Quaternion.RotateTowards(bodyAndWeaponsRotation, finalRotation, remoteRotationSpeed);

            bodyAndWeapons.rotation = rotation;

            SendRotate(rotation);
        }

        private void SendRotate(Quaternion quaternion)
        {
            if (Time.time >= nextRotate)
            {
                //set next update timestamp and send to server
                nextRotate = Time.time + sendRate;
                if (isClient) CmdRotateHead(quaternion);
            }
        }

        private Vector3 MousePosition()
        {
            return new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
        }

        bool IsLookingAtObject(Transform looker, Vector3 targetPos, float FOVAngle)
        {
            Vector3 direction = targetPos - looker.position;
            float ang = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float lookerAngle = looker.eulerAngles.z;
            float checkAngle = 0f;

            if (ang >= 0f)
                checkAngle = ang - lookerAngle - 90f;
            else if (ang < 0f)
                checkAngle = ang - lookerAngle + 270f;

            if (checkAngle < -180f)
                checkAngle = checkAngle + 360f;

            if (checkAngle <= FOVAngle * .5f && checkAngle >= -FOVAngle * .5f)
                return true;
            else
                return false;
        }

        #endregion

        #region Dealing with Attack Outcomes

        // hit player with melee weapon (held or thrown, somewhat confusingly)
        [Command]
        public void CmdHitPlayerWithHandWeapon(HumanPlayer hitPlayer)
        {
            if (hitPlayer.movementState == MovementState.blocking)
            {
                if (IsLookingAtObject(hitPlayer.gameObject.transform, gameObject.transform.position,45.0f))
                {
                    Debug.Log("Attack blocked");
                    return;
                }
            }

            // kill player
            hitPlayer.GetKilled(this);
        }

        // projectile like laser, not a thrown melee weapon, which will still call the melee script.
        [Server]
        public void HitPlayerWithLaser(HumanPlayer hitPlayer)
        {
            hitPlayer.GetHitWithLaser(this);
        }
        [Server]
        public void GetHitWithLaser(HumanPlayer attackingPlayer)
        {
            //reduce shield on hit
            if (shield > 0)
            {
                shield--;
                return;
            }
            
        }

        protected void OnShieldChange(int oldValue, int newValue)
        {
            shield = newValue;
            //shieldSlider.value = shield;
        }

        #endregion

        #region Dying, Respawning, and Game Ending

        [Server]
        public void GetKilled (HumanPlayer killingPlayer)
        {
            //the game is already over so don't do anything
            if (GameManager.GetInstance().IsGameOver()) return;

            GameManager.GetInstance().AddScore(ScoreType.Kill, killingPlayer.teamIndex);
            //the maximum score has been reached now
            if (GameManager.GetInstance().IsGameOver())
            {
                //tell all clients the winning team
                RpcGameOver(killingPlayer.teamIndex);
                return;
            }

            //the game is not over yet, reset runtime values
            //also tell all clients to despawn this player
            //health = maxHealth;

            //SHIELD HERE?????

            short senderId = (short)killingPlayer.gameObject.GetComponent<NetworkIdentity>().netId;

            RpcRespawn(senderId);
        }

        //called on all clients on both player death and respawn
        //only difference is that on respawn, the client sends the request
        [ClientRpc]
        protected virtual void RpcRespawn(short senderId)
        {
            //toggle visibility for player gameobject (on/off)
            gameObject.SetActive(!gameObject.activeInHierarchy);
            bool isActive = gameObject.activeInHierarchy;
            killedBy = null;

            //the player has been killed
            if (!isActive)
            {
                //find original sender game object (killedBy)
                GameObject senderObj = null;
                if (senderId > 0 && NetworkIdentity.spawned.ContainsKey((uint)senderId))
                {
                    senderObj = NetworkIdentity.spawned[(uint)senderId].gameObject;
                    if (senderObj != null) killedBy = senderObj;
                }

                //detect whether the current user was responsible for the kill, but not for suicide
                //yes, that's my kill: increase local kill counter
                if (this != GameManager.GetInstance().localPlayer && killedBy == GameManager.GetInstance().localPlayer.gameObject)
                {
                    GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
                    GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
                }
            }

            if (isServer)
            {
                //send player back to the team area, this will get overwritten by the exact position from the client itself later on
                //we just do this to avoid players "popping up" from the position they died and then teleporting to the team area instantly
                //this is manipulating the internal PhotonTransformView cache to update the networkPosition variable
                transform.position = GameManager.GetInstance().GetSpawnPosition(teamIndex);
            }

            //further changes only affect the local client
            if (!isLocalPlayer)
                return;

            //local player got respawned so reset states
            if (isActive == true)
                ResetPosition();
            else
            {
                //local player was killed, set camera to follow the killer
                if (killedBy != null) mainCamera.GetComponent<FollowTarget>().target = killedBy.transform;

                //display respawn window (only for local player)
                GameManager.GetInstance().DisplayDeath();
            }
        }

        /// <summary>
        /// Command telling the server that this client is ready for respawn.
        /// This is when the respawn delay is over or a video ad has been watched.
        /// </summary>
        [Command]
        public void CmdRespawn()
        {
            RpcRespawn((short)0);
        }

        /// <summary>
        /// Repositions in team area and resets camera & input variables.
        /// This should only be called for the local player.
        /// </summary>
        public void ResetPosition()
        {
            //start following the local player again
            mainCamera.GetComponent<FollowTarget>().target = gameObject.transform;

            //get team area and reposition it there
            transform.position = GameManager.GetInstance().GetSpawnPosition(teamIndex);
        }

        /// <summary>
        /// Called on all clients on game end providing the winning team.
        /// This is when a target kill count was achieved.
        /// </summary>
        [ClientRpc]
        public void RpcGameOver(int teamIndex)
        {
            Debug.Log("Game Over");
            //display game over window
            GameManager.GetInstance().DisplayGameOver(teamIndex);
        }

        #endregion

        #region Sending Attacks to Server and Back

        [Command]
        private void CmdLongShieldAttack(Vector3 mousePos)
        {
            takingAction = true;
            attackingState = AttackingState.longShieldAttack;
            RotateToPosition(mousePos);
        }
        [ClientCallback]
        private void ClientLongShieldAttack(Vector3 mousePos)
        {
            takingAction = true;
            RotateToPosition(mousePos);
            EnterAnimationState(AnimationState.longShieldAttack);
            attackingState = AttackingState.longShieldAttack;

            CmdLongShieldAttack(mousePos);
        }

        [Command]
        private void CmdShieldAttack(Vector3 mousePos)
        {
            takingAction = true;
            attackingState = AttackingState.shieldAttack;
            RotateToPosition(mousePos);
        }
        [ClientCallback]
        private void ClientShieldAttack(Vector3 mousePos)
        {
            takingAction = true;
            RotateToPosition(mousePos);
            EnterAnimationState(AnimationState.shieldAttack);
            attackingState = AttackingState.shieldAttack;

            CmdShieldAttack(mousePos);
        }

        [Command]
        private void CmdLongNormalAttack(Vector3 mousePos)
        {
            takingAction = true;
            attackingState = AttackingState.longNormalAttack;
            RotateToPosition(mousePos);
        }
        [ClientCallback]
        private void ClientLongNormalAttack(Vector3 mousePos)
        {
            takingAction = true;
            RotateToPosition(mousePos);
            EnterAnimationState(AnimationState.longNormalAttack);
            attackingState = AttackingState.longNormalAttack;

            CmdLongNormalAttack(mousePos);
        }

        [Command]
        private void CmdNormalAttack(Vector3 mousePos)
        {
            takingAction = true;
            attackingState = AttackingState.normalAttack;
            RotateToPosition(mousePos);
        }
        [ClientCallback]
        private void ClientNormalAttack(Vector3 mousePos)
        {
            takingAction = true;
            RotateToPosition(mousePos);
            EnterAnimationState(AnimationState.normalAttack);
            attackingState = AttackingState.normalAttack;

            CmdNormalAttack(mousePos);
        }

        [Command]
        private void CmdThrowSpear(Vector3 mousePos)
        {
            takingAction = true;
            attackingState = AttackingState.throwingSpear;
            RotateToPosition(mousePos);
        }
        [ClientCallback]
        private void ClientThrowSpear(Vector3 mousePos)
        {
            takingAction = true;
            RotateToPosition(mousePos);
            EnterAnimationState(AnimationState.throwingSpear);
            attackingState = AttackingState.throwingSpear;

            CmdThrowSpear(mousePos);
        }

        #endregion

        #region Rolling

        private void StartRoll(Vector3 destination)
        {
            PreSetMovementState(MovementState.rolling);
            rollDirection = (destination - gameObject.transform.position).normalized;
            endRollTime = Time.time + rollTime;

            // if I implement this in the future, need to make sure that this is networked. Right now this is all happening client side.
            //takingAction = true;
        }
        private void ContinueRoll()
        {
            if (Time.time > endRollTime)
            {
                EndRoll();
            }
            else
            {
                gameObject.transform.Translate(rollSpeed * rollDirection * Time.deltaTime);
            }
        }
        private void EndRoll()
        {
            PreSetMovementState(MovementState.walking);
            endRollCoolDownTime = Time.time + rollCoolDownTime;

            // if I implement this in the future, need to make sure that this is networked. Right now this is all happening client side.
            //takingAction = false;
        }

        #endregion

        #region Throwing Spear

        [Command]
        private void CmdSpearStart()
        {
            LoadSpear();
            RpcLoadSpear();
        }

        [ClientRpc]
        private void RpcLoadSpear()
        {
            LoadSpear();
        }
        
        public void LoadSpear()
        {
            holdingSpear = true;
            foreach (Transform spear in hand)
            {
                Destroy(spear.gameObject);
            }
            spearClone = Instantiate(spearPrefab);
            spearClone.GetComponent<Spear>().spearState = Spear.SpearState.held;
            spearClone.transform.SetParent(hand);
            spearClone.transform.localPosition = new Vector3(0, 0, 0);
            spearClone.transform.localRotation = new Quaternion(0, 0, 0,0);

            spearClone.GetComponent<Spear>().myPlayer = this;
        }

        [Command]
        public void CmdReleaseSpear()
        {
            holdingSpear = false;
            foreach (Transform s in hand)
            {
                Destroy(s.gameObject);
            }
            networkedSpearClone = Instantiate(networkedSpearPrefab);
            networkedSpearClone.transform.position = hand.position;
            networkedSpearClone.transform.rotation = hand.rotation;
            networkedSpearClone.GetComponent<NetworkedSpear>().spearState = NetworkedSpear.SpearState.flying;
            networkedSpearClone.GetComponent<NetworkedSpear>().StartFlying();
            networkedSpearClone.GetComponent<Spear>().myPlayer = this;

            NetworkServer.Spawn(networkedSpearClone);

            Debug.Log("Networked spear clone.player = " + networkedSpearClone.GetComponent<Spear>().myPlayer);
            
            networkedSpearClone = null;

            RpcReleaseSpear();

            LoadSpear();
            RpcLoadSpear();
        }

        [ClientRpc]
        public void RpcReleaseSpear()
        {
            foreach (Transform spear in hand)
            {
                Destroy(spear.gameObject);
            }
            spearClone = null;
        }

        #endregion

        #region Animation Test
        void SetOrKeepState(AnimationState a_state)
        {
            if (this.animationState == a_state) return;
            EnterAnimationState(a_state);
        }

        void ExitState()
        {
        }

        void EnterAnimationState(AnimationState state)
        {
            //ExitState();
            switch (state)
            {
                case AnimationState.normalAttack:
                    animator.Play(normalAttack);
                    break;
                case AnimationState.longNormalAttack:
                    animator.Play(longNormalAttack);
                    break;
                case AnimationState.shieldAttack:
                    animator.Play(shieldAttack);
                    break;
                case AnimationState.longShieldAttack:
                    animator.Play(longShieldAttack);
                    break;
                case AnimationState.throwingSpear:
                    animator.Play(throwingSpear);
                    break;
                case AnimationState.block:
                    animator.Play(block);
                    break;
                case AnimationState.idle:
                    animator.Play(idle);
                    break;
            }
            this.animationState = state;
        }

        void DetermineAnim()
        {
            if (!takingAction)
            {
                if (movementState == MovementState.blocking) SetOrKeepState(AnimationState.block);
                else SetOrKeepState(AnimationState.idle);
            }
        }

        public void EndAttack ()
        {
            if(!Input.GetKey(KeyCode.Mouse1) && isLocalPlayer) RotateToMouse();
            attackingState = AttackingState.notAttacking;
            takingAction = false;
        }

        #endregion
    }
}


