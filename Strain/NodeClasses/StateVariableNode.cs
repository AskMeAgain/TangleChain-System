using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class StateVariableNode : Node
    {
        public string VariableName { get; protected set; }
        public string VariableType { get; protected set; }

        public StateVariableNode(string variableName, string variableType)
        {
            VariableName = variableName;
            VariableType = variableType;
        }

        public override List<Expression> Compile(string context = null)
        {
            var list = new List<Expression>();
            list.Add(new Expression(10, VariableName, VariableName));
            return list;
        }
    }
}
