using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class WhileLoopNode : Node
    {
        public Node QuestionNode { get; set; }

        public WhileLoopNode(Node questionNode, List<Node> body)
        {
            Nodes.AddRange(body);
            QuestionNode = new NegateNode(questionNode);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            list.Add(Factory.Label(context + "-StartWhileLoop"));

            list.AddRange(QuestionNode.Compile(scope, context.NewContext("Question")));
            var questionResult = list.Last().Args3;

            list.Add(Factory.BranchIfOne(context + "-Bottom", questionResult));

            //body
            list.AddRange(Nodes.Compile(scope, context, "Body"));

            list.Add(Factory.Goto(context + "-StartWhileLoop"));
            list.Add(Factory.Label(context + "-Bottom"));

            return list;
        }
    }
}
