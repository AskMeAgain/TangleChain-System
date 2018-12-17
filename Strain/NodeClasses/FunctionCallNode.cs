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

        public FunctionCallNode(string name, List<ExpressionNode> paraNodes)
        {
            Name = name;
            Nodes.AddRange(paraNodes);
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {

            var list = new List<Expression>();
            var paraList = scope.GetFunctionParameter(Name);

            try
            {
                for (int i = 0; i < Nodes.Count; i++)
                {

                    list.AddRange(Nodes[i].Compile(scope, context.NewContext()));

                    var last = list.Last().Args2;

                    list.Add(new Expression(00, last, $"Parameters-{paraList[i]}-{Name}"));
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
