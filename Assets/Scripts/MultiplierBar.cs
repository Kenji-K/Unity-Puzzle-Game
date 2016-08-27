using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplierBar : MonoBehaviour {
    public List<tk2dSprite> MultiplierIndicators;
    public tk2dSprite Background;
    public tk2dClippedSprite Foreground;
    public float fill;

    private float previousFill;
    private readonly Vector3 smallSize = new Vector3(1, 1, 1);
    private readonly Vector3 bigSize = new Vector3(2f, 2f, 1);

	// Use this for initialization
	void Start () {
        MultiplierIndicators[0].gameObject.transform.localScale = bigSize;
	}
	
	// Update is called once per frame
	void Update () {
        var updatedClipRect = new Rect(Foreground.ClipRect);
        updatedClipRect.height = Mathf.Clamp(fill, 0, 1);
        Foreground.ClipRect = updatedClipRect;

        int previousMultiplier = Mathf.Clamp((int)(previousFill / 0.25f), 0, 3);
        int currentMultiplier = Mathf.Clamp((int)(fill / 0.25f), 0, 3);

        if (currentMultiplier != previousMultiplier) {
            var prevMultIndicator = MultiplierIndicators[previousMultiplier];
            iTween.ScaleTo(prevMultIndicator.gameObject, iTween.Hash(
                "scale", smallSize, 
                "time", 0.75f,
                "easetype", "easeInOutBack"));
            var newMultIndicator = MultiplierIndicators[currentMultiplier];
            iTween.ScaleTo(newMultIndicator.gameObject, iTween.Hash(
                "scale", bigSize,
                "time", 0.75f,
                "easetype", "easeInOutBack"));
        }

        previousFill = fill;
	}
}
