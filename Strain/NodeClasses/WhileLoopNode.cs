using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class WhileLoopNode : ParserNode
    {
        public ParserNode QuestionParserNode { get; set; }

        public WhileLoopNode(ParserNode questionParserNode, List<ParserNode> body)
        {
            Nodes.AddRange(body);
            QuestionParserNode = new NegateNode(questionParserNode);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            list.Add(new Expression(05, context + "-StartWhileLoop"));
            list.AddRange(QuestionParserNode.Compile(scope, context.NewContext("Question")));
            var questionResult = list.Last().Args2;

            list.Add(new Expression(21, context + "-Bottom", questionResult));
            list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext())));
            list.Add(new Expression(13, context + "-StartWhileLoop"));
            list.Add(new Expression(05, context + "-Bottom"));

            return list;
        }
    }
}
