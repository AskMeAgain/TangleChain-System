using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class MetaNode : ArrayNode
    {

        public MetaNode(string index) : base("--", index)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            var indexList = IndexNode.Compile(scope, context.NewContext("Index"));
            var indexResult = indexList.Last().Args2;

            list.AddRange(indexList);



            list.Add(new Expression(01, "Str_Int_", context + "-Temp1"));
            list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
            list.Add(new Expression(11, "*" + context + "-Result", context + "-MetaResult"));
            return list;

        }
    }
}
