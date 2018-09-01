using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts {

    [Serializable]
    public class Variable {

        public string Variable_ { get; set; }
        public int Value { get; set; }

        public Variable(string name, int value) {
            Variable_ = name;
            Value = value;
        }
    }

}
