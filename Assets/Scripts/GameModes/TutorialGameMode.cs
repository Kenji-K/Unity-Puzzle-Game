using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.GameModes.TutorialModeStates;
using Assets.Scripts.Util.Extensions;
using MoreLinq;
using UnityEngine;

namespace Assets.Scripts.GameModes {
    internal class TutorialGameMode : GrowthGameMode {
        public override string ModeName {
            get { return "TutorialMode"; }
        }

        private int previousIndex = 4;
        private bool tutoPause;
        public TutorialModeState currentState;
        public List<ControlHint> controlHints;
        public EffectManager message;
        public dfLabel modeNotification;

        private GameObject modeNotificationPrefab;
        public GameObject controlHintPrefab;

        public event LevelProgressChangedHandler LevelProgressChanged;

        void Update() {
            if (timerGo && !tutoPause) {
                if (bonusBarTimer >= 0) {
                    bonusBarTimer = Mathf.Clamp(bonusBarTimer - Time.deltaTime, 0, MULTIPLIER_TIMESPAN);
                }
            }

            bonusBar.fill = bonusBarTimer / MULTIPLIER_TIMESPAN;

            if (Input.GetMouseButtonUp((int)Mouse.LeftButton)) {
                WaitTapped();
            }

            if ((Input.GetMouseButtonUp((int)Mouse.LeftButton) || Input.anyKey) && currentState == null) {
                PersistentUtility.Instance.GoToMainMenu();
            }
        }

        public override void Initialize(List<Grid<int>> initialPieceLayouts) {
            tutoPause = true;
            controlHints = new List<ControlHint>();
            controlHintPrefab = Resources.Load<GameObject>("Prefabs/TutorialControlHint");
            modeNotificationPrefab = Resources.Load<GameObject>("Prefabs/Mode Notification");

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

            var mainPanel = GameUIController.Instance.GetComponent<dfPanel>();
            modeNotification = mainPanel.AddPrefab(modeNotificationPrefab) as dfLabel;
            modeNotification.Text = I2.Loc.ScriptLocalization.Get("Instructions/GrowthModeOnly");
            modeNotification.Opacity = 0;

            Board.Instance.Initialize(GameControl.Instance.mainCamera, GameControl.Instance.GameplayBounds, 6, 6);

            //Multiplier bar
            bonusBar = Instantiate(GameControl.Instance.bonusBarPrefab, new Vector3(4f, 5.4f, 0), Quaternion.identity) as MultiplierBar;
            bonusBar.name = GameControl.Instance.bonusBarPrefab.name;
            bonusBar.Foreground.SetSprite("MultBarFillTutorial");

            foreach (var childRenderer in bonusBar.GetComponentsInChildren<Renderer>()) {
                childRenderer.material.color = new Color(childRenderer.material.color.r, childRenderer.material.color.g, childRenderer.material.color.b, 0);
            }

            //GameController.uiController.gameInfo.Opacity = 0;

            message = GameControl.Instantiate(GameController.messageTextPrefab) as EffectManager;
            message.m_character_size = 0.07f;
            message.m_line_height *= 1.2f; 
            message.m_animate_per = AnimatePerOptions.LINE;

            List<Grid<int>> pieceLayouts = ArrangePieceLayouts();

            GameController.PieceLayouts = new List<Grid<int>>();
            foreach (var pieceLayout in pieceLayouts) {
                GameController.PieceLayouts.Add(pieceLayout.Copy());
            }

            var complicationPreviewPrefab = Resources.Load<GameObject>("Prefabs/Complication Preview");
            var complicationPreviewGameObject = Instantiate(complicationPreviewPrefab) as GameObject;
            complicationPreview = complicationPreviewGameObject.GetComponent<ComplicationPreview>();
            int chosenIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, 6));
            ComplicateNextPiece(chosenIndex);

            pieceQueue = new Queue<PreviewPiece>();

