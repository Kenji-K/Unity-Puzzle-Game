using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class PieceFlippingState : TutorialModeState {
        private TutorialGameMode gameMode;
        public PieceFlippingState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            int[,] boardConfig = 
                        {{0, 0, 0, 0, 0, 0},
                         {0, 1, 1, 1, 1, 0},
                         {0, 0, 0, 0, 1, 0},
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

            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/Flipping"); //"Flip pieces with middle click or the X key.";
            gameMode.message.PlayAnimation();

            gameMode.currentState = new PersistentPiecesState(gameMode);
        }
    }
}
