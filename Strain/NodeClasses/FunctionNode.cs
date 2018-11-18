using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class FunctionNode : Node
    {
        private string _name;

        public FunctionNode(string name, params Node[] list)
        {
            _name = name;
            Nodes = list.ToList();
        }

        public override List<Expression> Compile(string context)
        {
            int i = 0;

            var list = new List<Expression>() {
                new Expression(05,_name),
                //here we will insert the range
                new Expression(20)
            };

            list.InsertRange(1, Nodes.SelectMany(x => x.Compile($"{context}-{i++}")).ToList());

            return list;
        }
    }
}
