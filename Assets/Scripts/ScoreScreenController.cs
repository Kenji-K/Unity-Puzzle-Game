using UnityEngine;
using System.Collections;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;

public class ScoreScreenController : MonoBehaviour {
    public dfLabel level;
    public dfLabel moves;
    public dfLabel blocksCleared;
    public dfLabel squaresErased;
    public dfLabel longestChain;
    public dfLabel biggestCombo;
    public dfLabel hintsUsed;
    public dfLabel timePlayed;
    public dfLabel score;
    public dfLabel bestScore;
    public dfSprite gameType;
    public List<dfSprite> backgrounds;
    public List<dfButton> buttons;
    public Leaderboard leaderboard;

	// Use this for initialization
	void Start () {
        var modeName = GlobalData.Instance.GameMode;

        var gameStats = GlobalData.Instance.GameStats;
        level.Text = gameStats.Get<int>("levelReached").ToString();
        moves.Text = gameStats.Get<int>("piecesPlaced").ToString();
        blocksCleared.Text = gameStats.Get<int>("blocksCleared").ToString();
        squaresErased.Text = gameStats.Get<int>("squaresCleared").ToString();
        longestChain.Text = gameStats.Get<int>("maxChain").ToString();
        biggestCombo.Text = gameStats.Get<int>("maxCombo").ToString();
        hintsUsed.Text = gameStats.Get<int>("hintsUsed").ToString();
        score.Text = gameStats.Get<int>("score").ToString();
        bestScore.Text = GlobalData.Instance.highScores[modeName].ToString();
        leaderboard.OnModeSelected(modeName);

        if (modeName == "PeriodicMode") level.IsVisible = false;

        var totalSeconds = gameStats.Get<int>("timePlayed");
        var secondsPlayed = totalSeconds % 60;
        var minutesPlayed = totalSeconds / 60;
        //var timeSpan = new TimeSpan(0, 0, totalSeconds);
        timePlayed.Text = String.Format("{0:##00}:{1:00}", minutesPlayed, secondsPlayed);

        string temporaryModeName = string.Empty;
        if (modeName == "ClassicMode") {
            temporaryModeName = "NormalMode"; //KLUDGE: los nombres de los sprites dicen Normal en lugar de Classic
        } else {
            temporaryModeName = modeName;
        }
        string strippedString = temporaryModeName.Replace("Mode", string.Empty);
        gameType.SpriteName = strippedString + "Level";

        var labels = FindObjectsOfType<dfLabel>();

        foreach (var label in labels) {
            if (label.Color.r == 14 && label.Color.g == 103 && label.Color.b == 163) {
                label.Color = GlobalData.Instance.SolidColors[modeName];
            }
        }

        foreach (var background in backgrounds) {
            background.Color = GlobalData.Instance.SolidColors[modeName];
        }

        foreach (var button in buttons) {
            button.NormalBackgroundColor = GlobalData.Instance.SolidColors[modeName];
            button.HoverBackgroundColor = GlobalData.Instance.SolidColors[modeName];
            button.FocusBackgroundColor = GlobalData.Instance.SolidColors[modeName];
            button.DisabledColor = GlobalData.Instance.SolidColors[modeName];
            button.PressedBackgroundColor = GlobalData.Instance.SolidColors[modeName];
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPlayAgainClick(dfControl control, dfMouseEventArgs mouseEvent) {
        foreach (var playlistController in PlaylistController.Instances) {
            var localPlaylist = playlistController;
            localPlaylist.FadeToVolume(0f, 1f, () => {
                localPlaylist.StopPlaylist();
                //Debug.Log("Playlist " + localPlaylist.PlaylistName + " has been stopped.");
                localPlaylist.PlaylistVolume = 1f;
            });
        }
        CameraFade.StartAlphaFade(Color.white, false, 1f, 0f, () => { PersistentUtility.Instance.GoToScene("GameScene"); /*Debug.Log("Transitioning scenes.");*/ });
    }

    public void OnMainMenuClick(dfControl control, dfMouseEventArgs mouseEvent) {
        PersistentUtility.Instance.GoToMainMenu();
    }

}
