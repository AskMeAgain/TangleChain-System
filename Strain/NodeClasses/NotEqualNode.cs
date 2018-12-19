using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class NotEqualNode : EqualNode
    {

        public NotEqualNode(Node right, Node left) : base(right, left)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>(){
                new Expression(26, context + "-Temp"),
                new Expression(00, context + "-Temp", context + "-Result")
            };

            var list2 = base.Compile(scope, context);
            list2.AddRange(list);

            return list2;
        }
    }
}