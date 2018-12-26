using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class ArrayNode : ArrayBaseNode
    {

        public string Name { get; set; }

        public ArrayNode(string name, string index, Node expNode = null) : base(index)
        {
            if (expNode != null)
            {
                Nodes.Add(expNode);
            }

            Name = name;
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = base.Compile(scope, context);
            var indexResult = list.Last().Args3;

            //we need to find the highest context of the variable:
            var varContext = scope.GetHighestContext(Name, context);

            //its an assignment!
            if (Nodes.Count > 0)
            {
                //we compile the assignment first
                list.AddRange(Nodes.SelectMany(x => x.Compile(scope, context.NewContext("Assignment"))));
                var assignResult = list.Last().Args3;

                //we also need to update the state vars if its a state var!
                if (scope.StateVariables.Select(x => x.Split("_")[0]).Contains(Name))
                {
                    //we need to find out the name via compile stuff
                    list.Add(Factory.IntroduceValue("Str_" + Name + "_", context + "-Temp1"));
                    list.Add(Factory.Add(context + "-Temp1", indexResult, context + "-Result"));
                    list.Add(Factory.SetState(assignResult, "*" + context + "-Result"));
                }

                list.Add(Factory.IntroduceValue("Str_" + varContext + "-" + Name + "_", context + "-Temp1"));
                list.Add(Factory.Add(context + "-Temp1", indexResult, context + "-Result"));
                list.Add(Factory.Copy(assignResult, "*" + context + "-Result"));
            }
            else
            {
                //we just want the normal value!
                list.Add(Factory.IntroduceValue("Str_" + varContext + "-" + Name + "_", context + "-Temp1"));
                list.Add(Factory.Add(context + "-Temp1", indexResult, context + "-Result"));
                list.Add(Factory.Copy("*" + context + "-Result", "*" + context + "-Result"));
            }

            return list;

        }
    }
}
