using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Util {
    public class PeriodHashEventArgs : EventArgs {
        public int Seed { get; set; }
        public bool Success { get; set; }
    }
}
