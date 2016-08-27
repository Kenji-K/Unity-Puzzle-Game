using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class PieceComplexityState : TutorialModeState {
        private TutorialGameMode gameMode;
        public PieceComplexityState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            gameMode.message.ContinueAnimation();

            GameControl.Instance.CurrentPiece.gameObject.SetActive(false);
            yield return gameMode.WaitForSecondsWrapper(1f);

            gameMode.complicationPreview.StartCrossFade = true;

            //GameObject arrow = Instantiate(controlHintPrefab, new Vector3(levelProgressIndicator.transform.position.x - 2f, levelProgressIndicator.transform.position.y, 0f), Quaternion.identity) as GameObject;
            GameObject arrow = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(2.4f, 9.2f, 0f), Quaternion.identity) as GameObject;
            arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
            arrow.GetComponent<tk2dSprite>().scale = new Vector3(-0.75f, 0.75f);
            gameMode.controlHints.Add(arrow.GetComponent<ControlHint>());

            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/PieceComplexity"); //"Pieces will get more complex as the game goes on.";
            gameMode.message.PlayAnimation();

            gameMode.currentState = new BoardPaintingState(gameMode);
            GameControl.Instance.processNextMove = false;

            yield return gameMode.WaitForSecondsWrapper(1f);
            var enumerator = gameMode.WaitForSecondsOrTap(2f);
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }
    }
}
