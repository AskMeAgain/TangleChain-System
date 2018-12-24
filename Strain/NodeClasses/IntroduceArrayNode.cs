using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class IntroduceArrayNode : ParserNode
    {
        public string Name { get; protected set; }
        public int MaxIndex { get; protected set; }

        public IntroduceArrayNode(string name, string index)
        {
            if (int.TryParse(index, out int result))
            {
                MaxIndex = result - 1;
                Name = name;
            }
            else
            {
                throw new ArgumentException("index not correct!");
            }
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            scope.AddVariable(Name, context.OneContextUp().ToString());
            scope.ArrayIndex.Add(Name, MaxIndex);

            return new List<Expression>();
        }
    }
}
