using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class QuestionNode : Node
    {
        public string Question { get; protected set; }

        public QuestionNode(string question)
        {
            Question = Utils.ShrinkExpression(question);
            Nodes.Add(Utils.CreateNodeFromQuestion(Question));
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return Nodes.Compile(scope, context, "Question");
        }
    }
}
