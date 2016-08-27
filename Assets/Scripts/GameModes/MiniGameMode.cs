using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Util.Extensions;
using UnityEngine;
using MoreLinq;

namespace Assets.Scripts.GameModes {
    internal class MiniGameMode : GameMode {
        protected Queue<PreviewPiece> pieceQueue;
        public Queue<PreviewPiece> PieceQueue {
            get { return pieceQueue; }
            protected set { pieceQueue = value; }
        }

        public override string ModeName {
            get { return "MiniMode"; }
        }

        protected const float TICK_DURATION = 1f;
        protected const float MULTIPLIER_TIMESPAN = 16f; //Seconds
        protected const int MULTIPLIER_SECTIONS = 4;
        protected const float TIME_TILL_HINT = 10f;

        protected float gracePeriod;
        protected float multiplierBonusDecreasePeriod;
        protected int chain;
        protected int turnsToLevelUp;
        protected int multiplier;
        protected float bonusBarTimer;
        protected float hintTimer;
        protected bool timerGo;
        protected bool enlargeHorizontal;
        protected float initialBoardClearYPos;
        protected Dictionary<int, int> layoutUsage;

        protected MultiplierBar bonusBar;

        protected ComplicationPreview complicationPreview;
        protected LayoutDTO normalPiece;
        protected LayoutDTO complicatedPiece;

        public override void Initialize(List<Grid<int>> initialPieceLayouts) {
            base.Start();

            level = 1;
            chain = 0;
            multiplier = 1;
            turnsToLevelUp = level + 9;
            gracePeriod = 4f;
            multiplierBonusDecreasePeriod = 1f;
            bonusBarTimer = 0;
            hintTimer = TIME_TILL_HINT;
            timerGo = false;
            normalPiece = new LayoutDTO();
            complicatedPiece = new LayoutDTO();

            Board.Instance.Initialize(GameControl.Instance.mainCamera, GameControl.Instance.GameplayBounds, 5, 5);

            var complicationPreviewPrefab = Resources.Load<GameObject>("Prefabs/Complication Preview");
            var complicationPreviewGameObject = Instantiate(complicationPreviewPrefab) as GameObject;
            complicationPreview = complicationPreviewGameObject.GetComponent<ComplicationPreview>();
            int chosenIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, 6));
            ComplicateNextPiece(chosenIndex);

            //Multiplier bar
            bonusBar = Instantiate(GameControl.Instance.bonusBarPrefab, new Vector3(4f, 5.4f, 0), Quaternion.identity) as MultiplierBar;
            bonusBar.name = GameControl.Instance.bonusBarPrefab.name;
            bonusBar.Foreground.SetSprite("MultBarFillMini");

            var levelProgressIndicator = GameController.uiController.gameInfo.GetComponent<LevelProgressIndicator>();
            levelProgressIndicator.Construct(this);

            layoutUsage = new Dictionary<int, int>();
            pieceQueue = new Queue<PreviewPiece>();

            for (int i = TOTAL_PIECE_PREVIEWS - 1; i >= 0; i--) {
                var previewPiece = GameControl.Instantiate(GameController.previewPiecePrefab, new Vector3(PIECE_PREVIEW_INITIAL_X, PIECE_PREVIEW_INITIAL_Y + i * 1.6f, 0), Quaternion.identity) as PreviewPiece;
                var index = UnityEngine.Random.Range(0, initialPieceLayouts.Count);
                previewPiece.gameObject.name = GameController.previewPiecePrefab.name;
                previewPiece.Init(initialPieceLayouts[index].Copy(), index);
                pieceQueue.Enqueue(previewPiece);
            }

