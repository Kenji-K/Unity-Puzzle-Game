using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Util;
using Assets.Scripts.Util.Extensions;
using UnityEngine;

namespace Assets.Scripts.GameModes {
    internal abstract class GameMode : MonoBehaviour {
        protected const int TOTAL_PIECE_PREVIEWS = 5;
        protected const float PIECE_PREVIEW_INITIAL_Y = 0.7f;
        protected const float PIECE_PREVIEW_INITIAL_X = 1.6f;

        public abstract string ModeName { get; }

        private GameControl gameController;
        public GameControl GameController {
            get { return gameController; }
            set { gameController = value; }
        }

        private int score = 0;
        public int Score {
            get { 
                return score; 
            }
            set {
                var previousValue = score;
                var updatedValue = value;
                score = value;
                ScoreChangedEvent(previousValue, updatedValue);
            }
        }

        private bool userHasPlayed = false;
        public bool UserHasPlayed {
            get { return userHasPlayed; }
            set { userHasPlayed = value; }
        }

        protected int level;
        public int Level {
            get { return level; }
            set { level = value; }
        }

        public int SquaresCleared { get; set; }
        public int BlocksCleared { get; set; }
        public int PiecesPlaced { get; set; }
        public int HintsUsed { get; set; }
        public int MaxCombo { get; set; }
        public int MaxChain { get; set; }
        public int TotalCombos { get; set; }
        public int TotalChains { get; set; }
        public float TimePlayed { get; set; }
        public LevelProgressIndicator LevelProgressIndicator { get; set; }

        protected Queue<MessageDTO> MessageQueue { get; set; }
        protected List<DiamondEmitter> DiamondEmitters { get; set; }

        //EVENTS

        public delegate void LevelChangedHandler(int level);
        public event LevelChangedHandler LevelChanged;

        protected void OnLevelChanged(int level) {
            LevelChangedHandler handler = LevelChanged;
            if (handler != null) {
                handler(level);
            }
        }

        public void OnLevelChangedSubscribe(LevelChangedHandler handler) {
            LevelChanged += handler;
        }

        public void OnLevelChangedUnsubscribe(LevelChangedHandler handler) {
            LevelChanged -= handler;
        }

        public delegate void LevelProgressChangedHandler(int requiredMoves, int progress);
        public event LevelProgressChangedHandler LevelProgressChanged;

        protected void OnLevelProgressChanged(int requiredMoves, int progress) {
            LevelProgressChangedHandler handler = LevelProgressChanged;
            if (handler != null) {
                handler(requiredMoves, progress);
            }
        }

        public void OnLevelProgressChangedSubscribe(LevelProgressChangedHandler handler) {
            LevelProgressChanged += handler;
        }

        public void OnLevelProgressChangedUnsubscribe(LevelProgressChangedHandler handler) {
            LevelProgressChanged -= handler;
        }

        public delegate void ScoreChangedEventHandler(int previousScore, int updatedScore);
        public event ScoreChangedEventHandler ScoreChangedEvent;


        //ABSTRACT FUNCTIONS

        public abstract void Initialize(List<Grid<int>> initialPieceLayouts);
        public abstract IEnumerator StartGame(float delay);
        public abstract IEnumerator ClearBoard(float destructionDelay);
        public abstract bool IsGameOver();
        public abstract LayoutDTO GetNextPiece();
        
        //IMPLEMENTATIONS

        protected void Start() {
            MessageQueue = new Queue<MessageDTO>();
            DiamondEmitters = FindObjectsOfType<DiamondEmitter>().ToList();
            foreach (var emitter in DiamondEmitters) {
                var color = GlobalData.Instance.SolidColors[ModeName];
                color = new Color32(color.r, color.g, color.b, 127);
                emitter.GetComponent<tk2dSprite>().color = color;
                emitter.trail.startColor = color;
            }
        }

        protected void Update() {
            if (MessageQueue.Count > 0) {
                var comboText = GameControl.Instance.comboTextQueue.Peek();
                if (comboText.Hidden) {
                    var message = MessageQueue.Dequeue();
                    comboText.type.Text = message.type;
                    comboText.number.Text = message.number;
                    comboText.AnimateIn();
                    comboText.TimeTillExit = 2.5f;
                }
            }
        }

        protected void EnqueueMessage(string type, string number) {
            var comboText = GameControl.Instance.comboTextQueue.Peek();
            if (!comboText.Hidden && !comboText.GetComponent<dfTweenVector3>().IsPlaying
                && !comboText.number.GetComponent<dfTweenFloat>().IsPlaying
                && !comboText.type.GetComponent<dfTweenFloat>().IsPlaying
                && comboText.TimeTillExit > 0.2f /*Tolerance*/) {

                comboText.number.Text = number;
                comboText.number.gameObject.animation.Play();
                comboText.number.gameObject.GetComponent<dfTweenFloat>().Play();
                comboText.TimeTillExit = 2.5f; //Give more time
            } else { 
                MessageQueue.Enqueue(new MessageDTO { type = type, number = number });
            }
        }

        protected void ShowBackgroundEffect() {
            foreach (var emitter in DiamondEmitters) {
                emitter.rigidbody2D.WakeUp();
            }
        }

        protected struct MessageDTO {
            public string type;
            public string number; 
        }
    }
}
