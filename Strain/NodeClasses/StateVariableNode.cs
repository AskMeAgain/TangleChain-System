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
        public int? MaxIndex { get; set; }

        public StateVariableNode(string variableName, string maxIndex = null)
        {
            VariableName = variableName;

            if (int.TryParse(maxIndex, out int result))
            {
                MaxIndex = result;
            }
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            ;
            context = context.OneContextUp();
            scope.AddVariable(VariableName, context.ToString());
            scope.StateVariables.Add(VariableName);

            if (MaxIndex != null)
            {
                scope.ArrayIndex.Add(VariableName, MaxIndex.Value);
            }

            ;
            return new List<Expression>() {
                new Expression(10,VariableName,VariableName)
            };
        }
    }
}
