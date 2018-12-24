using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceVariableNode : ParserNode
    {
        public string Name { get; protected set; }

        public IntroduceVariableNode(string name, ParserNode parserNode)
        {
            Name = name;
            Nodes.Add(parserNode);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();
            context = context.OneContextUp();

            scope.AddVariable(Name, context.ToString());

            var subNodeList = Nodes.SelectMany(x => x.Compile(scope, context.NewContext()));

            list.AddRange(subNodeList);
            list.Add(new Expression(00, list.Last().Args2, context + "-" + Name));

            return list;
        }

    }
}
