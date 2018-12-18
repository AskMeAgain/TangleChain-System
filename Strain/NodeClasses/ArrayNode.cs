using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ArrayNode : Node
    {

        public string Name { get; set; }
        public int? Index { get; set; } = null;

        //incase of an assignment
        public ArrayNode(string name, string index, Node expNode)
        {
            var flag = int.TryParse(index, out int result);

            if (!flag) throw new ArgumentException("index not correct!");

            Index = result;
            Name = name;
            Nodes.Add(expNode);
        }

        public ArrayNode(string name, string index)
        {

            Name = name;

            var flag = int.TryParse(index, out int result);

            if (!flag) throw new ArgumentException("index not correct!");

            Index = result;

        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            //we access the metadata field!
            if (Name.Equals("_META"))
            {
                list.Add(new Expression(11, "Int_" + Index, context + "-MetaResult"));
                return list;
            }

            //we access the data field!
            if (Name.Equals("_DATA"))
            {
                list.Add(new Expression(15, "Int_" + Index, context + "-DataResult"));
                return list;
            }

            //we need to find the highest context of the variable:
            var varContext = scope.GetHighestContext(Name, context);

            //its an assignment!
            if (Nodes.Count > 0)
            {
                list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext())));
                var result = list.Last().Args2;

                //we also need to update the state vars if its a state var!
                if (scope.StateVariables.Contains(Name))
                {
                    list.Add(new Expression(06, result, Name));
                }

                list.Add(new Expression(00, result, varContext + "-" + Name + "_" + Index));
                return list;
            }

            //we just want the normal value!
            return new List<Expression>() {
                new Expression(00, varContext + "-" + Name + "_"+Index, context + "-Variable_" + Index)
            };
        }
    }
}
