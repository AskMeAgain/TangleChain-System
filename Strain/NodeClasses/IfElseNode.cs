using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class IfElseNode : Node
    {
        public QuestionNode Question{ get; protected set; }
        public List<Node> IfBlock{ get; protected set; }
        public List<Node> ElseBlock{ get; protected set; }

        public IfElseNode(QuestionNode question, List<Node> ifBlock, List<Node> elseBlock)
        {
            Question = question;
            IfBlock = ifBlock;
            ElseBlock = elseBlock;
        }
    }
}
