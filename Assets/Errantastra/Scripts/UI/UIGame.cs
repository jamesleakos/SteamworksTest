
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;


namespace Errantastra
{
    /// <summary>
    /// UI script for all elements, team events and user interactions in the game scene.
    /// </summary>
    public class UIGame : MonoBehaviour
    {

        public GameObject teamScoreBoxPrefab;

        public GameObject scoreboardArea;

        /// <summary>
        /// UI text for indicating player death and who killed this player.
        /// </summary>
        public Text deathText;
        
        /// <summary>
        /// UI text displaying the time in seconds left until player respawn.
        /// </summary>
        public Text spawnDelayText;

        /// <summary>
        /// UI text for indicating game end and which team has won the round.
        /// </summary>
        public Text gameOverText;

        /// <summary>
        /// UI window gameobject activated on game end, offering sharing and restart buttons.
        /// </summary>
        public GameObject gameOverMenu;


        //initialize variables
        IEnumerator Start()
        {
			//wait until the network is ready
            while (GameManager.GetInstance() == null || GameManager.GetInstance().localPlayer == null)
                yield return null;
        }

        public void UpdatePlayerUI ()
        {
            Debug.Log("Update Player UI called");
            var teams = GameManager.GetInstance().teams;
            while (teams.Count > scoreboardArea.GetComponentsInChildren<ScoreBox>().ToList().Count)
            {
                AddTeamBox("Temp", 0);
            }
            while (teams.Count > scoreboardArea.GetComponentsInChildren<ScoreBox>().ToList().Count)
            {
                Destroy(scoreboardArea.GetComponentsInChildren<ScoreBox>().ToList()[0].gameObject);
            }

            for (int i = 0; i < scoreboardArea.GetComponentsInChildren<ScoreBox>().ToList().Count; i++)
            {
                var box = scoreboardArea.GetComponentsInChildren<ScoreBox>().ToList()[i];
                box.SetName(teams[i].name);
                box.SetScore(teams[i].score);
            }
        }

        public void AddTeamBox (string teamName, int score)
        {
            var go = Instantiate(teamScoreBoxPrefab) as GameObject;
            go.transform.SetParent(scoreboardArea.transform, false);
            go.GetComponent<ScoreBox>().SetName(teamName);
            go.GetComponent<ScoreBox>().SetScore(score);
        }
        
        public void OnTeamScoreChanged(int index, int newValue)
        {
            
        }
      
        public void SetDeathText(string playerName, Team team)
        {
            //show killer name and colorize the name converting its team color to an HTML RGB hex value for UI markup
            deathText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">" + playerName + "</color>";
        }

        public void SetSpawnDelay(float time)
        {                
            spawnDelayText.text = Mathf.Ceil(time) + "";
        }
        
        
        /// <summary>
        /// Hides any UI components related to player death after respawn.
        /// </summary>
        public void DisableDeath()
        {
            //clear text component values
            deathText.text = string.Empty;
            spawnDelayText.text = string.Empty;
        }


        /// <summary>
        /// Set game end text and display winning team in its team color.
        /// </summary>
        public void SetGameOverText(Team team)
        {
            //show winning team and colorize it by converting the team color to an HTML RGB hex value for UI markup
            gameOverText.text = "TEAM <color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">" + team.name + "</color> WINS!";
        }


        /// <summary>
        /// Displays the game's end screen. Called by GameManager after few seconds delay.
        /// Tries to display a video ad, if not shown already.
        /// </summary>
        public void ShowGameOver()
        {        
            //hide text but enable game over window
            gameOverText.gameObject.SetActive(false);
            gameOverMenu.SetActive(true);
        }


        /// <summary>
        /// Returns to the starting scene and immediately requests another game session.
        /// In the starting scene we have the loading screen and disconnect handling set up already,
        /// so this saves us additional work of doing the same logic twice in the game scene. The
        /// restart request is implemented in another gameobject that lives throughout scene changes.
        /// </summary>
        public void Restart()
        {
            GameObject gObj = new GameObject("RestartNow");
            //gObj.AddComponent<UIRestartButton>();
            DontDestroyOnLoad(gObj);
            
            Quit();
        }


        /// <summary>
        /// Stops receiving further network updates by hard disconnecting, then load starting scene.
        /// </summary>
        public void Disconnect()
        {
            Mirror.NetworkManager.singleton.StopHost();
            Quit();
        }


        /// <summary>
        /// Loads the starting scene. Disconnecting already happened when presenting the GameOver screen.
        /// </summary>
        public void Quit()
        {	
			SceneManager.LoadScene(0);
        }
    }
}
