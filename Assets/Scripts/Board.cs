using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util.Extensions;
using System.Text;
using System;


public class Board : MonoBehaviour {
    public int pieceMaxWidth;
    public int pieceMaxHeight;
    public Slot slotPrefab;

    private bool initialized = false;
    private tk2dCamera mainCamera;

    private Bounds gameplayBounds;
    public Bounds GameplayBounds {
        get { return gameplayBounds; }
        set { gameplayBounds = value; }
    }

    public bool ShowingHint { get; private set; }

    private int totalColumns;
    public int TotalColumns {
        get { return totalColumns; }
        private set { totalColumns = value; }
    }

    private int totalRows;
    public int TotalRows {
        get { return totalRows; }
        private set { totalRows = value; }
    }

    private Grid<Slot> slotGrid;
    public Grid<Slot> SlotGrid {
        get { return slotGrid; }
        private set { slotGrid = value; }
    }

    private static Board instance;
    public static Board Instance {
        get { return instance; }
    }

    private List<PiecePosition> possiblePositions;

    void Awake() {
        Board.instance = this;
        ShowingHint = false;
        possiblePositions = new List<PiecePosition>();
    }

    // Use this for initialization
    void Start() {
        if (!initialized) throw new UninitializedException();

        if (totalColumns == 0 && totalRows == 0) {
            throw new System.Exception("The board has not been constructed. Call the Construct() method after Instantiate()");
        }

        slotGrid = new Grid<Slot>();
        var slotSprite = slotPrefab.GetComponent<tk2dSprite>();
        float blockHeight = slotSprite.GetUntrimmedBounds().size.y;
        float blockWidth = slotSprite.GetUntrimmedBounds().size.x;

        var offset = CalculateBoardOffsets(blockWidth, blockHeight);

        float boardOffsetY = offset.y;
        float boardOffsetX = offset.x;

        int initAnimationIndex = UnityEngine.Random.Range(0, 4);

        for (int i = 0; i < totalColumns; i++) {
            for (int j = 0; j < totalRows; j++) {
                //var position = new Vector3(xOffset + i * blockWidth, yOffset + j * blockWidth, -15f);
                var position = new Vector3(boardOffsetX + i * blockWidth, boardOffsetY + j * blockWidth, 0f);
                slotGrid[i, j] = Instantiate(slotPrefab, position, Quaternion.identity) as Slot;
                slotGrid[i, j].gameObject.name = slotPrefab.name + " (" + i + ", " + j + ")";
                slotGrid[i, j].gameObject.transform.parent = this.transform;
                slotGrid[i, j].GridPosition = new IntVector2(i, j);

                switch (initAnimationIndex) {
                    case 0:
                        var delay = UnityEngine.Random.Range(0, 0.6f);
                        iTween.ColorFrom(slotGrid[i, j].gameObject, iTween.Hash("a", 0f, "easeType", "linear", "time", 1f, "delay", delay, "includechildren", false));
                        iTween.MoveFrom(slotGrid[i, j].gameObject, iTween.Hash("z", -15f, "easetype", "easeInOutBack", "time", 1f, "delay", delay));
                        break;

                    case 1:
                        iTween.ColorFrom(slotGrid[i, j].gameObject, iTween.Hash("a", 0f, "easeType", "easeInCubic", "time", 1f, "delay", 0.6f * i / totalColumns, "includechildren", false));
                        iTween.MoveFrom(slotGrid[i, j].gameObject, iTween.Hash("z", 4f, "easetype", "easeInOutBack", "time", 1f, "delay", 0.6f * i / totalColumns));
                        break;

                    case 2:
                        iTween.ColorFrom(slotGrid[i, j].gameObject, iTween.Hash("a", 0f, "easeType", "easeInCubic", "time", 1f, "delay", 0.6f * j / totalRows, "includechildren", false));
                        iTween.MoveFrom(slotGrid[i, j].gameObject, iTween.Hash("z", 4f, "easetype", "easeInOutBack", "time", 1f, "delay", 0.6f * j / totalRows));
                        break;

                    case 3:
                        iTween.ColorFrom(slotGrid[i, j].gameObject, iTween.Hash("a", 0f, "easeType", "easeInCubic", "time", 1f, "delay", 0.6f * (i + j) / (totalColumns + totalRows), "includechildren", false));
                        iTween.MoveFrom(slotGrid[i, j].gameObject, iTween.Hash("z", 4f, "easetype", "easeInOutBack", "time", 1f, "delay", 0.6f * (i + j) / (totalColumns + totalRows)));
                        break;
                }
            }
        }

        MasterAudio.PlaySound("BoardStart", 1f, null, 0.5f);
    }

