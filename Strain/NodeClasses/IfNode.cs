using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IfNode : ParserNode
    {

        public QuestionNode Question { get; protected set; }

        public IfNode(QuestionNode question, List<ParserNode> nodes)
        {
            Nodes = nodes;
            Question = question;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            //the whole question stuff
            list.AddRange(Question.Compile(scope, context.NewContext("Question")));
            var questionResult = list.Last().Args2;

            //we first check if questionStuff.Last() is 0
            list.Add(new Expression(01, "Int_1", context + "-Compare"));
            list.Add(new Expression(14, context + "-Block", questionResult, context + "-Compare"));
            list.Add(new Expression(13, context + "-EndOfBlock"));

            list.Add(new Expression(05, context + "-Block"));

            list.AddRange(Nodes.Compile(scope, context));

            list.Add(new Expression(05, context + "-EndOfBlock"));

            return list;
        }
    }
}
