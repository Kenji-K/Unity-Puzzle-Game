using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class PiecePlacementState : TutorialModeState {
        private TutorialGameMode gameMode;
        public PiecePlacementState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            var piece = GameControl.Instantiate(gameMode.GameController.piecePrefab, gameMode.GameController.SlotGrid[4, 4].transform.position, Quaternion.identity) as Piece;
            piece.name = gameMode.GameController.piecePrefab.name;
            gameMode.GameController.CurrentPiece = piece;

            int[,] boardConfig = 
                {{0, 0, 0, 0, 0, 0},
                 {0, 1, 1, 1, 1, 0},
                 {0, 1, 1, 0, 0, 0},
                 {0, 1, 0, 0, 1, 0},
                 {0, 1, 1, 1, 1, 0},
                 {0, 0, 0, 0, 0, 0}};

            Board.Instance.SetBoard(boardConfig, gameMode.GameController.CurrentPiece.blockPrefab);

            GameObject keyHint = GameMode.Instantiate(gameMode.controlHintPrefab) as GameObject;
            gameMode.controlHints.Add(keyHint.GetComponent<ControlHint>());
            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/PiecePlacement"); //"Left click to place a piece.\r\nMake four by four squares to clear the blocks.";
            gameMode.message.PlayAnimation();

            piece.gameObject.SetActive(true);

            yield return null;
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            throw new NotImplementedException();
        }
    }
}
