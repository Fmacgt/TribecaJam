using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour 
{
	public RunningCharacter gameController;
	public Text scoreText;

	//==============================================================================

	private int score;
	private float timerScore;
	private float maxDistance;

	/////////////////////////////////////////////////////////////////////////////////////

	void Start ()
	{
		maxDistance = gameController.maxDistance;
//		print(maxDistance);
		score = 0;
		// UpdateScore ();
	}

	public void FinalScore(float score) {
		// display final score.
	}
	
	// Update is called once per frame
	void Update () {
		timerScore = gameController.GetTimer();


		if (Input.GetKeyDown("space")) {
			print("The score is " + timerScore);
		}
	}
}
