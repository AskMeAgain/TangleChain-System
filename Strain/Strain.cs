using System;
using System.Collections.Generic;
using System.Text;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain
{
    public class Strain
    {
        public string Code { get; set; }

        public Strain(string code)
        {

            Code = code;

        }

        public List<Expression> Compile()
        {
            return new Parser(new Lexer(Code).Lexing()).Parse();
        }
    }
}
