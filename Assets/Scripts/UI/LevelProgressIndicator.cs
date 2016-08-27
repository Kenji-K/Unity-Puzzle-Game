using UnityEngine;
using System.Collections;
using Assets.Scripts.GameModes;

public class LevelProgressIndicator : MonoBehaviour {
    public dfProgressBar progressBar;
    public dfLabel levelLabel;
    public dfLabel incomingLevelLabel;
    public dfSprite backgroundSprite;
    internal GameMode GameMode { get; set; }

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    internal void LevelProgressChangedHandler(int requiredMoves, int progress) {
        //Update the Text
        //int remainingMoves = requiredMoves - progress;
        //textualIndicator.text = "In " + remainingMoves + " moves";

        //Update the bar
        float fillRatio = (float)progress / requiredMoves;
        progressBar.Value = fillRatio;
    }

    internal void LevelChangedHandler(int levelValue) {
        incomingLevelLabel.Text = levelValue.ToString();
        levelLabel.GetComponent<dfTweenVector3>().Play();
        incomingLevelLabel.GetComponent<dfTweenVector3>().Play();
    }

    public void ChangeText(string text) {
        incomingLevelLabel.Text = text;
        levelLabel.GetComponent<dfTweenVector3>().Play();
        incomingLevelLabel.GetComponent<dfTweenVector3>().Play();
    }

    internal void Construct(GameMode gameMode) {
        GameMode = gameMode;
        gameMode.LevelProgressIndicator = this;
        if (gameMode.ModeName != "PeriodicMode") { 
            GameMode.OnLevelProgressChangedSubscribe(LevelProgressChangedHandler);
            GameMode.OnLevelChangedSubscribe(LevelChangedHandler);
        }
    }

    public void RepositionLevelLabels() {
        levelLabel.Position = new Vector3(160, -24);
        levelLabel.Text = incomingLevelLabel.Text;
        incomingLevelLabel.Position = new Vector3(410, -274);
    }
}
