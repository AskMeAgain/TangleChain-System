using System;
using System.Collections.Generic;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
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

        public override List<Expression> Compile(Scope scope, string context)
        {

            context = Utils.JumpContextUp(context);
            
            scope.AddVariable(ParameterName, context);
            scope.AddFunctionParameter(ParameterName, FunctionName);

            return new List<Expression>() {
                new Expression(00, $"Parameters-{ParameterName}-{FunctionName}", context + "-" + ParameterName)
            };

        }
    }
}
