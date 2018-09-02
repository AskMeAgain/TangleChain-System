using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts {

    [Serializable]
    public class Expression {

        public Expression(){}

        public int ByteCode { set; get; }
        public string Args1 { set; get; }
        public string Args2 { set; get; }
        public string Args3 { set; get; }

        public Expression(int bytecode, string args1, string args2, string args3) {
            ByteCode = bytecode;
            Args1 = args1;
            Args2 = args2;
            Args3 = args3;
        }

        public Expression(int bytecode, string args1, string args2) {
            ByteCode = bytecode;
            Args1 = args1;
            Args2 = args2;
            Args3 = "";
        }

        public Expression(int bytecode, string args1) {
            ByteCode = bytecode;
            Args1 = args1;
            Args2 = "";
            Args3 = "";
        }


        public override string ToString() {
            return $"{ByteCode}    {Args1}    {Args2}    {Args3};\n";
        }
    }
}
