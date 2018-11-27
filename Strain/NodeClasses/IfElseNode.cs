using System;
using System.Collections.Generic;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class IfElseNode : Node
    {
        private QuestionNode _question;
        public List<Node> _ifBlock;
        public List<Node> _elseBlock;

        public IfElseNode(QuestionNode question, List<Node> ifBlock, List<Node> elseBlock)
        {
            _question = question;
            _ifBlock = ifBlock;
            _elseBlock = elseBlock;
        }
    }
}
