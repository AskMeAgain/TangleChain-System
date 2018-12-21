using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class EntryNode : ParserNode
    {
        public List<ParserNode> ParameterNodes { get; set; }

        public string EntryName { get; protected set; }

        public EntryNode(string entryName, List<ParserNode> paraNodes, List<ParserNode> list)
        {
            EntryName = entryName;
            Nodes = list;
            ParameterNodes = paraNodes;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            list.Add(new Expression(05, EntryName));

            int entryIndex = 2;
            //we now need to introduce the Datafields of the parameternodes!
            list.AddRange(ParameterNodes.Cast<ParameterNode>().Select(x => new Expression(15, "Int_" + entryIndex++, $"Parameters-{x.ParameterName}-{x.FunctionName}")));
            //and then add the move command so we can actually use them
            list.AddRange(ParameterNodes.SelectMany(x => x.Compile(scope, context.NewContext())));

            ;

            //we also need to add the statevars to each entry
            scope.StateVariables.ForEach(x => list.Add(new Expression(10, x, context.OneContextUp() + "-" + x)));

            list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext())));
            list.Add(new Expression(99)); //exit program

            return list;
        }

    }
}
