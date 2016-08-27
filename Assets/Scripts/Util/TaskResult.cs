using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Util {
    public class TaskResult {
        public bool IsCompleted { get; set; }
        public bool IsFaulted { get; set; }
        public bool IsCanceled { get; set; }
    }
}
