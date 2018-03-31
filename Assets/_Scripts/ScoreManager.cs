using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {
	static ScoreManager instance;

	public ScoreManager Instance { 
		get { return instance; }
	}
	public RunningCharacter gameController;
	public GUIText scoreText;

	private int score;
	private float timerScore;
	private float maxDistance;
	// Use this for initialization
	void Awake () {
		if (instance == null) {
			instance = this;
		} else {
			Destroy (gameObject);
		}
	}

	void Start ()
	{
		maxDistance = gameController.maxDistance;
		print(maxDistance);
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
