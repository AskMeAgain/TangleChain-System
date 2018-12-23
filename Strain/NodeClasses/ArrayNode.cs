using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ArrayNode : ParserNode
    {

        public string Name { get; set; }
        public ParserNode Index { get; set; } = null;

        //incase of an assignment
        public ArrayNode(string name, string index, ParserNode expParserNode)
        {
            if (int.TryParse(index, out int result))
            {

                Index = new ValueNode(index);

            }
            else
            {
                Index = new VariableNode(index);
            }

            Name = name;
            Nodes.Add(expParserNode);
        }

        public ArrayNode(string name, string index)
        {

            Name = name;

            if (int.TryParse(index, out int result))
            {

                Index = new ValueNode(index);

            }
            else
            {
                Index = new VariableNode(index);
            }

        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();

            var indexList = Index.Compile(scope, context.NewContext("Index"));
            var indexResult = indexList.Last().Args2;

            list.AddRange(indexList);

            //we access the metadata field!
            if (Name.Equals("_META"))
            {
                list.Add(new Expression(01, "Str_Int_", context + "-Temp1"));
                list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
                list.Add(new Expression(11, "*" + context + "-Result", context + "-MetaResult"));
                return list;
            }

            //we access the data field!
            if (Name.Equals("_DATA"))
            {

                //we first need to introduce the "current" path without index
                list.Add(new Expression(01, "Str_Int_", context + "-Temp1"));
                list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
                list.Add(new Expression(15, "*" + context + "-Result", context + "-DataResult"));
                return list;
            }

            //we need to find the highest context of the variable:
            var varContext = scope.GetHighestContext(Name, context);

            //its an assignment!
            if (Nodes.Count > 0)
            {
                //we compile the assignment first
                list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext("Assignment"))));
                var assignResult = list.Last().Args2;
                
                //we also need to update the state vars if its a state var!
                if (scope.StateVariables.Select(x => x.Split("_")[0]).Contains(Name)) {
                    
                    //we need to find out the name via compile stuff
                    list.Add(new Expression(01, "Str_" + Name + "_", context + "-Temp1"));
                    list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
                    list.Add(new Expression(06, assignResult, "*" + context + "-Result"));
                }

                ;
                list.Add(new Expression(01, "Str_" + varContext + "-" + Name + "_", context + "-Temp1"));
                list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
                list.Add(new Expression(00, assignResult, "*" + context + "-Result"));
                return list;
            }

            //we just want the normal value!
            list.AddRange(indexList);
            list.Add(new Expression(01, "Str_" + varContext + "-" + Name + "_", context + "-Temp1"));
            list.Add(new Expression(03, context + "-Temp1", indexResult, context + "-Result"));
            list.Add(new Expression(00, "*" + context + "-Result", "*" + context + "-Result"));
            return list;
        }
    }
}
