using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.NodeClasses
{
    public class IfNode : Node
    {
        private string _question;

        public IfNode(string question, FunctionNode node)
        {
            Nodes = new List<Node>() { node };
            _question = question;
        }
    }
}
