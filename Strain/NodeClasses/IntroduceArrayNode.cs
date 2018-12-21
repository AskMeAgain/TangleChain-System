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
        public string Type { get; protected set; }
        public int MaxIndex { get; protected set; }

        public IntroduceArrayNode(string name, string index)
        {
            var flag = int.TryParse(index, out int result);

            if (!flag) throw new ArgumentException("index not correct!");

            MaxIndex = result;
            Name = name;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            var list = new List<Expression>();
            context = context.OneContextUp();

            scope.AddVariable(Name, context.ToString());
            scope.ArrayIndex.Add(Name, MaxIndex);

            //var subNodeList = Nodes.SelectMany(x => x.Compile(scope, context.NewContext()));

            //list.AddRange(subNodeList);
            //list.Add(new Expression(00, list.Last().Args2, context + "-" + Name + "_" + Index));

            return list;
        }
    }
}
