using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
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

        public override List<Expression> Compile(Scope scope, ParserContext context) {
            
            var list = new List<Expression>();

            //the whole question stuff
            list.AddRange(Question.Compile(scope, context.NewContext("Question")));
            var questionResult = list.Last().Args3;
            
            //we first check if questionStuff.Last() is 0
            list.Add(Factory.IntroduceValue("Int_1", context + "-Compare"));
            list.Add(new Expression(14, context + "-Block", questionResult, context + "-Compare"));
            list.Add(Factory.Goto(context + "-EndOfBlock"));

            list.Add(Factory.Label(context + "-Block"));

            list.AddRange(Nodes.Compile(scope, context));

            list.Add(Factory.Label(context + "-EndOfBlock"));

            return list;
        }
    }
}
