using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strain.Classes
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
