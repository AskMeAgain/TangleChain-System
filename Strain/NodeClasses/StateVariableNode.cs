using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
