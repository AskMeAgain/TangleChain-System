using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class FunctionCallNode : Node
    {

        public string Name { get; set; }

        public FunctionCallNode(string expression)
        {

            var helper = new ExpressionHelper(expression);

            Name = helper[0];
            
            Nodes.AddRange(ConvertStringToValues(helper.GetStringInBrackets()));
            
        }

        private List<Node> ConvertStringToValues(string parameters) {
            ;
            var list = new List<Node>();

            var arr = parameters.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            arr.ForEach(x =>
            {

                //we check if x is a variable or not
                var flag = int.TryParse(x, out int result);

                if (flag || x.StartsWith('"') && x.EndsWith('"'))
                {
                    list.Add(new ValueNode(x));
                }
                else
                {
                    list.Add(new VariableNode(x));
                }

            });

            return list;
        }

        public override List<Expression> Compile(Scope scope,ParserContext context)
        {

            var list = new List<Expression>();
            var paraList = scope.GetFunctionParameter(Name);

            try
            {
                for (int i = 0; i < Nodes.Count; i++)
                {

                    list.AddRange(Nodes[i].Compile(scope,context.NewContext()));

                    var last = list.Last().Args2;

                    list.Add(new Expression(00, last,$"Parameters-{paraList[i]}-{Name}"));
                }
            }
            catch (Exception e)
            {
                throw new Exception("provided Parameter number is not equal to the correct amount of parameters");
            }

            list.Add(new Expression(19, Name));

            return list;
        }
    }
}
