using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util;
using Assets.Scripts.Util.Extensions;
using System.Linq;
using MoreLinq;
using System;

namespace Assets.Scripts.GameModes {
    internal class PeriodicGameMode : GameMode {
        protected Queue<PreviewPiece> pieceQueue;
        public Queue<PreviewPiece> PieceQueue {
            get { return pieceQueue; }
            protected set { pieceQueue = value; }
        }

        public override string ModeName {
            get { return "PeriodicMode"; }
        }

        protected const float TICK_DURATION = 1f;
        protected const float TIME_TILL_HINT = 10f;
        protected const float INITIAL_TIME = 120f; //Seconds

        protected int chain;
        protected decimal multiplier;
        protected float timer;
        protected float hintTimer;
        protected float initialBoardClearYPos;
        protected Dictionary<int, int> layoutUsage;
        protected dfLabel bottomTimer;

        protected MultiplierBar timeBar;

        public override void Initialize(List<Grid<int>> initialPieceLayouts) {
            UnityEngine.Random.seed = GlobalData.Instance.periodicModeSeed;
            base.Start();

            multiplier = 1;
            chain = 0;
            timer = INITIAL_TIME;
            hintTimer = TIME_TILL_HINT;

            Board.Instance.Initialize(GameControl.Instance.mainCamera, GameControl.Instance.GameplayBounds, 10, 10);

            GameUIController.Instance.lifeCounter.IsVisible = false;
            bottomTimer = GameController.bottomTimer;
            bottomTimer.IsVisible = true;
            bottomTimer.Text = "2:00";

            //Multiplier bar
            timeBar = Instantiate(GameControl.Instance.bonusBarPrefab, new Vector3(4f, 5.4f, 0), Quaternion.identity) as MultiplierBar;
            timeBar.name = GameControl.Instance.bonusBarPrefab.name;
            timeBar.MultiplierIndicators.ForEach(m => m.renderer.material.color = new Color(m.renderer.material.color.r, m.renderer.material.color.g, m.renderer.material.color.b, 0));
            timeBar.Foreground.SetSprite("MultBarFillPeriodic");

            var levelProgressIndicator = GameController.uiController.gameInfo.GetComponent<LevelProgressIndicator>();
            levelProgressIndicator.Construct(this);
            levelProgressIndicator.levelLabel.TextScale = 100f / LevelProgressIndicator.levelLabel.Font.FontSize;
            levelProgressIndicator.levelLabel.Text = "x1.0";
            LevelProgressIndicator.incomingLevelLabel.TextScale = 100f / LevelProgressIndicator.incomingLevelLabel.Font.FontSize;
            levelProgressIndicator.progressBar.IsVisible = false;

            //Redefiniendo los piece layouts
            initialPieceLayouts = new List<Grid<int>>();
            var provisionalPieceLayoutList = new List<Grid<int>>();
            for (int i = 0; i < GameControl.TOTAL_PIECES; i++) {
                var layout = new Grid<int>();
                layout[0, 0] = 1;
                provisionalPieceLayoutList.Add(layout);
            }

            var complicationLevels = new List<int> { 3, 4, 4, 5, 6, 6 };

            foreach (var complicationLevel in complicationLevels) {
                var index = UnityEngine.Random.Range(0, provisionalPieceLayoutList.Count);
                var layout = provisionalPieceLayoutList[index];
                provisionalPieceLayoutList.RemoveAt(index);

                for (int i = 0; i < complicationLevel - 1; i++) {
                    layout = GameControl.Instance.ComplicateLayout(layout);
                }
                initialPieceLayouts.Add(layout);
            }

            GameController.PieceLayouts = initialPieceLayouts;

            layoutUsage = new Dictionary<int, int>();
            pieceQueue = new Queue<PreviewPiece>();

            for (int i = TOTAL_PIECE_PREVIEWS; i >= 0; i--) {
                var previewPiece = GameControl.Instantiate(GameController.previewPiecePrefab, new Vector3(PIECE_PREVIEW_INITIAL_X, PIECE_PREVIEW_INITIAL_Y + i * 1.6f, 0), Quaternion.identity) as PreviewPiece;
                var index = UnityEngine.Random.Range(0, initialPieceLayouts.Count);
                previewPiece.gameObject.name = GameController.previewPiecePrefab.name;
                previewPiece.Init(initialPieceLayouts[index].Copy(), index);
                pieceQueue.Enqueue(previewPiece);
            }

            for (int i = 0; i < initialPieceLayouts.Count; i++) {
                layoutUsage.Add(i, 0);
            }

            initialBoardClearYPos = GameController.boardClearText.Position.y;
            GameController.simpleSongController.PlaylistVolume = 0;
            GameController.enhancedSongController.PlaylistVolume = 1;
        }

