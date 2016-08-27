using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Util.Extensions {
    public class UninitializedException : Exception {
        public UninitializedException() : base("This class has not been initialized.") {
            
        }
    }
}
