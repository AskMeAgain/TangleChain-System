using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class EntryNode : Node
    {

        public string EntryName { get; protected set; }

        public EntryNode(string entryName, List<Node> list)
        {
            EntryName = entryName;
            Nodes = list;
        }

        public override List<Expression> Compile(string context)
        {

            int i = 0;
            var list = new List<Expression>();
            ;
            list.Add(new Expression(05, EntryName));
            list.AddRange(Nodes.SelectMany(x => x.Compile(context + "-" + i++)));
            list.Add(new Expression(99)); //exit program

            return list;
        }

    }
}
