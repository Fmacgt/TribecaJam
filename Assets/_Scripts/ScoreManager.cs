using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour 
{
	public RunningCharacter gameController;
	public Text scoreText;
	public Text pastHighScore;
	//==============================================================================

	private float timerScore;
	private float maxDistance;
	private float pastMaxScore;
	string highScoreKey = "HighScore";
	/////////////////////////////////////////////////////////////////////////////////////
	// Use this for initialization
	void Start ()
	{
		PlayerPrefs.DeleteAll(); // For Debugging Score
		maxDistance = gameController.maxDistance;
		pastMaxScore = PlayerPrefs.GetFloat(highScoreKey,99999f);
		print("Current Highsore is " + PlayerPrefs.GetFloat(highScoreKey,0f));

		//        print(maxDistance);
		// score = 0;
		// UpdateScore ();
	}

	public void FinalScore(float score) {
		// display final score.
		string truncatedScore = score.ToString("F2");
		pastHighScore.text = "HighScore: " + pastMaxScore.ToString() + "Seconds";
		scoreText.text = truncatedScore + "!";

		print (pastMaxScore + " > "+ score + " is " + (pastMaxScore > score));

		if (pastMaxScore > score) {
			PlayerPrefs.SetFloat(highScoreKey, score);
			PlayerPrefs.Save();
			// Show update
			if (SoundManager.instance) {
				SoundManager.instance.PlayWin ();
			}
			print("I have been saved to high score with... " + PlayerPrefs.GetFloat(highScoreKey,0f));
			pastHighScore.text = "New HighScore: " + truncatedScore;
			pastHighScore.fontSize = 24;
		}
	}

	private float GetFloat(string stringValue, float defaultValue)
	{
		float result = defaultValue;
		float.TryParse(stringValue, out result);
		return result;
	}

	// Update is called once per frame
	void Update () {
		timerScore = gameController.GetTimer();


		if (Input.GetKeyDown("space")) {
			print("The score is " + timerScore);
		}
	}
}