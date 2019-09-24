using System.Collections.Generic;
using System.Linq;
using Strain.NodeClasses;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public static class Utils
    {
        public static List<Expression> Compile(this List<Node> nodes, Scope scope, ParserContext context, string contextName = null)
        {
            return nodes.SelectMany(x => x.Compile(scope, context.NewContext(contextName))).ToList();
        }

        public static string ShrinkExpression(string exp)
        {
            return exp.Replace(" [ ", "[").Replace(" ]", "]").Replace(" ( ", "(").Replace(" )", ")");
        }

        public static string StretchExpression(string exp)
        {
            return exp.Replace("[", " [ ").Replace("]", " ]").Replace("(", " ( ").Replace(")", " )");
        }

        public static Node ExpressionStringToNode(string expression)
        {
            return new ExpressionBuilder(expression).BuildExpression();
        }

        public static Node CreateNodeFromAssertion(string assertion)
        {
            return new ExpressionBuilder(assertion).BuildAssertion();
        }

        public static Node CreateNodeFromQuestion(string question)
        {
            return new ExpressionBuilder(question).BuildQuestion();
        }
    }
}