        new void Update() {
            base.Update();

            if (UserHasPlayed && !GameController.GameOver && GameControl.Instance.CurrentPiece.gameObject.activeSelf) { //If the player has played, start counting time
                TimePlayed += Time.deltaTime;

                var timeLeft = (int)Mathf.Clamp(120 - TimePlayed, 0, 120);
                var secondsLeft = timeLeft % 60;
                var minutesLeft = timeLeft / 60;
                bottomTimer.Text = String.Format("{0:0}:{1:00}", minutesLeft, secondsLeft);

                if (timer > 0) {
                    timer = timer - Time.deltaTime;
                } else {
                    StartCoroutine(GameController.ResolveBoardChange(0));
                    //GameController.ResolveBoardChange(0);
                }

                if (hintTimer > 0) {
                    hintTimer -= Time.deltaTime;
                } else if (!Board.Instance.ShowingHint) {
                    Board.Instance.ShowHint(GameControl.Instance.CurrentPiece.Layout);
                    HintsUsed++;
                }
            }

            timeBar.fill = timer / INITIAL_TIME;
        }

        public override IEnumerator StartGame(float delay) {
            var animLength = 0.5f;

            //Animate bonus bar
            iTween.ColorFrom(timeBar.gameObject, iTween.Hash("a", 0, "time", 1, "delay", 0, "easetype", "easeInOutBack"));
            iTween.MoveFrom(timeBar.gameObject, iTween.Hash("z", -5f, "time", 1, "delay", 0, "easetype", "easeInOutBack"));
            var child = timeBar.GetComponentInChildren<tk2dSprite>();
            iTween.ColorTo(child.gameObject, iTween.Hash("a", 1, "time", animLength, "delay", delay - animLength, "easetype", "easeOutCubic"));

            yield return new WaitForSeconds(delay);

            var piece = GameControl.Instantiate(GameController.piecePrefab, GameController.SlotGrid[4, 4].transform.position, Quaternion.identity) as Piece;
            piece.name = GameController.piecePrefab.name;
            GameController.CurrentPiece = piece;

            var nextPieceInfo = GetNextPiece();
            GameController.CurrentPiece.InitPiece(nextPieceInfo.Layout, nextPieceInfo.Variant);

            GameController.GameStarted = true;

            PlaylistController.InstanceByName("PC Enhanced Music").PlayRandomSong();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            PiecesPlaced++;
            GameControl.Instance.ClearAnimationOngoing = true;

            var squaresToClear = Board.Instance.SweepGrid();
            var blocksToDestroy = Board.Instance.ClearSquares();

            #region SCORING IS CALCULATED HERE

            //Scoring for piece placed
            var totalScoreGain = 1;

            if (squaresToClear.Count > 0) {
                multiplier += 0.1m;
                LevelProgressIndicator.ChangeText(string.Format("x{0:#.#}", multiplier));

                //Scoring for square clear
                totalScoreGain += Mathf.FloorToInt((blocksToDestroy.Count / 16) * 500 *
                                                   (squaresToClear.Count + 3) / 4 * (float)multiplier *
                                                   (chain + 3) / 4);

                if (Board.Instance.BoardIsCompletelyCleared()) {
                    totalScoreGain += 2500;
                }
            }

            Debug.Log("Total Score Gain: " + totalScoreGain);
            Debug.Log("After multiplying: " + (int)(totalScoreGain * multiplier));
            Score += totalScoreGain;

            #endregion

            yield return new WaitForSeconds(blockDestructionDelay);

            if (squaresToClear.Count > 0) {
                SquaresCleared += squaresToClear.Count;
                BlocksCleared += blocksToDestroy.Count;

                Board.Instance.DestroyBlocks(blocksToDestroy);

                foreach (var clearedSquarePosition in squaresToClear) {
                    var effect = Instantiate(GameControl.Instance.squareSolvedEffectPrefab) as GameObject;
                    effect.GetComponent<SquareSolvedEffect>().Initialize(clearedSquarePosition);
                }

                int textPosition = 0;
                float shakeIntensity;
                if (squaresToClear.Count > 2) {
                    shakeIntensity = Mathf.Lerp(1, 3, squaresToClear.Count / 18f);

                    EnqueueMessage("COMBO", squaresToClear.Count.ToString());
                    MaxCombo = Mathf.Max(squaresToClear.Count, MaxCombo);
                    TotalCombos++;
                } else {
                    shakeIntensity = 0;
                }

                iTween.ShakePosition(GameControl.Instance.mainCamera.gameObject,
                    iTween.Hash("time", 0.25f * shakeIntensity,
                                "amount", new Vector3(0.2f, 0.2f, 0) * shakeIntensity));

                chain++;
                MaxChain = Mathf.Max(chain, MaxChain);
                if (chain > 1) {
                    EnqueueMessage("CHAIN", chain.ToString());
                }

                if (Board.Instance.BoardIsCompletelyCleared()) {
                    foreach (var slot in Board.Instance.SlotGrid) {
                        slot.GlowEnd();
                    }

                    ShowBoardClear(textPosition++);

                    MasterAudio.PlaySound("BoardClear", 1);
                } else {
                    MasterAudio.PlaySound("SquareClear", 1);
                }

                Board.Instance.StopHint();
            } else {
                if (chain > 1) {
                    TotalChains++;
                }
                chain = 0;
                Board.Instance.StopHint();
            }

            GameControl.Instance.ClearAnimationOngoing = false;
        }

