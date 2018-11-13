using System;
using System.Collections.Generic;
using Strain.Classes;
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

        public List<Expression> Parse(Node node)
        {
            return node.Parse();
        }
    }
}
