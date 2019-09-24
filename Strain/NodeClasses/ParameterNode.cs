using System.Collections.Generic;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class ParameterNode : Node
    {
        public string ParameterName { get; protected set; }
        public string FunctionName { get; protected set; }

        public ParameterNode(string parameterName, string functionName)
        {
            ParameterName = parameterName;
            FunctionName = functionName;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            context = context.OneContextUp();

            scope.AddVariable(ParameterName, context.ToString());
            scope.AddFunctionParameter(ParameterName, FunctionName);

            return new List<Expression>() {
                Factory.Copy($"Parameters-{ParameterName}-{FunctionName}", context + "-" + ParameterName)
            };

        }
    }
}
