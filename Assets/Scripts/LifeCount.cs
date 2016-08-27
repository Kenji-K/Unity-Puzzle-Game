using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util;

public class LifeCount : MonoBehaviour {
    public List<dfSprite> slots;
    private int totalLives;

    public int Count {
        get { 
            return totalLives; 
        }
        set {
            if (value < totalLives) {
                //SHAKE IT
                iTween.ShakePosition(gameObject, iTween.Hash(
                    "delay", 0.75f, 
                    "amount", new Vector3(0.02f, 0.02f),
                    "time", 0.5f));
            }

            totalLives = value;
            for (int i = 0; i < Mathf.Clamp(totalLives, 0, 10); i++) {
                var slot = slots[i];
                var currentColor = GlobalData.Instance.SolidColors[GameControl.Instance.CurrentGameMode.ModeName];
                slot.Opacity = 1f;
            }

            for (int i = totalLives; i < 10; i++) {
                var slot = slots[i];
                var currentColor = GlobalData.Instance.SolidColors[GameControl.Instance.CurrentGameMode.ModeName];
                slot.Color = currentColor;
                slot.Opacity = 0.5f;
            }

            if (totalLives > 10) {
                slots[0].SpriteName = "UILives" + (totalLives % 10).ToString();
                slots[1].SpriteName = "UILives" + (totalLives / 10).ToString();
            } else {
                slots[0].SpriteName = "UILivesSquare";
                slots[1].SpriteName = "UILivesSquare";
            }
        }
    }

	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(0.75f, 0.75f, 1);
	}
}
