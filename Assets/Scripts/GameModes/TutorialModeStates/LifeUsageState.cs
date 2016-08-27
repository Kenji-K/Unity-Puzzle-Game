using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class LifeUsageState : TutorialModeState {
        TutorialGameMode gameMode;
        int lifeUsageMoves;
        GameObject arrow = null;
        public LifeUsageState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            if (lifeUsageMoves == 0) {
                gameMode.message.ContinueAnimation();
                gameMode.modeNotification.GetComponents<dfTweenGroup>().Single(tg => tg.TweenName == "TweenOut").Play();
                arrow = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(18.2f, 2.5f, 0), Quaternion.identity) as GameObject;
                arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
                arrow.GetComponent<tk2dSprite>().scale = new Vector3(0.75f, -0.75f);
                arrow.name = "DoNotRemove";
                gameMode.controlHints.Add(arrow.GetComponent<ControlHint>());

                int[,] boardConfig = 
                                {{1, 0, 1, 1, 0, 1, 1},
                                 {1, 1, 1, 1, 1, 1, 1},
                                 {1, 1, 1, 0, 0, 1, 1},
                                 {1, 1, 0, 0, 1, 1, 1},
                                 {1, 0, 1, 1, 1, 0, 1},
                                 {1, 1, 1, 1, 1, 1, 1}};
                Board.Instance.SetBoard(boardConfig, GameControl.Instance.CurrentPiece.blockPrefab);

                yield return gameMode.WaitForSecondsWrapper(1f);
                gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/LifeUsage"); //When there are no available moves, a life is consumed.
                gameMode.message.PlayAnimation();
                lifeUsageMoves++;
            } else if (lifeUsageMoves == 1) {
                //gameMode.modeNotification.GetComponents<dfTweenGroup>().Single(tg => tg.TweenName == "TweenOut").Play();
                //yield return gameMode.WaitForSecondsWrapper(1.5f);
                arrow.GetComponent<ControlHint>().FadeOut();
                gameMode.message.ContinueAnimation();
                gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/SpecialPiece"); //"At this point you will receive a special piece.";
                gameMode.message.PlayAnimation(1f);
                lifeUsageMoves++;
                //gameMode.currentState = new GameOverExplanationState(gameMode);
            } else if (lifeUsageMoves == 2) {
                gameMode.currentState = new GameOverExplanationState(gameMode);
                GameControl.Instance.processNextMove = false;
            }
        }

        public override IEnumerator BeforeClear() {
            if (lifeUsageMoves == 2) {
                gameMode.message.ContinueAnimation();

                var position = gameMode.GameController.CurrentPiece.transform.position;
                arrow = GameMode.Instantiate(gameMode.controlHintPrefab, position, Quaternion.identity) as GameObject;
                arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
                arrow.GetComponent<tk2dSprite>().scale = new Vector3(0.75f, 0.75f);
                gameMode.controlHints.Add(arrow.GetComponent<ControlHint>());

                gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/LifeTile");
                gameMode.message.PlayAnimation();

                yield return gameMode.WaitForSecondsWrapper(2f);
                var enumerator = gameMode.WaitForSecondsOrTap(1f);
                while (enumerator.MoveNext()) {
                    yield return enumerator.Current;
                }
            }
        }
    }
}
