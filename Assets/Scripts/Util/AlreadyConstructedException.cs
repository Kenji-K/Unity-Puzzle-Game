using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Util.Extensions {
    public class AlreadyConstructedException : Exception {
        public AlreadyConstructedException() : base("This class has already been constructed, it can't be constructed again.") {
            
        }
    }
}
