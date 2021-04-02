using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Errantastra
{
    /// <summary>
    /// Item to add to scoreboard to keep score
    /// </summary>
    public class ScoreBox : MonoBehaviour
    {
        public string teamName;
        public int score;

        public TMP_Text teamNameText;
        public TMP_Text scoreText;

        public void SetScore (int score)
        {
            this.score = score;
            scoreText.text = score.ToString();
        }

        public void SetName (string name)
        {
            teamName = name;
            teamNameText.text = teamName;
        }
    }
}

