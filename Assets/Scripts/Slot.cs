using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Slot : MonoBehaviour {
    public HintIndicator hintIndicator;

    public Block HeldBlock { get; set; }
    public IntVector2 GridPosition { get; set; }
    public static Slot currentHoveredSlot = null;
    public bool Cleared { get; set; }

    private GameObject slotBackground;
    private float alphaValue;

    void Awake() {
        HeldBlock = null;
        slotBackground = transform.GetChild(0).gameObject;
        alphaValue = 0f;
    }

	// Use this for initialization
	void Start () {
        iTween.Init(slotBackground);
        iTween.Init(gameObject);
        var delay = (GridPosition.x + GridPosition.y) / (float) (Board.Instance.TotalColumns + Board.Instance.TotalRows - 2);

        iTween.ColorTo(slotBackground,
            iTween.Hash("r", 249f / 255, "g", 200f / 255, "b", 49f / 255,
                        "time", 1f,
                        "looptype", "pingPong",
                        "delay", delay));
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void OnMouseOver() {
        Slot.currentHoveredSlot = this;
    }

    public void GlowStart() {
        if (!Cleared) { 
            Cleared = true;
            iTween.StopByName(gameObject, "slotBackgroundFadeOut");
            iTween.ValueTo(gameObject, iTween.Hash(
                "name", "slotBackgroundFadeIn",
                "from", alphaValue,
                "to", 0.7f,
                "time", 1f,
                "onupdate", "TweenAlpha"));
        }
    }

    public void GlowEnd() {
        if (Cleared) { 
            Cleared = false;
            iTween.StopByName(gameObject, "slotBackgroundFadeIn");
            iTween.ValueTo(gameObject, iTween.Hash(
                "name", "slotBackgroundFadeOut",
                "from", alphaValue,
                "to", 0f,
                "time", 1f,
                "onupdate", "TweenAlpha"));
        }
    }

    public void TweenAlpha(float alpha) {
        alphaValue = alpha;
        var sprite = slotBackground.GetComponent<tk2dSprite>();
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alphaValue);
    }
}
