
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace Errantastra
{
    /// <summary>
    /// Manages game workflow and provides high-level access to networked logic during a game.
    /// It manages functions such as team fill, scores and ending a game, but also video ad results.
    /// </summary>
	public class GameManager : NetworkBehaviour
    {   
        //reference to this script instance
        private static GameManager instance;
        
        /// <summary>
        /// The local player instance spawned for this client.
        /// </summary>
        [HideInInspector]
        public HumanPlayer localPlayer;

        /// <summary>
        /// Active game mode played in the current scene.
        /// </summary>
        public GameMode gameMode = GameMode.TDM;

        /// <summary>
        /// Reference to the UI script displaying game stats.
        /// </summary>
        public UIGame ui;
        
        public SyncList<Team> teams = new SyncList<Team>();

        public List<Transform> spawns = new List<Transform>();

        /// <summary>
        /// The maximum amount of kills to reach before ending the game.
        /// </summary>
        public int maxScore = 30;

        /// <summary>
        /// The delay in seconds before respawning a player after it got killed.
        /// </summary>
        public int respawnTime = 5;

        /// <summary>
        /// Enable or disable friendly fire. This is verified in the Bullet script on collision.
        /// </summary>
        public bool friendlyFire = false;


        //initialize variables
        void Awake()
        {
            instance = this;
        }
        

        /// <summary>
        /// Returns a reference to this script instance.
        /// </summary>
        public static GameManager GetInstance()
        {
            return instance;
        }


        /// <summary>
        /// Global check whether this client is the match master or not.
        /// </summary>
		public static bool isMaster()
		{
			return GetInstance().isServer;
		}
        

        /// <summary>
        /// Server only: initialize SyncList length with team size once.
        /// Also verifies the team fill for each team in case of host migration.
        /// </summary>
        public override void OnStartServer()
        {

        }


        /// <summary>
        /// On establishing a successful connection to the master and initializing client variables,
        /// update the game UI, i.e. team fill and scores, by looping over the received SyncLists.
        /// </summary>
        public override void OnStartClient()
        {        
            //double check whether the game joined is still running, otherwise return to the menu scene
            //this could happen when a connection has been made right before the game ended and stopped
            if(IsGameOver())
            {
                ui.Quit();
                return;
            }

            Debug.Log("GameManageer.OnStartClient: There are " + teams.Count.ToString() + " teams.");
        }
        
        void Start()
        {
        }     
        
        /// <summary>
        /// Returns a random spawn position within the team's spawn area.
        /// </summary>
        public Transform GetSpawnPosition()
        {
            float furthestDistance = 0;
            Transform furthestSpawn = spawns[0];

            var players = GameObject.FindObjectsOfType<Player>();
            if (players.Length == 0) return furthestSpawn;

            foreach (var spawn in spawns)
            {
                float closestPlayer = Mathf.Infinity;
                foreach (var player in players)
                {
                    var distance = (player.gameObject.transform.position - spawn.position).magnitude;
                    if (distance < closestPlayer) closestPlayer = distance;
                }

                if (closestPlayer > furthestDistance)
                {
                    furthestDistance = closestPlayer;
                    furthestSpawn = spawn;
                }
            }

            return furthestSpawn;
        }

        [Server]
        public int CreateTeam()
        {
            Debug.Log("CreateTeam called");
            var newTeam = new Team();
            newTeam.name = "Team " + (teams.Count + 1).ToString();
            newTeam.score = 0;

            teams.Add(newTeam);

            RpcAddTeamOnClient(newTeam.name, newTeam.score);
            RpcUpdatePlayerUI();

            return teams.Count - 1;
        }

        [ClientRpc]
        public void RpcAddTeamOnClient(string name, int score)
        {
            Debug.Log("There are " + teams.Count.ToString() + " teams now.");
            //var newTeam = new Team();
            //newTeam.name = namme;
            //newTeam.score = score;

            //teams.Add(newTeam);
        }

        /// <summary>
        /// Adds points to the target team depending on matching game mode and score type.
        /// This allows us for granting different amount of points on different score actions.
        /// </summary>
        public void AddScore(ScoreType scoreType, int teamIndex)
        {
            //distinguish between game mode
            switch(gameMode)
            {
                //in TDM, we only grant points for killing
                case GameMode.TDM:
                    switch(scoreType)
                    {
                        case ScoreType.Kill:
                            teams[teamIndex].score += 1;
                            Debug.Log("Team " + teamIndex + " has " + teams[teamIndex].score.ToString() + " points.");
                            break;
                    }
                break;

                //in CTF, we grant points for both killing and flag capture
                case GameMode.CTF:
                    switch(scoreType)
                    {
                        case ScoreType.Kill:
                            teams[teamIndex].score += 1;
                            break;

                        case ScoreType.Capture:
                            teams[teamIndex].score += 10;
                            break;
                    }
                break;
            }

            RpcUpdatePlayerUI();
        }

        [Server]
        public void UpdatePlayerUI()
        {
            RpcUpdatePlayerUI();
        }

        [ClientRpc]
        public void RpcUpdatePlayerUI()
        {
            ui.UpdatePlayerUI();
        }

        /// <summary>
        /// Returns whether a team reached the maximum game score.
        /// </summary>
        public bool IsGameOver()
        {
            //init variables
            bool isOver = false;
            
            //loop over teams to find the highest score
            for(int i = 0; i < teams.Count; i++)
            {
                //score is greater or equal to max score,
                //which means the game is finished
                if(teams[i].score >= maxScore)
                {
                    isOver = true;
                    break;
                }
            }
            
            //return the result
            return isOver;
        }
        
        
        /// <summary>
        /// Only for this player: sets the death text stating the killer on death.
        /// If Unity Ads is enabled, tries to show an ad during the respawn delay.
        /// By using the 'skipAd' parameter is it possible to force skipping ads.
        /// </summary>
        public void DisplayDeath(bool skipAd = false)
        {
            //get the player component that killed us
            HumanPlayer other = localPlayer;
            string killedByName = "YOURSELF";
            if(localPlayer.killedBy != null)
                other = localPlayer.killedBy.GetComponent<HumanPlayer>();

            //suicide or regular kill?
            if (other != localPlayer)
            {
                killedByName = other.myName;
            }

            //calculate if we should show a video ad
            #if UNITY_ADS
            if (!skipAd && UnityAdsManager.ShowAd())
                return;
            #endif

            //when no ad is being shown, set the death text
            //and start waiting for the respawn delay immediately
            ui.SetDeathText(killedByName, teams[other.teamIndex]);
            StartCoroutine(SpawnRoutine());
        }
        
        
        //coroutine spawning the player after a respawn delay
        IEnumerator SpawnRoutine()
        {
            Debug.Log("SpawnRoutine");
            //calculate point in time for respawn
            float targetTime = Time.time + respawnTime;
           
            //wait for the respawn to be over,
            //while waiting update the respawn countdown
            while(targetTime - Time.time > 0)
            {
                ui.SetSpawnDelay(targetTime - Time.time);
                yield return null;
            }
            
            //respawn now: send request to the server
            ui.DisableDeath();
            localPlayer.CmdRespawn();
        }
        
        
        /// <summary>
        /// Only for this player: sets game over text stating the winning team.
        /// Disables player movement so no updates are sent through the network.
        /// </summary>
        public void DisplayGameOver(int teamIndex)
        {           
            localPlayer.enabled = false;
            ui.SetGameOverText(teams[teamIndex]);

            //starts coroutine for displaying the game over window
            StopCoroutine(SpawnRoutine());
            StartCoroutine(DisplayGameOver());
        }
        
        
        //displays game over window after short delay
        IEnumerator DisplayGameOver()
        {
            //give the user a chance to read which team won the game
            //before enabling the game over screen
            yield return new WaitForSeconds(3);
            
            //show game over window and disconnect from network
            ui.ShowGameOver();
            NetworkManager.singleton.StopHost();
        }
        
        
        //clean up callbacks on scene switches
        void OnDestroy()
        {
            #if UNITY_ADS
                UnityAdsManager.adResultEvent -= HandleAdResult;
            #endif
        }
    }

    
    /// <summary>
    /// Defines properties of a team.
    /// </summary>
    [System.Serializable]
    public class Team
    {
        /// <summary>
        /// The name of the team shown on game over.
        /// </summary>
        public string name;

        public int score;
    }


    /// <summary>
    /// Defines the types that could grant points to players or teams.
    /// Used in the AddScore() method for filtering.
    /// </summary>
    public enum ScoreType
    {
        Kill,
        Capture
    }


    /// <summary>
    /// Available game modes selected per scene.
    /// Used in the AddScore() method for filtering.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Team Deathmatch
        /// </summary>
        TDM,

        /// <summary>
        /// Capture The Flag
        /// </summary>
        CTF
    }
}
