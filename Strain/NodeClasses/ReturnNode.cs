using System.Collections.Generic;
using System.Linq;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
{
    public class ReturnNode : Node
    {

        public ReturnNode(Node expNode)
        {
            Nodes.Add(expNode);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var funcName = scope.GetFunctionNameFromContext(context.ToString());

            var list = new List<Expression>();
            list.AddRange(Nodes.Compile(scope, context, "Return"));

            var lastResult = list.Last().Args3;
            list.Add(Factory.Copy(lastResult, $"FunctionReturn-{funcName}"));

            return list;
        }
    }
}
