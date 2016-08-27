using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    class TutorialEndState : TutorialModeState {
        private TutorialGameMode gameMode;

        public TutorialEndState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            gameMode.message.ContinueAnimation();
            GameUIController.Instance.noMoreMovesMessage.GetComponents<dfTweenVector3>().Single(tg => tg.TweenName == "TweenOut").Play();
            GameUIController.Instance.gameOverMessage.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenOut").Play();

            int[,] boardConfig = 
                {{0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0}};
            Board.Instance.SetBoard(boardConfig, GameControl.Instance.CurrentPiece.blockPrefab);

            for (int i = 0; i < Board.Instance.SlotGrid.Width; i++) {
                for (int j = 0; j < Board.Instance.SlotGrid.Height; j++) {
                    Board.Instance.SlotGrid[i, j].GlowEnd();
                }
            }

            yield return gameMode.WaitForSecondsWrapper(1f);
            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/End"); //"That's it! Get as many points as you can!";
            gameMode.message.PlayAnimation();

            yield return gameMode.WaitForSecondsWrapper(1.5f);
            gameMode.currentState = null;
        }
    }
}
