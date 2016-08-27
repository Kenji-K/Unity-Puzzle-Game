using UnityEngine;
using System.Collections;
using MoreLinq;

public class PauseMenuController : MonoBehaviour {
    public dfPanel menuPanel;
    public dfSprite pauseIndicator;
    public dfButton pauseButton;
    public dfProgressBar SFXVolume;
    public dfProgressBar BGMVolume;
    public dfLabel SFXPercentage;
    public dfLabel BGMPercentage;

	// Use this for initialization
	void Start () {
        menuPanel.Opacity = 0;
        menuPanel.IsVisible = true;
        SFXVolume.Value = PersistentUtility.Instance.EffectsVolume;
        BGMVolume.Value = PersistentUtility.Instance.MusicVolume;
        SFXPercentage.Text = ((int)(PersistentUtility.Instance.EffectsVolume * 100)).ToString() + "%";
        BGMPercentage.Text = ((int)(PersistentUtility.Instance.MusicVolume * 100)).ToString() + "%";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Appear() {
        var tweenAlpha = menuPanel.GetComponent<dfTweenFloat>();
        tweenAlpha.StartValue = 0;
        tweenAlpha.EndValue = 1;
        tweenAlpha.Play();

        menuPanel.IsInteractive = true;
        GameControl.Instance.Paused = true;
    }

    public void Disappear() {
        var tweenAlpha = menuPanel.GetComponent<dfTweenFloat>();
        tweenAlpha.StartValue = 1;
        tweenAlpha.EndValue = 0;
        tweenAlpha.Play();

        menuPanel.IsInteractive = false;
        GameControl.Instance.Paused = false;
        PlayerPrefs.Save();
    }

    public void Toggle() {
        var tweenAlpha = menuPanel.GetComponent<dfTweenFloat>();
        //Debug.Log("Toggled!");
        //Debug.Log("tweenAlpha.IsPlaying: " + tweenAlpha.IsPlaying);
        //Debug.Log("GameControl.Instance.GameStarted: " + GameControl.Instance.GameStarted);
        //Debug.Log("GameControl.Instance.GameOver: " + GameControl.Instance.GameOver);
        //Debug.Log("CameraFade.Fading: " + CameraFade.Fading);
        //Debug.Log("menuPanel.IsInteractive: " + menuPanel.IsInteractive);

        if (!tweenAlpha.IsPlaying && GameControl.Instance.GameStarted && !GameControl.Instance.GameOver && !CameraFade.Fading) {
            if (menuPanel.IsInteractive) {
                GameObject[] previewPieces = GameObject.FindGameObjectsWithTag("PreviewPiece");
                foreach (var previewPiece in previewPieces) {
                    previewPiece.GetComponentInChildren<MeshRenderer>().enabled = true;
                }

                GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
                foreach (var block in blocks) {
                    block.GetComponentInChildren<MeshRenderer>().enabled = true;
                }

                GameObject[] complicationPreviews = GameObject.FindGameObjectsWithTag("ComplicationPreview");
                foreach (var complicationPreview in complicationPreviews) {
                    complicationPreview.GetComponentsInChildren<MeshRenderer>().ForEach(x => x.enabled = true);
                }

                GameUIController.Instance.lifeCounter.IsVisible = true;
                GameUIController.Instance.nextIndicator.IsVisible = true;
                if (GameControl.Instance.CurrentGameMode.ModeName != "PeriodicMode") { 
                    GameUIController.Instance.nextMutationIndicator.IsVisible = true;
                }

                Time.timeScale = 1;

                Disappear();
            } else {
                GameObject[] previewPieces = GameObject.FindGameObjectsWithTag("PreviewPiece");
                foreach (var previewPiece in previewPieces) {
                    previewPiece.GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
                foreach (var block in blocks) {
                    block.GetComponentInChildren<MeshRenderer>().enabled = false;
                }

                GameObject[] complicationPreviews = GameObject.FindGameObjectsWithTag("ComplicationPreview");
                foreach (var complicationPreview in complicationPreviews) {
                    complicationPreview.GetComponentsInChildren<MeshRenderer>().ForEach(x => x.enabled = false);
                }

                GameUIController.Instance.lifeCounter.IsVisible = false;
                GameUIController.Instance.nextIndicator.IsVisible = false;
                if (GameControl.Instance.CurrentGameMode.ModeName != "PeriodicMode") {
                    GameUIController.Instance.nextMutationIndicator.IsVisible = false;
                }

                Time.timeScale = 0;

                Appear();
            }
        }

    }

    public void OnSFXVolumeChanged(dfControl control, System.Single value) {
        PersistentUtility.Instance.EffectsVolume = value;
        SFXPercentage.Text = ((int)(PersistentUtility.Instance.EffectsVolume * 100)).ToString() + "%";
        //MasterAudio.GrabBusByName("Sound Effects").volume = value;
    }

    public void OnMusicVolumeChanged(dfControl control, System.Single value) {
        PersistentUtility.Instance.MusicVolume = value;
        BGMPercentage.Text = ((int)(PersistentUtility.Instance.MusicVolume * 100)).ToString() + "%";
        //MasterAudio.PlaylistMasterVolume = value;
    }

    public void GoToMainMenu(dfControl control, dfMouseEventArgs mouseEvent) {
        PersistentUtility.Instance.GoToMainMenu();
    }

    public void RestartLevel(dfControl control, dfMouseEventArgs mouseEvent) {
        PersistentUtility.Instance.RestartScene();
    }
}
