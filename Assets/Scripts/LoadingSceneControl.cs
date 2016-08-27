using UnityEngine;
using System.Collections;
using I2.Loc;

public class LoadingSceneControl : MonoBehaviour {

	// Use this for initialization
    void Start() {
        if (PlayerPrefs.HasKey("Language")) {
            LocalizationManager.CurrentLanguage = PlayerPrefs.GetString("Language");
        }
        Application.LoadLevel("MainMenuScene");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
