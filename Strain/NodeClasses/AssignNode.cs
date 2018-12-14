using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class AssignNode : Node
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }

        public AssignNode(string name, Node node)
        {
            Name = name;
            Nodes.Add(node);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();
            context = context.OneContextUp();

            scope.AddVariable(Name, context.ToString());

            int i = 0;
            var subNodeList = Nodes.SelectMany(x => x.Compile(scope, context.NewContext()));

            list.AddRange(subNodeList);
            list.Add(new Expression(00, list.Last().Args2, context + "-" + Name));

            return list;
        }

    }
}
