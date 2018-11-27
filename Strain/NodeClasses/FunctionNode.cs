using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class FunctionNode : Node
    {
        private string _name;
        private List<ParameterNode> _paraNodes;

        public FunctionNode(string name, List<ParameterNode> paraList,List<Node>  list)
        {
            _name = name;
            Nodes = list;
            _paraNodes = paraList;
        }
    }
}
