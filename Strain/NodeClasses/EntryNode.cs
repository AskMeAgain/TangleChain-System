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
        public List<Node> ParameterNodes { get; set; }

        public string EntryName { get; protected set; }

        public EntryNode(string entryName, List<Node> paraNodes, List<Node> list)
        {
            EntryName = entryName;
            Nodes = list;
            ParameterNodes = paraNodes;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            list.Add(new Expression(05, EntryName));

            //we start to count @ 2 because datafield contains more stuff
            int entryIndex = 2;
            //we now need to introduce the Datafields of the parameternodes!
            list.AddRange(ParameterNodes.Cast<ParameterNode>().Select(x => new Expression(15, "Int_" + entryIndex++, $"Parameters-{x.ParameterName}-{x.FunctionName}")));
            //and then add the move command so we can actually use them

            list.AddRange(ParameterNodes.Compile(scope, context));

            //we also need to add the statevars to each entry
            scope.StateVariables.ForEach(x => list.Add(new Expression(10, x, context.OneContextUp() + "-" + x)));

            list.AddRange(Nodes.Compile(scope, context));

            list.Add(new Expression(99)); 

            return list;
        }

    }
}
