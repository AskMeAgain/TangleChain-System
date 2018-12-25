using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public abstract class ArrayBaseNode : Node
    {
        public Node IndexNode { get; set; }

        public ArrayBaseNode(string index)
        {
            IndexNode = int.TryParse(index, out int result) ? (Node)new ValueNode(index) : new VariableNode(index);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();

            var indexList = IndexNode.Compile(scope, context.NewContext("Index"));
            var indexResult = indexList.Last().Args3;

            list.AddRange(indexList);

            return list;
        }
    }
}
