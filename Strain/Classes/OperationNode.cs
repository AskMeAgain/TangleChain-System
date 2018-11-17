using System;
using System.Collections.Generic;
using System.Text;

namespace Strain.Classes
{
    public class OperationNode : Node
    {

        private string _operation;

        public OperationNode(string operation, Node left, Node right)
        {
            _operation = operation;

            Nodes = new List<Node>() {
                left, right
            };
        }
    }
}
