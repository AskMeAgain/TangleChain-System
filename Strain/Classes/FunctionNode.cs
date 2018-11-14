using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class FunctionNode : Node
    {
        private string _name;

        public FunctionNode(string name, params Node[] list)
        {
            _name = name;
            Nodes = list.ToList();
        }

        public override List<Expression> Parse()
        {

            var list = new List<Expression>() {
                new Expression(05,_name)
            };

            Nodes.ForEach(x => list.AddRange(x.Parse()));

            list.Add(new Expression(20));

            return list;
        }
    }
}
