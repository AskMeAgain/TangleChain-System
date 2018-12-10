using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceFunctionNode : Node
    {
        public string FunctionName { get; protected set; }
        public List<ParameterNode> ParameterNodes { get; protected set; }

        public IntroduceFunctionNode(string functionName, List<ParameterNode> parameterList, List<Node> nodes)
        {
            FunctionName = functionName;
            Nodes = nodes;
            ParameterNodes = parameterList;
        }

        public override List<Expression> Compile(string context)
        {
            var list = new List<Expression>();

            int i = 0;
            int ii = 0;
            list.Add(new Expression(05, "Exit"));
            list.Add(new Expression(05, FunctionName));
            //list.AddRange(ParameterNodes.SelectMany(x => x.Compile(context + "-Parameter-" + i++)));
            list.AddRange(Nodes.SelectMany(x => x.Compile(context + "-FunctionBody-" + ii++)));
            list.Add(new Expression(20)); //we jump back where we were

            return list;
        }
    }
}
