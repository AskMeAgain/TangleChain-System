﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IfNode : Node
    {

        public QuestionNode Question { get; protected set; }

        public IfNode(QuestionNode question, List<Node> nodes)
        {
            Nodes = nodes;
            Question = question;
        }

        public override List<Expression> Compile(string context = null)
        {

            var list = new List<Expression>();

            //the whole question stuff
            list.AddRange(Question.Compile(context + "-Question"));
            var questionResult = list.Last().Args2;

            //we first check if questionStuff.Last() is 0
            list.Add(new Expression(01, "Int_1", context + "-Compare"));
            list.Add(new Expression(14, context + "-Block", questionResult, context + "-Compare"));
            list.Add(new Expression(13, context + "-EndOfBlock"));

            list.Add(new Expression(05, context + "-Block"));
            int i = 0;
            list.AddRange(Nodes.SelectMany(x => x.Compile(context + "-" + i++)));
            list.Add(new Expression(05, context + "-EndOfBlock"));

            return list;
        }
    }
}