using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class BoardExpansionState : TutorialModeState {
        private TutorialGameMode gameMode;
        public BoardExpansionState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            for (int i = 0; i < Board.Instance.SlotGrid.Width; i++) {
                for (int j = 0; j < Board.Instance.SlotGrid.Height; j++) {
                    Board.Instance.SlotGrid[i, j].GlowEnd();
                }
            }
            gameMode.message.ContinueAnimation();

            int[,] boardConfig = 
                        {{0, 0, 0, 0, 0, 0},
                         {0, 1, 1, 0, 1, 0},
                         {0, 1, 0, 0, 0, 0},
                         {0, 1, 0, 0, 0, 0},
                         {0, 1, 1, 1, 1, 0},
                         {0, 0, 0, 0, 0, 0}};
            Board.Instance.SetBoard(boardConfig, GameControl.Instance.CurrentPiece.blockPrefab);

            var boardWidth = Board.Instance.SlotGrid.Width;
            var boardHeight = Board.Instance.SlotGrid.Height;
            for (int i = 0; i < boardWidth; i++) {
                for (int j = 0; j < boardHeight; j++) {
                    if (i == 0 || i == boardWidth - 1 || j == 0 || j == boardHeight - 1) {
                        Board.Instance.SlotGrid[i, j].GlowStart();
                    }
                }
            }

            yield return gameMode.WaitForSecondsWrapper(1f);
            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/BoardExpansion");
            gameMode.message.PlayAnimation();

            gameMode.modeNotification.GetComponents<dfTweenGroup>().Single(tg => tg.TweenName == "TweenIn").Play();

            gameMode.currentState = new LifeUsageState(gameMode);
        }
    }
}
