using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Block : MonoBehaviour {
    public static int MAX_VALUE = 4;
    public static int TOTAL_VARIANTS = 6;
    
    private tk2dSprite sprite;
    private float alpha;

	public int Value { get; set; }
    public int Variant { get; set; }
    public int ToSubtract { get; set; }
	public Piece ParentPiece { get; set; }
    public Slot HoveredSlot { get; set; }
    public IntVector2 PositionInPiece { get; set; }

    void Awake() {
        sprite = GetComponent<tk2dSprite>();
        alpha = 0f;
        HoveredSlot = null;
        sprite.SortingOrder = 1;
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
    }

	// Use this for initialization
	void Start () {
        //iTween.ColorFrom(gameObject, iTween.Hash("a", 0, "time", 0.10f, "easetype", "easeInQuad", "delay", 0.20f));
        iTween.ValueTo(gameObject, iTween.Hash("name", "tweenAlpha", "from", 0f, "to", 1f, "time", 0.2f, "onupdate", "TweenAlpha", "easetype", "easeOutQuad"));
	}
	
	// Update is called once per frame
	void Update () {
        if ((ParentPiece != null && Board.Instance.PositionFree(ParentPiece)) || ParentPiece == null) {
            //Is either set on the board, or in the user's cursor on a free position
            sprite.SetSprite("Blocks/" + (Value - 1 + Variant * Block.MAX_VALUE).ToString());
        } else {
            //Is in the user's cursor over an occupied position
            //Debug.Log("Value: " + Value + " - Variant: " + Variant);
            sprite.SetSprite("Blocks/" + (Value - 1 + Block.TOTAL_VARIANTS * Block.MAX_VALUE).ToString());
        }

        if (ParentPiece != null) {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha * 0.5f);
        } else {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha * 1f);
        }
	}

    internal void PlaceBlock(IntVector2 coordinates, float animationLength) {
        Board.Instance.SlotGrid[coordinates.x, coordinates.y].HeldBlock = this;
        transform.parent = Board.Instance.SlotGrid[coordinates.x, coordinates.y].transform;
        HoveredSlot = Board.Instance.SlotGrid[coordinates.x, coordinates.y];
        PositionInPiece = null;
        ParentPiece = null;        

        iTween.MoveTo(gameObject, iTween.Hash("z", -1f, "time", animationLength / 2, "easetype", "linear"));
        iTween.MoveTo(gameObject, iTween.Hash("z", 0f, "time", animationLength / 2, "easetype", "easeInQuart", "delay", animationLength / 2));
        //iTween.MoveFrom(gameObject, iTween.Hash("z", -1.5f, "time", 0.10f, "easetype", "easeInQuart"));
        //iTween.ColorFrom(gameObject, iTween.Hash("a", 0, "time", 0.10f, "easetype", "easeInQuad"));

        sprite.SortingOrder = 0;
    }

    internal int SubtractValue() {
        var subtractedValue = ToSubtract;
        Value -= ToSubtract;

        if (ToSubtract > 0) {
            HoveredSlot.GlowStart();
        }

        if (Value <= 0) {
            //HoveredSlot.GlowStart();
            Destroy(gameObject);
        } else {
            ToSubtract = 0;
        }

        return subtractedValue;
    }

    public void TweenAlpha(float alphaValue) {
        alpha = alphaValue;
    }
}