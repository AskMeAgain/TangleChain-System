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
        public int? Index { get; protected set; } = null;


        public VariableNode(string name, ExpressionNode expNode)
        {
            //it is an array
            if (name.Contains("["))
            {
                var indexString = name.Substring(name.IndexOf("[") + 1, name.Length - name.IndexOf("]"));
                var flag = int.TryParse(indexString, out int index);

                if (!flag) throw new ArgumentException("Index is not correctly formatted");

                Index = index;
                Name = name.Substring(0, name.IndexOf("["));

            }
            else
            {
                Name = name;
            }

            Nodes.Add(expNode);
            ;
        }

        public VariableNode(string name)
        {
            Name = name;

            if (name.Contains("["))
            {
                var indexString = name.Substring(name.IndexOf("[") + 1, name.Length - name.IndexOf("]"));
                var flag = int.TryParse(indexString, out int index);

                if (!flag) throw new ArgumentException("Index is not correctly formatted");

                Index = index;
                Name = name.Substring(0, name.IndexOf("["));

            }
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            //we need to find the highest context of the variable:
            var varContext = scope.GetHighestContext(Name, context);

            int i = 0;
            var list = new List<Expression>();

            //its an assignment!
            if (Nodes.Count > 0)
            {
                list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext())));
                var result = list.Last().Args2;

                //we also need to update the state vars if its a state var!
                if (scope.StateVariables.Contains(Name))
                {
                    list.Add(new Expression(06, result, Name));
                }

                list.Add(new Expression(00, result, varContext + "-" + Name + IndexString()));
                return list;
            }

            //we just want the normal value!
            return new List<Expression>() {
                new Expression(00, varContext + "-" + Name+IndexString(), context + "-Variable"+IndexString())
            };
        }

        private string IndexString()
        {

            if (Index == null) return "";
            return "_" + Index;

        }
    }
}
