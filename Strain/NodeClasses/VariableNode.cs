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
        public string Type { get; protected set; }


        public VariableNode(string name, string type, ExpressionNode expNode)
        {
            Name = name;
            Type = type;
            Nodes.Add(expNode);
        }

        public VariableNode(string name)
        {
            Name = name;
        }

        public override List<Expression> Compile(string context)
        {
            //we need to find the highest context of the variable:
            var varContext = ScopeManager.GetHighestContext(Name, context);

            int i = 0;
            var list = new List<Expression>();

            //its an assignment!
            if (Nodes.Count > 0)
            {
                list.AddRange(Nodes.SelectMany(x => x.Compile(context + "-" + i++)));
                var result = list.Last().Args2;

                //we also need to update the state vars if its a state var!
                if (ScopeManager.StateVariables.Contains(Name))
                {
                    list.Add(new Expression(06, result, Name));
                }

                list.Add(new Expression(00, result, varContext + "-" + Name));
                return list;
            }

            //we just want the normal value!
            return new List<Expression>() {
                new Expression(00, varContext + "-" + Name, context + "-Variable")
            };
        }
    }
}