    // Update is called once per frame
    void Update() {

    }

    internal void Initialize(tk2dCamera mainCamera, Bounds gameplayBounds, int initialColumns, int initialRows) {
        this.mainCamera = mainCamera;
        this.gameplayBounds = gameplayBounds;
        totalRows = initialRows;
        totalColumns = initialColumns;
        initialized = true;
    }

    private Vector2 CalculateBoardOffsets(float blockWidth, float blockHeight) {
        float boardOffsetX;
        float boardOffsetY;

        if (mainCamera.camera.isOrthoGraphic) {
            boardOffsetX = (mainCamera.NativeScreenExtents.width - (totalColumns - 1) * blockWidth) / 2;
            boardOffsetY = (mainCamera.NativeScreenExtents.height - (totalRows - 1) * blockHeight) / 2;
        } else {
            boardOffsetX = (gameplayBounds.size.x - (totalColumns - 1) * blockWidth) / 2;
            boardOffsetY = (gameplayBounds.size.y - (totalRows - 1) * blockHeight) / 2;
        }

        return new Vector2(boardOffsetX, boardOffsetY);
    }

    /// <summary>
    /// Scans the board to check which blocks should be cleared
    /// </summary>
    /// <returns>The total amount of 4x4 squares marked for clearing</returns>
    internal List<IntVector2> SweepGrid() {
        var squaresToClear = new List<IntVector2>();
        for (int i = 0; i < TotalColumns; i++) {
            for (int j = 0; j < TotalRows; j++) {
                if (CheckSquare(new IntVector2(i, j)) > 0) {
                    squaresToClear.Add(new IntVector2(i, j));
                }
            }
        }

        return squaresToClear;
    }

    internal int CheckSquare(IntVector2 coords) {
        var sideLength = 4;

        if (coords.x + sideLength <= TotalColumns && coords.y + sideLength <= TotalRows) {
            var clear = Block.MAX_VALUE;

            for (int i = 0; i < sideLength; i++) {
                for (int j = 0; j < sideLength; j++) {
                    var currentBlock = SlotGrid[coords.x + i, coords.y + j].HeldBlock;
                    if (currentBlock != null) {
                        clear = Mathf.Min(clear, currentBlock.Value);
                    } else {
                        clear = 0;
                        break;
                    }
                }
                if (clear == 0) break;
            }

            if (clear > 0) {
                for (int i = 0; i < sideLength; i++) {
                    for (int j = 0; j < sideLength; j++) {
                        SlotGrid[coords.x + i, coords.y + j].HeldBlock.ToSubtract = Mathf.Max(SlotGrid[coords.x + i, coords.y + j].HeldBlock.ToSubtract, clear);
                    }
                }
            }

            return clear;
        }

        return 0;
    }

    internal List<Block> ClearSquares() {
        var blocksToDestroy = new List<Block>();

        for (int i = 0; i < TotalColumns; i++) {
            for (int j = 0; j < TotalRows; j++) {
                var currentBlock = SlotGrid[i, j].HeldBlock;

                if (currentBlock != null) {
                    if (currentBlock.ToSubtract > 0) {
                        blocksToDestroy.Add(currentBlock);
                    }
                }
            }
        }

        //DestroyBlocks(blocksToDestroy);

        return blocksToDestroy;
    }

    public IEnumerator DestroyBlocks(List<Block> blocksToDestroy, float delay) {
        yield return new WaitForSeconds(delay);
        foreach (var block in blocksToDestroy) {
            block.SubtractValue();
        }
    }

