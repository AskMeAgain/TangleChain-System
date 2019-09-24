using System.Collections.Generic;
using System.Linq;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
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

            list.Add(Factory.Entry(EntryName));

            //we start to count @ 2 because datafield contains more stuff
            int entryIndex = 2;
            //we now need to introduce the Datafields of the parameternodes!
            list.AddRange(ParameterNodes.Cast<ParameterNode>().Select(x => Factory.IntroduceData("Int_" + entryIndex++, $"Parameters-{x.ParameterName}-{x.FunctionName}")));
            //and then add the move command so we can actually use them

            list.AddRange(ParameterNodes.Compile(scope, context));

            //we also need to add the statevars to each entry
            scope.StateVariables.ForEach(x => list.Add(Factory.CopyState(x, context.OneContextUp() + "-" + x)));

            list.AddRange(Nodes.Compile(scope, context));

            list.Add(Factory.Exit());

            return list;
        }

    }
}
