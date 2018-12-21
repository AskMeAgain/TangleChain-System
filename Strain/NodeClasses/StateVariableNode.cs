using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class StateVariableNode : ParserNode
    {
        public string VariableName { get; protected set; }

        public StateVariableNode(string variableName)
        {
            VariableName = variableName;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context) {
            context = context.OneContextUp();
            scope.AddVariable(VariableName, context.ToString());
            scope.StateVariables.Add(VariableName);

            return new List<Expression>();
        }
    }
}