        public override bool IsGameOver() {
            var nextPiece = pieceQueue.Peek();
            var possiblePlays = Board.Instance.PossiblePlays(nextPiece.Layout);

            if (timer <= 0 || possiblePlays == 0) {
                if (timer <= 0) {
                    GameUIController.Instance.noMoreMovesMessage.GetComponentInChildren<dfLabel>().Text = I2.Loc.ScriptLocalization.Get("Time Up"); //"Time Up!";
                }
                return true;
            } else {
                return false;
            }
        }

        public override LayoutDTO GetNextPiece() {
            var nextPiece = pieceQueue.Peek();
            var possiblePlays = Board.Instance.PossiblePlays(nextPiece.Layout);
            LayoutDTO layoutValues;

            if (possiblePlays > 0) {
                //Creates a new piecePreview to be enqueued
                var newPiecePreview = GameControl.Instantiate(GameController.previewPiecePrefab, new Vector3(PIECE_PREVIEW_INITIAL_X, -0.6f, 0), Quaternion.identity) as PreviewPiece;
                newPiecePreview.gameObject.name = GameController.previewPiecePrefab.name;

                //Selects what layout that piece will have
                var minUsage = layoutUsage.MinBy(x => x.Value);
                var newIndex = layoutUsage.Where(x => x.Value <= minUsage.Value + 1).RandomElement().Key;
                layoutUsage[newIndex]++;

                //Enqueues the new piecePreview
                newPiecePreview.Init(GameController.PieceLayouts[newIndex].Copy(), newIndex);
                pieceQueue.Enqueue(newPiecePreview);

                var position = 0;
                foreach (var piecePreview in pieceQueue) {
                    iTween.MoveTo(piecePreview.gameObject, iTween.Hash(
                        "name", "tweenY",
                        "y", PIECE_PREVIEW_INITIAL_Y + (TOTAL_PIECE_PREVIEWS - position++ + 1) * 1.6f,
                        "time", 1f));
                }

                //Dequeues top piece
                var headPiece = pieceQueue.Dequeue();
                layoutValues = new LayoutDTO { Layout = headPiece.Layout, Variant = headPiece.Variant };

                iTween.ValueTo(headPiece.gameObject, iTween.Hash(
                    "name", "tweenAlpha",
                    "from", 1f,
                    "to", 0f,
                    "time", 0.25f,
                    "easetype", "easeOutCubic",
                    "onupdate", "TweenAlpha",
                    "oncomplete", "Destroy"));
                //Destroy(headPiece.gameObject);
            } else {
                var layout = new Grid<int>();
                layout[0, 0] = 4;
                layoutValues = new LayoutDTO { Layout = layout, Variant = Block.TOTAL_VARIANTS + 1 }; //This is a heart piece
            }

            if (UserHasPlayed) { //If the player has already started playing
                hintTimer = TIME_TILL_HINT;
            }

            return layoutValues;
        }

        protected void ShowBoardClear(int textPosition) {
            float yPos;

            yPos = initialBoardClearYPos;

            GameController.boardClearText.Position = new Vector3(GameController.boardClearText.Position.x, yPos, GameController.boardClearText.Position.z);
            GameController.boardClearText.GetComponent<dfTweenGroup>().Stop();
            GameController.boardClearText.GetComponent<dfTweenGroup>().Play();
        }
    }
}
