using UnityEngine;
using System.Collections;
using Assets.Scripts.Util;
using System.Linq;

public class GameUIController : MonoBehaviour {
    private static GameUIController instance;
    public static GameUIController Instance {
        get {
            return instance;
        }
    }

    public dfPanel gameInfo;
    public dfSlicedSprite nextIndicator;
    public dfSlicedSprite nextMutationIndicator;
    public dfButton pauseButton;
    public dfLabel score;
    public dfPanel scorePanel;
    public dfLabel targetScoreLabel;
    public dfLabel targetPlayerLabel;
    public dfPanel lifeCounter;
    public dfPanel gameOverMessage;
    public dfPanel noMoreMovesMessage;

    public dfTweenGroup NoMoreMovesTweenAlone { get; set; }

    void Awake() {
        GameUIController.instance = this;
        NoMoreMovesTweenAlone = noMoreMovesMessage.GetComponents<dfTweenGroup>().Single(tg => tg.TweenName == "TweenAlone");
    }

	// Use this for initialization
	void Start () {
        gameInfo.GetComponent<LevelProgressIndicator>().levelLabel.Text = "1";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ChangeImages(string modeName) {
        if (modeName == "ClassicMode") modeName = "NormalMode"; //KLUDGE: los nombres de los sprites dicen Normal en lugar de Classic
        string strippedString = modeName.Replace("Mode", string.Empty);

        gameInfo.GetComponent<LevelProgressIndicator>().backgroundSprite.SpriteName = strippedString + "Level";
        //nextIndicator.SpriteName = strippedString + "Next";
        //pauseButton.BackgroundSprite = strippedString + "Pause";

        nextIndicator.Color = GlobalData.Instance.SolidColors[modeName];
        nextMutationIndicator.Color = GlobalData.Instance.SolidColors[modeName];
        gameOverMessage.BackgroundColor = GlobalData.Instance.SolidColors[modeName];
        noMoreMovesMessage.BackgroundColor = GlobalData.Instance.SolidColors[modeName];
        scorePanel.BackgroundColor = GlobalData.Instance.SolidColors[modeName];

        if (modeName == "PeriodicMode") {
            nextMutationIndicator.IsVisible = false;
            nextIndicator.Position = new Vector3(0, 0);
        }
    }
}
