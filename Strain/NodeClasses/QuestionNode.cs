using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : Node
    {
        public string Question { get; protected set; }

        public QuestionNode(string question) {
            ;
            Question = Utils.ShrinkExpression(question);
            Nodes.Add(Utils.CreateNodeFromQuestion(Question));
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return Nodes.Compile(scope, context, "Question");
        }
    }
}