            for (int i = 0; i < initialPieceLayouts.Count; i++) {
                layoutUsage.Add(i, 0);
            }
        }

        void Update() {
            base.Update();

            if (timerGo && GameControl.Instance.CurrentPiece.gameObject.activeSelf) {
                if (bonusBarTimer > 0) {
                    bonusBarTimer = Mathf.Clamp(bonusBarTimer - Time.deltaTime, 0, MULTIPLIER_TIMESPAN);
                } else {
                    if (hintTimer > 0) {
                        hintTimer -= Time.deltaTime;
                    } else if (!Board.Instance.ShowingHint) {
                        Board.Instance.ShowHint(GameControl.Instance.CurrentPiece.Layout);
                        HintsUsed++;
                    }
                }
            }

            if (UserHasPlayed && !GameController.GameOver) { //If the player has played, start counting time
                TimePlayed += Time.deltaTime;
            }

            bonusBar.fill = bonusBarTimer / MULTIPLIER_TIMESPAN;

            //Ajustar volumen de playlists dependiendo del multiplier
            if (!GameController.enhancedSongController.IsFading && !GameController.simpleSongController.IsCrossFading && !GameController.GameOver && !GameController.Paused) {
                GameController.simpleSongController.PlaylistVolume = 1 - (bonusBarTimer / MULTIPLIER_TIMESPAN);
                GameController.simpleSongController.UpdateMasterVolume();
                GameController.enhancedSongController.PlaylistVolume = (bonusBarTimer / MULTIPLIER_TIMESPAN);
                GameController.enhancedSongController.UpdateMasterVolume();
            }

            initialBoardClearYPos = GameController.boardClearText.Position.y;
        }

        public override IEnumerator StartGame(float delay) {
            var animLength = 0.5f;
            
            //Animate bonus bar
            iTween.ColorFrom(bonusBar.gameObject, iTween.Hash("a", 0, "time", 1, "delay", 0, "easetype", "easeInOutBack"));
            iTween.MoveFrom(bonusBar.gameObject, iTween.Hash("z", -5f, "time", 1, "delay", 0, "easetype", "easeInOutBack"));
            var child = bonusBar.GetComponentInChildren<tk2dSprite>();
            iTween.ColorTo(child.gameObject, iTween.Hash("a", 1, "time", animLength, "delay", delay - animLength, "easetype", "easeOutCubic"));

            //Animate level progress indicator
            //iTween.ColorFrom(levelProgressIndicator.gameObject, iTween.Hash("a", 0, "time", 1, "delay", 0, "easetype", "easeInOutBack"));
            //iTween.MoveFrom(levelProgressIndicator.gameObject, iTween.Hash("z", -5f, "time", 1, "delay", 0, "easetype", "easeInOutBack"));

            //levelTitleText.PlayAnimation();
            //levelText.PlayAnimation();

            yield return new WaitForSeconds(delay);

            var piece = GameControl.Instantiate(GameController.piecePrefab, GameController.SlotGrid[4, 4].transform.position, Quaternion.identity) as Piece;
            piece.name = GameController.piecePrefab.name;
            GameController.CurrentPiece = piece;

            var nextPieceInfo = GetNextPiece();
            GameController.CurrentPiece.InitPiece(nextPieceInfo.Layout, nextPieceInfo.Variant);

            complicationPreview.StartCrossFade = true;

            GameController.GameStarted = true;

            PlaylistController.InstanceByName("PC Enhanced Music").PlayRandomSong();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            PiecesPlaced++;
            GameControl.Instance.ClearAnimationOngoing = true;
            multiplier = Mathf.Max(1, Mathf.CeilToInt(bonusBarTimer / MULTIPLIER_SECTIONS));
            bonusBarTimer = Mathf.Clamp(bonusBarTimer + 1, 0, MULTIPLIER_TIMESPAN);
            timerGo = false;

            var squaresToClear = Board.Instance.SweepGrid();
            var blocksToDestroy = Board.Instance.ClearSquares();

            #region SCORING IS CALCULATED HERE

            //Scoring for piece placed
            var totalScoreGain = Mathf.FloorToInt(level * multiplier);

            if (squaresToClear.Count > 0) {
                //Scoring for square clear
                totalScoreGain += Mathf.FloorToInt((blocksToDestroy.Count / 16) *
                                                   (5 + level) / 6 * 500 *
                                                   multiplier *
                                                   (squaresToClear.Count + 3) / 4 *
                                                   (chain + 3) / 4);

                if (Board.Instance.BoardIsCompletelyCleared()) {
                    totalScoreGain += Mathf.FloorToInt((2000 + (500 * level)) * multiplier);
                }
            }

            Score += totalScoreGain;

            #endregion

            yield return new WaitForSeconds(blockDestructionDelay);

            turnsToLevelUp--;

            if (turnsToLevelUp == 0) {
                level++;
                OnLevelChanged(level);
                ShowBackgroundEffect();
                turnsToLevelUp = level + 9;

                if (level % 5 == 0) {
                    GainLife();
                }

                //Set the previously complicated layout in place
                GameControl.Instance.PieceLayouts[complicatedPiece.Variant] = complicatedPiece.Layout;

                //Complicate another one
                var minLayoutValue = GameController.PieceLayouts.Min(l => l.Sum());
                var selectedLayout = GameController.PieceLayouts.Where(x => x.Sum() <= minLayoutValue).RandomElement();

                ComplicateNextPiece(GameControl.Instance.PieceLayouts.IndexOf(selectedLayout));
            }

            OnLevelProgressChanged(level + 9, level + 9 - turnsToLevelUp);
            //if (LevelProgressChanged != null)
            //    LevelProgressChanged(level + 9, level + 9 - turnsToLevelUp);

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
                    GainLife();

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

            if (possiblePlays == 0 && GameController.lifeCounter.Count == 0) {
                StopCoroutine("DecreaseMultiplier");
                timerGo = false;
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
                        "y", PIECE_PREVIEW_INITIAL_Y + (TOTAL_PIECE_PREVIEWS - position++) * 1.6f,
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
                if (GameController.lifeCounter.Count >= 1) {
                    GameController.lifeCounter.Count++;
                }
            }

            if (UserHasPlayed) { //If the player has already started playing
                StopCoroutine("DecreaseMultiplier");
                StartCoroutine("DecreaseMultiplier");

                hintTimer = TIME_TILL_HINT;
            }

            return layoutValues;
        }

        protected IEnumerator DecreaseMultiplier() {
            //yield return new WaitForSeconds(gracePeriod);
            var wait = gracePeriod;
            while (wait > 0) {
                if (GameControl.Instance.CurrentPiece.gameObject.activeSelf) {
                    wait -= Time.deltaTime;
                }
                yield return null;
            }

            timerGo = true;
        }

        protected void GainLife() {
            GameController.lifeCounter.Count++;
        }

        protected void ShowBoardClear(int textPosition) {
            float yPos;

            yPos = initialBoardClearYPos;

            GameController.boardClearText.Position = new Vector3(GameController.boardClearText.Position.x, yPos, GameController.boardClearText.Position.z);
            GameController.boardClearText.GetComponent<dfTweenGroup>().Stop();
            GameController.boardClearText.GetComponent<dfTweenGroup>().Play();
        }

        protected void ComplicateNextPiece(int chosenIndex) {
            normalPiece.Layout = GameController.PieceLayouts[chosenIndex];
            normalPiece.Variant = chosenIndex;

            complicatedPiece.Layout = GameController.ComplicateLayout(chosenIndex);
            complicatedPiece.Variant = chosenIndex;

            complicationPreview.normalLayout = normalPiece;
            complicationPreview.resultingLayout = complicatedPiece;
            complicationPreview.Refresh();
        }
    }
}
