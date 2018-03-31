using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	public static SoundManager instance;

	public SoundManager Instance { 
		get { return instance; }
	}
	// Use this for initialization
	public AudioClip musicLoop1;
	public AudioClip musicLoop2;

	public AudioClip zoom;
	public AudioClip slowdown;

	public AudioClip yeah;
	public AudioClip lose;
	public AudioClip win;
	public AudioClip success;

	public AudioSource effects;
	public AudioSource gameMusic;
	public AudioSource playerSource;
	// public AudioClip

	// Use this for initialization
	void Awake () {
		if (instance == null) {
			instance = this;
		} else {
			Destroy (gameObject);
		}
		gameMusic.clip = musicLoop1;
	}
	void Start () {
		gameMusic.Play ();
	}

	public void PlayLose () {
		ChangeSounds (lose, playerSource);
	}

	public void PlayWin() {
		ChangeSounds (win, playerSource);
	}

	public void PlayZoom () {
		 ChangeSounds (zoom, effects);
	}

	public void PlaySuccess () {
		ChangeSounds (success, playerSource);
	}

	public void PlaySlowdown () {
		ChangeSounds (slowdown, effects);
	}

	void ChangeSounds(AudioClip sound, AudioSource player){
		if (!player.isPlaying) {
			player.clip = sound;
			player.Play ();
		}
	}
}
