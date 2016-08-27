using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util.Extensions;

public class SquareSolvedEffect : MonoBehaviour {
    public tk2dSprite flash;
    public tk2dSprite frame;

	// Start is called just before any of the
	// Update methods is called the first time.
    void Start() {
    }
	
	// Update is called every frame, if the
	// MonoBehaviour is enabled.
	void Update () {
		
	}

    public void Initialize(IntVector2 gridCoordinates) {
        var slot = Board.Instance.SlotGrid[gridCoordinates.x, gridCoordinates.y];

        var slotSprite = slot.GetComponent<tk2dSprite>();
        float blockHeight = slotSprite.GetUntrimmedBounds().size.y;
        float blockWidth = slotSprite.GetUntrimmedBounds().size.x;

        transform.position = slot.transform.position + new Vector3(blockWidth * 1.5f, blockHeight * 1.5f);

        var flashInTime = 0.1f;
        var flashOutTime = 1f;

        iTween.ColorFrom(flash.gameObject, iTween.Hash("a", 0, "time", flashInTime, "easetype", "easeInCubic"));
        iTween.ColorTo(flash.gameObject, iTween.Hash("a", 0, "time", flashOutTime, "easetype", "easeOutCubic", "delay", flashInTime));
        iTween.ScaleTo(flash.gameObject, iTween.Hash("x", 2, "y", 2, "time", flashOutTime, "delay", flashInTime));

        iTween.ColorFrom(frame.gameObject, iTween.Hash("a", 0, "time", flashInTime, "easetype", "easeInCubic"));
        iTween.ColorTo(frame.gameObject, iTween.Hash("a", 0, "time", flashOutTime, "easetype", "easeOutCubic", "delay", flashInTime));

        Invoke("DestroySelf", flashInTime + flashOutTime);
    }

    public void DestroySelf() {
        this.DestroyAll(gameObject);
    }
}
