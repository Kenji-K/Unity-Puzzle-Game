using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Util.Extensions;
using MoreLinq;

public class Piece : MonoBehaviour {
    public Block blockPrefab;
    private IntVector2 Center;
    private int variant;

    private Grid<int> layout;
    public Grid<int> Layout {
        get { return layout; }
    }

    private IntVector2 currentCoords;
    public IntVector2 CurrentCoords {
        get { return currentCoords; }
        set { currentCoords = value; }
    }

    private float blockSide;
    public float BlockSide {
        get {
            if (blockSide == default(float)) {
                blockSide = (int)Mathf.Round(blockPrefab.GetComponent<tk2dSprite>().GetUntrimmedBounds().size.x);
                return blockSide;
            } else {
                return blockSide;
            }
        }
    }

	void Awake () {
        CurrentCoords = IntVector2.Zero;
	}
	
	// Update is called once per frame
	void Update () {
        if (!GameControl.Instance.Paused) {
            if (Slot.currentHoveredSlot != null) {
                transform.position = Slot.currentHoveredSlot.transform.position;
                CurrentCoords = Slot.currentHoveredSlot.GridPosition;
            }

            if (!GameControl.Instance.ClearAnimationOngoing) {
                if (Input.GetMouseButtonUp((int)Mouse.LeftButton)) {
                    var ray = GameControl.Instance.mainCamera.camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D raycastHitInfo;
                    raycastHitInfo = Physics2D.GetRayIntersection(ray);
                    if (raycastHitInfo.collider != null && raycastHitInfo.collider.CompareTag("Slot") && Board.Instance.PositionFree(this)) {
                        PlacePiece(0.2f);
                        StartCoroutine(GameControl.Instance.ResolveBoardChange(0.3f));
                        //GameControl.Instance.ResolveBoardChange(0.3f);
                    }
                } else if (Input.GetMouseButtonUp((int)Mouse.RightButton) || Input.GetKeyUp(KeyCode.Z)) {
                    Rotate();
                } else if (Input.GetMouseButtonUp((int)Mouse.MiddleButton) || Input.GetKeyUp(KeyCode.X)) {
                    Flip();
                }
            }
        }
	}

    internal void Rotate() {
        transform.Rotate(new Vector3(0, 0, -90));
    }

    internal void Flip() {
        var blocks = GetComponentsInChildren<Block>();

        if (Mathf.Approximately(transform.rotation.eulerAngles.z, 0f) || Mathf.Approximately(transform.rotation.eulerAngles.z, 180f)) { 
            foreach (var block in blocks) {
                block.transform.localPosition = Vector3.Scale(block.transform.localPosition, new Vector3(-1, 1, 1));
            }
        } else /*if (Mathf.Approximately(transform.rotation.eulerAngles.z, 90f) || Mathf.Approximately(transform.rotation.eulerAngles.z, 270f))*/ {
            foreach (var block in blocks) {
                block.transform.localPosition = Vector3.Scale(block.transform.localPosition, new Vector3(1, -1, 1));
            }
        }
    }

    private void PlacePiece(float animationLength) {
        var blocks = GetComponentsInChildren<Block>();

        foreach (var block in blocks) {
            var blockCoordsInPiece = new IntVector2(
                (int)Mathf.Round(block.transform.position.x - transform.position.x / BlockSide),
                (int)Mathf.Round(block.transform.position.y - transform.position.y / BlockSide));
            var blockCoords = new IntVector2(blockCoordsInPiece.x + CurrentCoords.x, blockCoordsInPiece.y + CurrentCoords.y);

            block.PlaceBlock(blockCoords, animationLength);
        }
    }

    public void InitPiece(Grid<int> layout, int index) {
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        Center = Piece.CalculateLayoutCenter(layout);
        variant = index;
        this.layout = layout;

        for (int i = 0; i < layout.Width; i++) {
            for (int j = 0; j < layout.Height; j++) {
                if (layout[i, j] <= 0) continue;

                var blockSize = blockPrefab.GetComponent<tk2dSprite>().GetUntrimmedBounds().size;
                var position = new Vector3((i - Center.x) * blockSize.x, (j - Center.y) * blockSize.y, 0);
                var block = Instantiate(blockPrefab) as Block;
                block.gameObject.transform.parent = transform;
                block.gameObject.transform.localPosition = position;
                block.gameObject.name = blockPrefab.name;
                block.ParentPiece = this;
                block.Variant = this.variant;
                block.PositionInPiece = new IntVector2(i, j);
                block.Value = layout[i, j];
            }
        }
    }

    public void DeleteBlocks() {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        foreach (var child in children) {
            if (child.tag == "Block")
                Destroy(child);
        }
    }

    public static IntVector2 CalculateLayoutCenter(Grid<int> layout) {
        var width = layout.Width;
        var height = layout.Height;

        //Special case piece:
        //   OX
        //   XO
        if (width == 2 && height == 2 && layout.Count(x => x != 0) == 2) {
            var maxValue = layout.Max();
            return layout.GetPositionsWhere(x => x == maxValue).First();
        }

        int horizontalCenter;
        if (width % 2 == 0) {
            var firstHalf = 0;
            for (int i = 0; i < width / 2; i++) {
                //firstHalf += layout.GetColumn(i).Count(x => x != 0);
                firstHalf += layout.GetColumn(i).Select(x => { return (x != 0) ? 100 + x : 0; }).Sum();
			}

            var secondHalf = 0;
            for (int i = width / 2; i < width; i++) {
                //secondHalf += layout.GetColumn(i).Count(x => x != 0);
                secondHalf += layout.GetColumn(i).Select(x => { return (x != 0) ? 100 + x : 0; }).Sum();
            }

            if (firstHalf >= secondHalf) {
                horizontalCenter = width / 2 - 1;
            } else {
                horizontalCenter = width / 2;
            }
        } else {
            horizontalCenter = (int)Mathf.Floor(layout.Width / 2);
        }

        int verticalCenter;
        if (height % 2 == 0) {
            var firstHalf = 0;
            for (int i = 0; i < height / 2; i++) {
                firstHalf += layout.GetRow(i).Select(x => { return (x != 0) ? 100 + x : 0; }).Sum();
            }

            var secondHalf = 0;
            for (int i = height / 2; i < height; i++) {
                secondHalf += layout.GetRow(i).Select(x => { return (x != 0) ? 100 + x : 0; }).Sum();
            }

            if (firstHalf >= secondHalf) {
                verticalCenter = height / 2 - 1;
            } else {
                verticalCenter = height / 2;
            }
        } else {
            verticalCenter = (int)Mathf.Floor(layout.Height / 2);
        }

        var center = new IntVector2(horizontalCenter, verticalCenter);
        return center;
    }
}
