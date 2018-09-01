using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts {

    [Serializable]
    public class Expression {

        public Expression(){}

        public int ByteCode { set; get; }
        public int Args1 { set; get; }
        public int Args2 { set; get; }

        public Expression(int bytecode, int args1, int args2) {

            ByteCode = bytecode;
            Args1 = args1;
            Args2 = args2;

        }

        public override string ToString() {
            return $"{ByteCode}    {Args1}    {Args2};\n";
        }
    }
}
