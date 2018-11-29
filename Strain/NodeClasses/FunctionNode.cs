using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class FunctionNode : Node
    {
        public string FunctionName{ get; protected set; }
        public List<ParameterNode> ParameterNodes{ get; protected set; }

        //Nodes contain the body of the function

        public FunctionNode(string functionName, List<ParameterNode> parameterList,List<Node>  list)
        {
            FunctionName = functionName;
            Nodes = list;
            ParameterNodes = parameterList;
        }
    }
}
