using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class AssignNode : Node
    {
        private string _name;
        private string _type;

        public AssignNode(string type, string name, Node operationNode)
        {

            _name = name;
            _type = type;

            Nodes = new List<Node>() { operationNode };

        }

        public override List<Expression> Compile(string context)
        {

            int i = 0;

            //we check were the operation node assigned the last value
            var expList = Nodes.SelectMany(x => x.Compile($"{context}-{_name}-{i++}")).ToList();

            var lastRegister = expList.Last().Args2;

            var list = new List<Expression>(expList);

            list.Add(new Expression(00, lastRegister, context));

            return list;
        }
    }
}
