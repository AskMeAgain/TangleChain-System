using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class DataNode : ArrayNode
    {
        public DataNode(string index) : base("--", index)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();

            var indexList = IndexNode.Compile(scope, context.NewContext("Index"));
            var indexResult = indexList.Last().Args2;

            list.AddRange(indexList);



            //we first need to introduce the "current" path without index
            list.Add(new Expression(01, "Str_Int_", context + "-Temp1"));
            list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
            list.Add(new Expression(15, "*" + context + "-Result", context + "-DataResult"));
            return list;

        }
    }
}
