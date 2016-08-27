using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal class GameGoalState : TutorialModeState {
        TutorialGameMode gameMode;
        public GameGoalState(TutorialGameMode gameMode) {
            this.gameMode = gameMode;
        }

        public override IEnumerator StartGame(float delay) {
            yield return gameMode.WaitForSecondsWrapper(delay);

            gameMode.message.Text = I2.Loc.ScriptLocalization.Get("Instructions/GameGoal");
            gameMode.message.PlayAnimation();

            yield return gameMode.WaitForSecondsWrapper(1f);
            var enumerator = gameMode.WaitForSecondsOrTap(3f);
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }

            gameMode.message.ContinueAnimation();

            yield return gameMode.WaitForSecondsWrapper(1f);
        }

        public override IEnumerator ClearBoard(float blockDestructionDelay) {
            throw new NotImplementedException();
        }
    }
}
