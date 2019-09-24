using System.Collections.Generic;
using System.Linq;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain.NodeClasses
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
            var indexResult = list.Last().Args3;

            //we dynamically construct from the index the instruction
            list.Add(Factory.IntroduceValue("Str_Int_", context + "-Temp1"));
            list.Add(Factory.Add(context + "-Temp1", indexResult, context + "-Result"));
            list.Add(new Expression(_assemblyInstruction, "*" + context + "-Result", context + "-DataResult", context + "-DataResult"));
            return list;
        }
    }
}
