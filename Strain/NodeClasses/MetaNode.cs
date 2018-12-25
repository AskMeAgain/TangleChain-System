using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class MetaNode : SpecialArrayNode
    {

        public MetaNode(string index) : base(index, 11)
        {
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return base.Compile(scope, context);
        }
    }
}
