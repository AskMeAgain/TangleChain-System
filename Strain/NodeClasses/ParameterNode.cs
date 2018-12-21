using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ParameterNode : ParserNode
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
                new Expression(00, $"Parameters-{ParameterName}-{FunctionName}", context + "-" + ParameterName)
            };

        }
    }
}
