using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.GameModes.TutorialModeStates {
    internal abstract class TutorialModeState {
        public abstract IEnumerator StartGame(float delay);

        public abstract IEnumerator ClearBoard(float blockDestructionDelay);

        public virtual IEnumerator BeforeClear() {
            yield return null;
        }
    }
}
