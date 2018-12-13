using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class StateVariableNode : Node
    {
        public string VariableName { get; protected set; }

        public StateVariableNode(string variableName)
        {
            VariableName = variableName;
        }

        public override List<Expression> Compile(Scope scope,string context)
        {
            context = Utils.JumpContextUp(context);
            scope.AddVariable(VariableName, context);
            scope.StateVariables.Add(VariableName);

            return new List<Expression>();
        }
    }
}
