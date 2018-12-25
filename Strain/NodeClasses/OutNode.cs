﻿using System;
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
        public OutNode(Node number, Node address) : base("--", new List<Node>() { number, address })
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();

            //first we compile the variable/result
            list.AddRange(Nodes[0].Compile(scope, context.NewContext()));
            var numberResultAddr = list.Last().Args3;

            list.AddRange(Nodes[1].Compile(scope, context.NewContext()));
            var addrResultAddr = list.Last().Args3;

            //we now set the out transaction
            list.Add(new Expression(09, addrResultAddr, numberResultAddr));

            return list;
        }
    }
}
