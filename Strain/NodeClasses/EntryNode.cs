using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
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

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            list.Add(new Expression(05, EntryName));

            //we also need to add the statevars to each entry
            scope.StateVariables.ForEach(x => list.Add(new Expression(10, x, context.OneContextUp() + "-" + x)));

            list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext())));
            list.Add(new Expression(99)); //exit program

            return list;
        }

    }
}