    public void DestroyBlocks(List<Block> blocksToDestroy) {
        foreach (var block in blocksToDestroy) {
            block.SubtractValue();
        }
    }

    internal int PossiblePlays(Grid<int> layout) {
        var possiblePlays = 0;
        possiblePositions.RemoveAll(x => true);

        for (int i = 0; i < TotalColumns; i++) {
            for (int j = 0; j < TotalRows; j++) {
                for (int direction = 0; direction < 4; direction++) {
                    if (PositionFree(layout, new IntVector2(i, j))) {
                        possiblePlays++;
                        possiblePositions.Add(new PiecePosition { Direction = direction, Position = new IntVector2(i, j), Flipped = false });
                    }

                    layout.RotateClockwise();
                }
                layout.HorizontalFlip();

                for (int direction = 0; direction < 4; direction++) {
                    if (PositionFree(layout, new IntVector2(i, j))) {
                        possiblePlays++;
                        possiblePositions.Add(new PiecePosition { Direction = direction, Position = new IntVector2(i, j), Flipped = true });
                    }

                    layout.RotateClockwise();
                }
                layout.HorizontalFlip();
            }
        }

        return possiblePlays;
    }

    internal bool PositionFree(Piece piece, IntVector2 pieceCoords = null) {
        var blocks = piece.gameObject.GetComponentsInChildren<Block>();
        var blockSide = piece.BlockSide;

        foreach (var block in blocks) {
            var blockCoordsInPiece = new IntVector2(
                (int)Mathf.Round((block.transform.position.x - piece.transform.position.x) / blockSide),
                (int)Mathf.Round((block.transform.position.y - piece.transform.position.y) / blockSide));

            IntVector2 blockCoords;
            if (pieceCoords == null) {
                blockCoords = new IntVector2(blockCoordsInPiece.x + piece.CurrentCoords.x, blockCoordsInPiece.y + piece.CurrentCoords.y);
            } else {
                blockCoords = new IntVector2(blockCoordsInPiece.x + pieceCoords.x, blockCoordsInPiece.y + pieceCoords.y);
            }

            bool positionInsideGrid = blockCoords.x >= 0 &&
                                        blockCoords.x < Board.Instance.TotalColumns &&
                                        blockCoords.y >= 0 &&
                                        blockCoords.y < Board.Instance.TotalRows;
            if (!positionInsideGrid) return false;

            var isOccupied = SlotGrid[blockCoords.x, blockCoords.y].HeldBlock != null && SlotGrid[blockCoords.x, blockCoords.y].HeldBlock.Value > 0;
            if (isOccupied) return false;
        }
        return true;
    }

    internal bool PositionFree(Grid<int> layout, IntVector2 pieceCoords) {
        var height = layout.Height;
        var width = layout.Width;
        var layoutCenter = Piece.CalculateLayoutCenter(layout);

        for (int r = 0; r < height; r++) {
            for (int c = 0; c < width; c++) {
                if (layout[c, r] <= 0) continue;

                IntVector2 blockCoords = pieceCoords - layoutCenter + new IntVector2(c, r);
                bool positionInsideGrid = blockCoords.x >= 0 &&
                                            blockCoords.x < Board.Instance.TotalColumns &&
                                            blockCoords.y >= 0 &&
                                            blockCoords.y < Board.Instance.TotalRows;
                if (!positionInsideGrid) return false;

                var isOccupied = slotGrid[blockCoords.x, blockCoords.y].HeldBlock != null &&
                                    slotGrid[blockCoords.x, blockCoords.y].HeldBlock.Value - slotGrid[blockCoords.x, blockCoords.y].HeldBlock.ToSubtract > 0;
                if (isOccupied) return false;
            }
        }

        return true;
    }

    internal void ChangeSize(int columns, int rows) {

    }

