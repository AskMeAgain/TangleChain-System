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

        public IntroduceFunctionNode(string functionName, List<Node> parameterList, List<Node> body)
        {
            FunctionName = functionName;
            Nodes = body;
            ParameterNodes = parameterList;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>() { Factory.Label(FunctionName) };

            scope.FunctionNames.Add((FunctionName, context.ToString()));

            //now copy the parameters into the correct scope
            list.AddRange(ParameterNodes.Compile(scope, context));

            //function body
            list.AddRange(Nodes.Compile(scope, context));

            //jump back!
            list.Add(Factory.PopAndJump());

            return list;
        }
    }
}
