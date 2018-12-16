using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceVariableNode : Node
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }
        public int? Index { get; protected set; } = null;

        public IntroduceVariableNode(string name, Node node)
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

            Nodes.Add(node);

        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();
            context = context.OneContextUp();

            scope.AddVariable(Name, context.ToString());

            var subNodeList = Nodes.SelectMany(x => x.Compile(scope, context.NewContext()));

            list.AddRange(subNodeList);
            list.Add(new Expression(00, list.Last().Args2, context + "-" + Name + IndexString()));

            return list;
        }

        private string IndexString()
        {
            if (Index == null) return "";
            return "_" + Index;
        }

    }
}
