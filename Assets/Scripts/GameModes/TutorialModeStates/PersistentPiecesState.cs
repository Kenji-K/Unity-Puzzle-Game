using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class PersistentPiecesState : TutorialModeState {
        private TutorialGameMode gameMode;
        private bool madeTheMove = false;
        public PersistentPiecesState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            if (!madeTheMove) {
                int[,] boardConfig = 
                   {{0, 0, 0, 0, 0, 0},
                    {0, 1, 1, 1, 1, 0},
                    {0, 1, 1, 0, 0, 0},
                    {0, 1, 0, 0, 1, 0},
                    {0, 1, 1, 1, 1, 0},
                    {0, 0, 0, 0, 0, 0}};
                Board.Instance.SetBoard(boardConfig, GameControl.Instance.CurrentPiece.blockPrefab);
                gameMode.message.ContinueAnimation();
                yield return gameMode.WaitForSecondsWrapper(1f);

                var totalKeyHints = 2;
                var width = gameMode.controlHintPrefab.GetComponent<tk2dSprite>().GetUntrimmedBounds().size.x;

                var currentControlHint = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(9.6f - (totalKeyHints * width) / 2 + gameMode.controlHints.Count * (width + 0.5f), 9.4f), Quaternion.identity) as GameObject;
                currentControlHint.GetComponent<tk2dSprite>().SetSprite("MouseCenter");
                gameMode.controlHints.Add(currentControlHint.GetComponent<ControlHint>());

                currentControlHint = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(9.6f - (totalKeyHints * width) / 2 + gameMode.controlHints.Count * (width + 0.5f), 9.4f), Quaternion.identity) as GameObject;
                currentControlHint.GetComponent<tk2dSprite>().SetSprite("KeyX");
                gameMode.controlHints.Add(currentControlHint.GetComponent<ControlHint>());

                gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/PersistentPieces"); //"Some blocks will need to be cleared more than once.";
                gameMode.message.PlayAnimation();
                madeTheMove = true;
            } else {
                gameMode.currentState = new PieceComplexityState(gameMode);
                GameControl.Instance.processNextMove = false;
            }
        }

        public override IEnumerator BeforeClear() {
            if (madeTheMove) {
                GameObject arrow = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(8.62f, 4.43f, 0f), Quaternion.identity) as GameObject;
                arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
                arrow.GetComponent<tk2dSprite>().scale = new Vector3(0.75f, 0.75f);
                gameMode.controlHints.Add(arrow.GetComponent<ControlHint>());

                arrow = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(11.53f, 5.4f, 0f), Quaternion.identity) as GameObject;
                arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
                arrow.GetComponent<tk2dSprite>().scale = new Vector3(-0.75f, 0.75f);
                gameMode.controlHints.Add(arrow.GetComponent<ControlHint>());

                arrow = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(10.57f, 4.43f, 0f), Quaternion.identity) as GameObject;
                arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
                arrow.GetComponent<tk2dSprite>().scale = new Vector3(-0.75f, 0.75f);
                gameMode.controlHints.Add(arrow.GetComponent<ControlHint>());

                yield return gameMode.WaitForSecondsWrapper(2f);
            }
        }
    }
}
