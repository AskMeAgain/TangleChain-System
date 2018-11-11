using System;
using System.Collections.Generic;
using TangleChainIXI.Smartcontracts;

namespace Strain
{
    public class Parser
    {
        public string Code { get; set; }

        public Parser(string code)
        {
            Code = code;
        }

        public List<Expression> Parse()
        {
            List<Expression> list = new List<Expression>();

            return list;
        }
    }
}
