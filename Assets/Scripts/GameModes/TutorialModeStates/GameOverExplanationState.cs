using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    class GameOverExplanationState : TutorialModeState {
        private TutorialGameMode gameMode;
        public GameOverExplanationState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            gameMode.message.ContinueAnimation();

            GameControl.Instance.CurrentPiece.gameObject.SetActive(false);
            yield return gameMode.WaitForSecondsWrapper(1.1f);

            int[,] boardConfig = 
                                {{1, 0, 1, 1, 0, 1, 1},
                                 {1, 1, 1, 1, 1, 1, 1},
                                 {1, 1, 0, 1, 1, 1, 1},
                                 {1, 1, 1, 1, 0, 1, 1},
                                 {1, 0, 1, 1, 1, 0, 1},
                                 {1, 1, 1, 1, 1, 1, 1}};
            Board.Instance.SetBoard(boardConfig, GameControl.Instance.CurrentPiece.blockPrefab);
            gameMode.GameController.lifeCounter.Count = 0;
            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/GameOver"); //"No moves or lives means GAME OVER.";
            gameMode.message.PlayAnimation();

            GameUIController.Instance.noMoreMovesMessage.GetComponents<dfTweenGroup>().Single(tg => tg.TweenName == "TweenCombined").Play();
            GameUIController.Instance.gameOverMessage.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenIn").Play();
            yield return gameMode.WaitForSecondsWrapper(1.75f);
            var enumerator = gameMode.WaitForSecondsOrTap(1f);
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
            gameMode.currentState = new TutorialEndState(gameMode);
            enumerator = gameMode.currentState.ClearBoard(blockDestructionDelay);
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }
    }
}
