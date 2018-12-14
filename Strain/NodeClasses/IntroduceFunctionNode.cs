using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceFunctionNode : Node
    {
        public string FunctionName { get; protected set; }
        public List<Node> ParameterNodes { get; protected set; }

        public IntroduceFunctionNode(string functionName, List<Node> parameterList, List<Node> nodes)
        {
            FunctionName = functionName;
            Nodes = nodes;
            ParameterNodes = parameterList;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context)
        {
            var list = new List<Expression>() { new Expression(05, FunctionName) };

            int i = 0;

            //now copy the parameters into the correct scope
            list.AddRange(ParameterNodes.SelectMany(x => x.Compile(scope,context.NewContext()).ToList()));

            int ii = 0;
            
            list.AddRange(Nodes.SelectMany(x => x.Compile(scope,context.NewContext())));
            
            list.Add(new Expression(20));
            
            return list;
        }
    }
}