            for (int i = TOTAL_PIECE_PREVIEWS - 1; i >= 0; i--) {
                var previewPiece = GameControl.Instantiate(GameController.previewPiecePrefab, new Vector3(PIECE_PREVIEW_INITIAL_X, PIECE_PREVIEW_INITIAL_Y + i * 1.6f, 0), Quaternion.identity) as PreviewPiece;
                var index = TOTAL_PIECE_PREVIEWS - i - 1;
                previewPiece.gameObject.name = GameController.previewPiecePrefab.name;
                previewPiece.Init(pieceLayouts[index], index);
                pieceQueue.Enqueue(previewPiece);
            }
        }

        private List<Grid<int>> ArrangePieceLayouts() {
            List<Grid<int>> pieceLayouts = new List<Grid<int>>();

            var layout = new Grid<int>();
            layout[0, 0] = 1; layout[1, 0] = 1; 
            /*layout[0, 1] = 0*/
            layout[1, 1] = 1; layout[2, 1] = 1;
            pieceLayouts.Add(layout);

            layout = new Grid<int>();
            layout[0, 0] = 1; layout[1, 0] = 1;
            layout[0, 1] = 0; layout[1, 1] = 1; layout[2, 1] = 1;
            layout[0, 2] = 0; layout[1, 2] = 1;
            pieceLayouts.Add(layout);

            layout = new Grid<int>();
            layout[0, 0] = 1; layout[1, 0] = 1;
            layout[0, 1] = 1; layout[1, 1] = 1; layout[2, 1] = 1;
            pieceLayouts.Add(layout);

            layout = new Grid<int>();
            layout[0, 0] = 3; layout[1, 0] = 2;
            /*layout[0, 1] = 0*/
            layout[1, 1] = 1; layout[2, 1] = 2;
            pieceLayouts.Add(layout);

            layout = new Grid<int>();
            layout[0, 0] = 1; layout[1, 0] = 1; layout[2, 0] = 1;
            layout[0, 1] = 0; layout[1, 1] = 1; layout[2, 1] = 1;
            layout[0, 2] = 0; layout[1, 2] = 1;
            pieceLayouts.Add(layout);

            layout = new Grid<int>();
            layout[0, 0] = 1; layout[1, 0] = 1; layout[2, 0] = 1;
            layout[0, 1] = 1; layout[1, 1] = 1; layout[2, 1] = 1;
            layout[0, 2] = 0; layout[1, 2] = 1; layout[2, 2] = 0;
            pieceLayouts.Add(layout);

            //layout = new Grid<int>();
            //layout[0, 0] = 1; layout[1, 0] = 1;
            ///*layout[0, 1] = 0*/
            //layout[1, 1] = 1; layout[2, 1] = 1;
            //pieceLayouts.Add(layout);
            return pieceLayouts;
        }

        public override IEnumerator StartGame(float delay) {
            currentState = new GameGoalState(this);
            yield return StartCoroutine(currentState.StartGame(delay));

            currentState = new PiecePlacementState(this);
            yield return StartCoroutine(currentState.StartGame(delay));

            var nextPieceInfo = GetNextPiece();
            GameController.CurrentPiece.InitPiece(nextPieceInfo.Layout, nextPieceInfo.Variant);
            GameController.GameStarted = true;

            currentState = new PieceRotationState(this);
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            Debug.Log("Current state: " + currentState.GetType());
            GameControl.Instance.ClearAnimationOngoing = true;

            //TODO-> There used to be a multiplier explanation section
            if (false) { 
                multiplier = Mathf.Max(1, Mathf.CeilToInt(bonusBarTimer / MULTIPLIER_SECTIONS));
                bonusBarTimer = Mathf.Clamp(bonusBarTimer + 1, 0, MULTIPLIER_TIMESPAN);
                timerGo = false;
            }

            yield return StartCoroutine(currentState.BeforeClear());

            var squaresToClear = Board.Instance.SweepGrid();
            var blocksToDestroy = Board.Instance.ClearSquares();
            yield return new WaitForSeconds(blockDestructionDelay);

            //Scoring for piece placed
            var totalScoreGain = Mathf.FloorToInt(level * multiplier);

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

            if (currentState.GetType() == typeof(PieceComplexityState)) {
                LevelProgressChanged += GameController.uiController.gameInfo.GetComponent<LevelProgressIndicator>().LevelProgressChangedHandler;
            }
            if (LevelProgressChanged != null)
                LevelProgressChanged(level + 9, level + 9 - turnsToLevelUp);

            if (squaresToClear.Count > 0) {
                Board.Instance.DestroyBlocks(blocksToDestroy);

                foreach (var clearedSquarePosition in squaresToClear) {
                    var effect = Instantiate(GameControl.Instance.squareSolvedEffectPrefab) as GameObject;
                    effect.GetComponent<SquareSolvedEffect>().Initialize(clearedSquarePosition);
                }

                //Scoring for square clear
                totalScoreGain += Mathf.FloorToInt((blocksToDestroy.Count / 16) *
                                                   (5 + level) / 6 * 500 *
                                                   multiplier *
                                                   (squaresToClear.Count + 3) / 4 *
                                                   (chain + 3) / 4);

                float shakeIntensity;
                if (squaresToClear.Count > 2) {
                    shakeIntensity = Mathf.Lerp(1, 3, squaresToClear.Count / 18f);

                    EnqueueMessage("COMBO", squaresToClear.Count.ToString());
                } else {
                    shakeIntensity = 0;
                }

                iTween.ShakePosition(GameControl.Instance.mainCamera.gameObject,
                    iTween.Hash("time", 0.25f * shakeIntensity,
                                "amount", new Vector3(0.2f, 0.2f, 0) * shakeIntensity));

                if (Board.Instance.BoardIsCompletelyCleared()) {
                    GainLife();
                    totalScoreGain += Mathf.FloorToInt((2000 + (500 * level)) * multiplier);

                    foreach (var slot in Board.Instance.SlotGrid) {
                        slot.GlowEnd();
                    }

                    ShowBoardClear();

                    if (currentState.GetType() == typeof(LifeUsageState)) {
                        enlargeHorizontal = !enlargeHorizontal;

                        if (enlargeHorizontal) {
                            if (Board.Instance.TotalColumns < 10) {
                                yield return StartCoroutine(Board.Instance.AddColumns(1, 0.25f));
                            }
                        } else {
                            if (Board.Instance.TotalRows < 10) {
                                yield return StartCoroutine(Board.Instance.AddRows(1, 0.25f));
                            }
                        } 
                    }

                    MasterAudio.PlaySound("BoardClear", 1);
                } else {
                    MasterAudio.PlaySound("SquareClear", 1);
                }

                Score += totalScoreGain;
                Board.Instance.StopHint();
            } else {
                Score += totalScoreGain;
                chain = 0;
                Board.Instance.StopHint();
            }

            foreach (var controlHint in controlHints.Where(c => c.name != "DoNotRemove")) {
                controlHint.FadeOut();
            }
            controlHints.RemoveAll(x => true);

            yield return StartCoroutine(currentState.ClearBoard(blockDestructionDelay));

            GameControl.Instance.ClearAnimationOngoing = false;
        }

        public override bool IsGameOver() {
            return false;
        }

        protected void ShowBoardClear() {
            float yPos;
            yPos = -480;

            GameController.boardClearText.GetComponent<dfTweenGroup>().Stop();
            GameController.boardClearText.Position = new Vector3(GameController.boardClearText.Position.x, yPos, GameController.boardClearText.Position.z);
            var tweens = GameController.boardClearText.GetComponents<dfTweenVector3>();
            foreach (var tween in tweens) {
                tween.EndValue = new Vector3(tween.EndValue.x, yPos, tween.EndValue.z);
            }
            GameController.boardClearText.GetComponent<dfTweenGroup>().Play();
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
                previousIndex = (previousIndex + 1) % 6;
                var layout = GameController.PieceLayouts[previousIndex];

                //Enqueues the new piecePreview
                newPiecePreview.Init(layout, previousIndex);
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
                    GameController.lifeCounter.Count--;
                }
            }

            if (UserHasPlayed) { //If the player has already started playing
                StopCoroutine("DecreaseMultiplier");
                StartCoroutine("DecreaseMultiplier");

                hintTimer = TIME_TILL_HINT;
            }

            return layoutValues;
        }

        float t = 0f;
        public IEnumerator WaitForSecondsOrTap(float delay) {
            t = delay;
            
            while (t > 0f) {
                t -= Time.deltaTime;
                yield return t;
            }
        }

        private void WaitTapped() {
            t = 0f;
        }

        public WaitForSeconds WaitForSecondsWrapper(float time) {
            return new WaitForSeconds(time);
        }
    }
}
