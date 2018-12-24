using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class VariableNode : Node
    {

        public string Name { get; protected set; }

        public VariableNode(string name, ExpressionNode expParserNode = null)
        {
            Name = name;

            if (expParserNode != null)
            {
                Nodes.Add(expParserNode);
            }
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            //we need to find the highest context of the variable:
            var varContext = scope.GetHighestContext(Name, context);

            var list = new List<Expression>();

            //its an assignment!
            if (Nodes.Count > 0)
            {
                list.AddRange(Nodes.Compile(scope, context));
                var result = list.Last().Args2;

                //we also need to update the state vars if its a state var!
                if (scope.StateVariables.Contains(Name))
                {
                    list.Add(new Expression(06, result, Name));
                }

                list.Add(new Expression(00, result, varContext + "-" + Name));
                return list;
            }

            //we just want the normal value!
            return new List<Expression>() {
                new Expression(00, varContext + "-" + Name, varContext + "-" + Name)
            };
        }
    }
}
