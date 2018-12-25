using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public abstract class SpecialArrayNode : ArrayBaseNode
    {
        private int _assemblyInstruction;

        public SpecialArrayNode(string index, int assemblyInstruction) : base(index)
        {
            _assemblyInstruction = assemblyInstruction;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = base.Compile(scope, context);
            var indexResult = list.Last().Args2;

            //we dynamically construct from the index the instruction
            list.Add(new Expression(01, "Str_Int_", context + "-Temp1"));
            list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
            list.Add(new Expression(_assemblyInstruction, "*" + context + "-Result", context + "-DataResult"));
            return list;
        }
    }
}
