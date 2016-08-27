using UnityEngine;
using System.Collections;
using Assets.Scripts.Util;

public class HintIndicator : MonoBehaviour {
    tk2dSprite sprite;
	// Use this for initialization
	void Start () {
        iTween.Init(gameObject);
        sprite = GetComponent<tk2dSprite>();
        string modeName = GameControl.Instance.CurrentGameMode.ModeName == "TutorialMode" ? "ClassicMode" : GameControl.Instance.CurrentGameMode.ModeName;
        sprite.color = GlobalData.Instance.TransparentColors[modeName];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartTween() {
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
        iTween.ScaleTo(gameObject, 
            iTween.Hash("name", "HintScale",
                        "scale", new Vector3(1.3f, 1.3f),
                        "easetype", "easeOutCubic",
                        "time", 0.3f,
                        "looptype", "pingPong"));
         
    }

    public void StopTween() {
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
        
        iTween.Stop(this.gameObject);
    }
}