    public IEnumerator AddRows(int amount, float animationDuration = 1f) {
        var slotHeight = slotPrefab.GetComponent<tk2dSprite>().CurrentSprite.GetUntrimmedBounds().size.y;
        var slotWidth = slotPrefab.GetComponent<tk2dSprite>().CurrentSprite.GetUntrimmedBounds().size.x;

        var rowsAdded = 0;

        for (int i = 0; i < amount; i++) {
            for (int c = 0; c < TotalColumns; c++) {
                var position = SlotGrid[c, TotalRows - 1].transform.position;
                slotGrid[c, TotalRows + i] = Instantiate(slotPrefab, position, Quaternion.identity) as Slot;
                slotGrid[c, TotalRows + i].gameObject.name = slotPrefab.name + " (" + c + ", " + (TotalRows + i).ToString() + ")";
                slotGrid[c, TotalRows + i].gameObject.transform.parent = this.transform;
                slotGrid[c, TotalRows + i].GridPosition = new IntVector2(c, TotalRows + i);
            }
            rowsAdded++;
        }

        foreach (var slot in SlotGrid) {
            iTween.MoveBy(slot.gameObject, iTween.Hash(
                "y", -(0.5f * amount) * slotHeight,
                "time", animationDuration,
                "easetype", "linear",
                "name", "moveby" + slot.GridPosition
            ));
        }

        TotalRows += rowsAdded;

        var offset = CalculateBoardOffsets(slotWidth, slotHeight);
        float boardOffsetY = offset.y;

        float tweenSpeed = 0;

        for (int i = 0; i < amount; i++) {
            for (int c = 0; c < TotalColumns; c++) {
                var currentPosition = slotGrid[c, TotalRows - i - 1].transform.position;
                var destinationY = boardOffsetY + (TotalRows - i - 1) * slotHeight;

                if (tweenSpeed == 0) tweenSpeed = (destinationY - currentPosition.y) / animationDuration;

                iTween.StopByName(slotGrid[c, TotalRows - i - 1].gameObject, "moveby" + new IntVector2(c, TotalRows - i - 1));
                iTween.MoveTo(slotGrid[c, TotalRows - i - 1].gameObject,
                    iTween.Hash(
                        "y", destinationY,
                        "speed", tweenSpeed,
                        "easetype", "linear")
                );

                iTween.ColorFrom(slotGrid[c, TotalRows - i - 1].gameObject, iTween.Hash("a", 0f, "easeType", "linear", "time", animationDuration / amount, "includechildren", false));
            }

            yield return new WaitForSeconds(animationDuration / amount);
        }
    }

    public IEnumerator AddColumns(int amount, float animationDuration = 1f) {
        var slotHeight = slotPrefab.GetComponent<tk2dSprite>().CurrentSprite.GetUntrimmedBounds().size.y;
        var slotWidth = slotPrefab.GetComponent<tk2dSprite>().CurrentSprite.GetUntrimmedBounds().size.x;

        var columnsAdded = 0;

        for (int i = 0; i < amount; i++) {
            for (int r = 0; r < TotalRows; r++) {
                var position = SlotGrid[TotalColumns - 1, r].transform.position;
                slotGrid[TotalColumns + i, r] = Instantiate(slotPrefab, position, Quaternion.identity) as Slot;
                slotGrid[TotalColumns + i, r].gameObject.name = slotPrefab.name + " (" + (TotalColumns + i).ToString() + ", " + r + ")";
                slotGrid[TotalColumns + i, r].gameObject.transform.parent = this.transform;
                slotGrid[TotalColumns + i, r].GridPosition = new IntVector2(TotalColumns + i, r);
            }

            columnsAdded++;
        }

        foreach (var slot in SlotGrid) {
            iTween.MoveBy(slot.gameObject, iTween.Hash(
                "x", -(0.5f * amount) * slotHeight,
                "time", animationDuration,
                "easetype", "linear",
                "name", "moveby" + slot.GridPosition
            ));
        }

        TotalColumns += columnsAdded;

        var offset = CalculateBoardOffsets(slotWidth, slotHeight);
        float boardOffsetX = offset.x;

        float tweenSpeed = 0;

        for (int i = 0; i < amount; i++) {
            for (int r = 0; r < TotalRows; r++) {
                var currentPosition = slotGrid[TotalColumns - i - 1, r].transform.position;
                var destinationX = boardOffsetX + (TotalColumns - i - 1) * slotHeight;

                if (tweenSpeed == 0) tweenSpeed = (destinationX - currentPosition.x) / animationDuration;

                iTween.StopByName(slotGrid[TotalColumns - i - 1, r].gameObject, "moveby" + new IntVector2(TotalColumns - i - 1, r));
                iTween.MoveTo(slotGrid[TotalColumns - i - 1, r].gameObject,
                    iTween.Hash(
                        "x", destinationX,
                        "speed", tweenSpeed,
                        "easetype", "linear")
                );

                iTween.ColorFrom(slotGrid[TotalColumns - i - 1, r].gameObject, iTween.Hash("a", 0f, "easeType", "linear", "time", animationDuration / amount, "includechildren", false));
            }

            yield return new WaitForSeconds(animationDuration / amount);
        }
    }

