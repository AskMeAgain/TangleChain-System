using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class OutNode : FunctionCallNode
    {
        //List(number, addr)
        public OutNode(List<ParserNode> nodes) : base("--", nodes)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();

            //first we compile the variable/result
            list.AddRange(Nodes[0].Compile(scope, context.NewContext()));
            var numberResultAddr = list.Last().Args2;

            list.AddRange(Nodes[1].Compile(scope, context.NewContext()));
            var addrResultAddr = list.Last().Args2;

            //we now set the out transaction
            list.Add(new Expression(09, addrResultAddr, numberResultAddr));

            return list;
        }
    }
}
