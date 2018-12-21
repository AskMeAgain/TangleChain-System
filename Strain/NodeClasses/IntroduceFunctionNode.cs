using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceFunctionNode : ParserNode
    {
        public string FunctionName { get; protected set; }
        public List<ParserNode> ParameterNodes { get; protected set; }

        public IntroduceFunctionNode(string functionName, List<ParserNode> parameterList, List<ParserNode> body)
        {
            FunctionName = functionName;
            Nodes = body;
            ParameterNodes = parameterList;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context)
        {
            var list = new List<Expression>() { new Expression(05, FunctionName) };

            scope.FunctionNames.Add((FunctionName,context.ToString()));

            //now copy the parameters into the correct scope
            list.AddRange(ParameterNodes.SelectMany(x => x.Compile(scope,context.NewContext()).ToList()));
            
            list.AddRange(Nodes.SelectMany(x => x.Compile(scope,context.NewContext())));
            
            list.Add(new Expression(20));
            
            return list;
        }
    }
}