    public bool BoardIsCompletelyCleared() {
        foreach (var slot in SlotGrid) {
            if (!slot.Cleared) return false;
        }

        return true;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        for (int r = totalRows - 1; r >= 0; r--) {
            sb.Append("{");
            for (int c = 0; c < totalColumns; c++) {
                if (SlotGrid[c, r].HeldBlock != null)
                    sb.Append(SlotGrid[c, r].HeldBlock.Value.ToString());
                else
                    sb.Append("0");
                if (c < totalColumns - 1) sb.Append(", ");
            }
            sb.Append("}\r\n");
        }

        return sb.ToString();
    }

    public void ShowHint(Grid<int> layout) {
        ShowingHint = true;
        var layoutCopy = layout.Copy();
        var chosenHint = possiblePositions.RandomElement();

        if (chosenHint.Flipped) {
            layoutCopy.HorizontalFlip();
        }

        for (int i = 0; i < chosenHint.Direction; i++) {
            layoutCopy.RotateClockwise();
        }

        var height = layoutCopy.Height;
        var width = layoutCopy.Width;
        var layoutCenter = Piece.CalculateLayoutCenter(layoutCopy);

        //Debug.Log("At position " + chosenHint.Position + (chosenHint.Flipped ? " flipped" : " not flipped") + " and with rotation " + chosenHint.Direction);

        for (int r = 0; r < height; r++) {
            for (int c = 0; c < width; c++) {
                if (layoutCopy[c, r] <= 0) continue;

                IntVector2 blockCoords = chosenHint.Position - layoutCenter + new IntVector2(c, r);
                //SlotGrid[blockCoords.x, blockCoords.y].gameObject.particleSystem.Play();
                SlotGrid[blockCoords.x, blockCoords.y].hintIndicator.StartTween();
            }
        }
    }

    public void StopHint() {
        ShowingHint = false;
        foreach (var slot in SlotGrid) {
            //slot.gameObject.particleSystem.Stop();
            slot.hintIndicator.StopTween();
        }
    }

    private class PiecePosition {
        public IntVector2 Position { get; set; }
        public int Direction { get; set; }
        public bool Flipped { get; set; }
    }

    internal void SetBoard(int[,] boardConfig, Block blockPrefab) {
        foreach (var slot in SlotGrid) {
            if (slot.HeldBlock != null) {
                this.DestroyAll(slot.HeldBlock.gameObject);
                slot.HeldBlock = null;
            }
        }

        for (int y = 0; y < boardConfig.GetLength(0); y++) {
            for (int x = 0; x < boardConfig.GetLength(1); x++) {
                if (boardConfig[boardConfig.GetLength(0) - 1 - y, x] > 0) {
                    SetBlockAt(x, y, boardConfig[boardConfig.GetLength(0) - 1 - y, x], UnityEngine.Random.Range(0, 5), blockPrefab);
                }
            }
        }
    }

    internal void SetBlockAt(int x, int y, int value, int variant, Block blockPrefab) {
        var block = Instantiate(blockPrefab) as Block;

        SlotGrid[x, y].HeldBlock = block;
        block.transform.position = SlotGrid[x, y].transform.position;
        block.Variant = variant;
        block.Value = value;
        block.PlaceBlock(new IntVector2(x, y), 0f);
    }
}
