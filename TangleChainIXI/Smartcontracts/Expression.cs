using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts {
    [Serializable]
    public class Expression {

        public string _ { get; set; }

        public Expression(string s) {
            _ = s;
        }
    }
}
