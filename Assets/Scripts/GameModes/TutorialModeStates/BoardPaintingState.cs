using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class BoardPaintingState : TutorialModeState {
        private TutorialGameMode gameMode;
        public BoardPaintingState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            throw new NotImplementedException();
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            int[,] boardConfig0 = 
                   {{0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0, 0}};
            Board.Instance.SetBoard(boardConfig0, GameControl.Instance.CurrentPiece.blockPrefab);

            gameMode.message.ContinueAnimation();
            gameMode.controlHints.ForEach(c => c.FadeOut());
            gameMode.controlHints.RemoveAll(x => true);
            yield return gameMode.WaitForSecondsWrapper(1f);

            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/BoardPainting");
            gameMode.message.PlayAnimation();
            GameObject arrow = GameMode.Instantiate(gameMode.controlHintPrefab, new Vector3(9.6f, 5.4f, 0f), Quaternion.identity) as GameObject;
            arrow.GetComponent<tk2dSprite>().SetSprite("Arrow");
            arrow.GetComponent<tk2dSprite>().scale = new Vector3(-0.75f, 0.75f);
            arrow.GetComponent<tk2dSprite>().SortingOrder = 4;

            yield return gameMode.WaitForSecondsWrapper(1f);
            var enumerator = gameMode.WaitForSecondsOrTap(3f);
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }

            arrow.GetComponent<ControlHint>().FadeOut();
            gameMode.message.ContinueAnimation();
            yield return gameMode.WaitForSecondsWrapper(1f);

            int[,] boardConfig = 
               {{0, 0, 0, 0, 0, 0},
                {0, 1, 0, 1, 1, 0},
                {0, 1, 0, 0, 1, 0},
                {0, 0, 0, 0, 1, 0},
                {0, 1, 1, 1, 1, 0},
                {0, 0, 0, 0, 0, 0}};
            Board.Instance.SetBoard(boardConfig, GameControl.Instance.CurrentPiece.blockPrefab);

            //Unpainting the center
            for (int i = 0; i < Board.Instance.SlotGrid.Width; i++) {
                for (int j = 0; j < Board.Instance.SlotGrid.Height; j++) {
                    Board.Instance.SlotGrid[i, j].GlowEnd();
                }
            }

            //Painting the borders
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
            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/BoardClear"); //"Clearing blocks on the whole board\r\nawards lots of points and a life.";
            gameMode.message.PlayAnimation();
            GameControl.Instance.CurrentPiece.gameObject.SetActive(true);

            gameMode.currentState = new BoardExpansionState(gameMode);
        }
    }
}
