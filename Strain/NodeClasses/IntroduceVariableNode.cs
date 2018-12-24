using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceVariableNode : Node
    {
        public string Name { get; protected set; }

        public IntroduceVariableNode(string name, Node node)
        {
            Name = name;
            Nodes.Add(node);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            context = context.OneContextUp();

            scope.AddVariable(Name, context.ToString());

            var list = new List<Expression>(Nodes.Compile(scope, context));
            var result = list.Last().Args2;

            list.Add(new Expression(00, result, context + "-" + Name, context + "-" + Name));

            return list;
        }

    }
}
