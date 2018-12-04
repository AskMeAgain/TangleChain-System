using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class AssignNode : Node
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }

        public AssignNode(string name, string type, Node node)
        {
            Name = name;
            Type = type;
            Nodes = new List<Node>() { node };
        }

        public override List<Expression> Compile(string context = null)
        {
            var list = new List<Expression>();

            int i = 0;
            var subNodeList = Nodes.SelectMany(x => x.Compile(context + "-" + i++));

            list.AddRange(subNodeList);
            list.Add(new Expression(00, list.Last().Args2, Name));

            return list;
        }

    }
}
