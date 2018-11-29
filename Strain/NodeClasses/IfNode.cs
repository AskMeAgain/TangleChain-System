using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class IfNode : Node
    {

        public QuestionNode Question { get; set; }

        public IfNode(QuestionNode question, List<Node> nodes)
        {
            Nodes = nodes;
            Question = question;
        }
    }
}
