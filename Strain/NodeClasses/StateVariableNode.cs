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

        public override List<Expression> Compile(string context)
        {
            context = Utils.JumpContextUp(context);
            ScopeManager.AddVariable(VariableName, context);
            ScopeManager.StateVariables.Add(VariableName);

            return new List<Expression>();
        }
    }
}
