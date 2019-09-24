using System;
using System.Linq;
using Strain.NodeClasses;

namespace Strain.Classes
{
    internal class Parser
    {

        private LexerNode _treenode;

        public Parser(LexerNode node)
        {
            _treenode = node;
        }

        public Node Parse(LexerNode treenode = null)
        {

            //means we started 
            if (treenode == null)
            {
                //we put the whole thing in an appnode
                return new ApplicationNode(new ExpressionHelper(_treenode.Line)[0], _treenode.SubLines.Select(x => Parse(x)).ToList());
            }

            var helper = new ExpressionHelper(treenode.Line);
            var subNodes = treenode.SubLines.Select(x => Parse(x)).ToList();

            if (helper[0].Equals("entry"))
            {
                return new EntryNode(helper[1], helper.GetParameterNodeFromString(), subNodes);
            }

            if (helper[0].Equals("function"))
            {
                return new IntroduceFunctionNode(helper[1], helper.GetParameterNodeFromString(), subNodes);
            }

            if (helper[0].Equals("state"))
            {
                if (helper.Contains("["))
                {
                    return new IntroduceStateVariableNode(helper[1], helper[helper.Length - 2]);
                }

                return new IntroduceStateVariableNode(helper[1]);
            }

            if (helper[0].Equals("while"))
            {
                return new WhileLoopNode(new QuestionNode(helper.GetStringInBrackets()), subNodes);
            }

            if (helper[0].Equals("if"))
            {

                var question = helper.GetStringInBrackets();

                if (!treenode.SubLines.Select(x => x.Line).Contains("} else {"))
                    return new IfNode(new QuestionNode(question), subNodes);

                //ifelsenode
                var nullIndex = subNodes.FindIndex(x => x != null && x.GetType() == typeof(ElseNode));

                var beginningLast = nullIndex + 1;
                var lastCount = subNodes.Count - (beginningLast);

                return new IfElseNode(new QuestionNode(question), subNodes.GetRange(0, nullIndex),
                    subNodes.GetRange(beginningLast, lastCount));

            }

            //specialfunctions but they are not void so it should never compute!
            if (helper[0].Equals("_META") || helper[0].Equals("_DATA") || helper[0].Equals("_LENGTH"))
            {
                return new EmptyNode();
            }

            //void function calls!
            if (helper[1].Equals("(") && helper[helper.Length - 1].Equals(")"))
            {

                var parameters = helper.GetStringInBrackets().Split(new[]{','},StringSplitOptions.RemoveEmptyEntries);

                //specialfunction outtransaction
                if (helper[0].Equals("_OUT"))
                {
                    return new OutNode(new ExpressionNode(parameters[0]), new ExpressionNode(parameters[1]));
                }

                //simple functioncall
                var paraNodes = parameters.Select(x => new ExpressionNode(x)).Cast<Node>().ToList();
                return new FunctionCallNode(helper[0], paraNodes);
            }

            if (helper[0].Equals("}") && helper[1].Equals("else") && helper[2].Equals("{"))
            {
                return new ElseNode();
            }

            if (helper[0].Equals("return"))
            {
                var expNode = new ExpressionNode(helper.GetSubList(1));
                return new ReturnNode(expNode);
            }

            //we do all the new creations
            if (helper[0].Equals("var"))
            {

                if (helper.Contains("[") && !helper.Contains("="))
                {
                    return new IntroduceArrayNode(helper[1], helper[helper.IndexOf("[") + 1]);
                }

                return new IntroduceVariableNode(helper[1], new ExpressionNode(helper.GetSubList(helper.IndexOf("=") + 1)));

            }

            //we do all the assignments
            if (helper.Contains("="))
            {

                var expNode = new ExpressionNode(helper.GetSubList(helper.IndexOf("=") + 1));

                if (helper.Contains("["))
                {
                    return new ArrayNode(helper[0], helper[helper.IndexOf("[") + 1], expNode);
                }

                return new VariableNode(helper[0], expNode);

            }

            throw new Exception($"Line could not get parsed: {treenode.Line}");
        }
    }
}
