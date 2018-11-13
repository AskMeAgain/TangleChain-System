using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class BlockNode : Node
    {
        private string _name;

        public BlockNode(string name, List<Node> nodes = null)
        {
            _name = name;
            Nodes = nodes;
        }

        public override List<Node> Nodes { get; set; }

        public override List<Expression> Parse()
        {

            var list = new List<Expression>() {
                new Expression(05,_name)
            };

            Nodes.ForEach(x => list.AddRange(x.Parse()));

            list.Add(new Expression(05, "Exit"));

            return list;
        }

        public override string GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
