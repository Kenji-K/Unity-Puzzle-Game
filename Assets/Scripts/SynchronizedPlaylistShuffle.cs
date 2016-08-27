using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SynchronizedPlaylistShuffle : MonoBehaviour {
	public PlaylistController mainPlaylistController;
	public float timeUntilNextSong;
	public int timeSamples;

	void Awake() {
	}

	// Use this for initialization
	void Start () {
		if (mainPlaylistController == null) {
			mainPlaylistController = GetComponent<PlaylistController>();
		}
		mainPlaylistController.SongChanged += OnSongChanged;
	}
	
	// Update is called once per frame
	void Update () {
		timeSamples = mainPlaylistController.ActiveAudioSource.timeSamples;
	}

	void OnSongChanged(string newSongName) {
		var simpleSongController = PlaylistController.InstanceByName("PC Simple Music");
		simpleSongController.TriggerPlaylistClip(newSongName.Replace("multiver", "nomultiver"));
		simpleSongController.ActiveAudioSource.timeSamples = mainPlaylistController.ActiveAudioSource.timeSamples;
	}
}
