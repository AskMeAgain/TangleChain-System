using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;

namespace StrainLanguage.Classes
{
    internal class Parser
    {

        private LexerNode _treenode;

        public Parser(LexerNode node)
        {
            _treenode = node;
        }

        public ParserNode Parse(LexerNode treenode = null)
        {

            //means we started 
            if (treenode == null)
            {
                treenode = _treenode;

                //we put the whole thing in an appnode
                //we also parse each subline
                return new ApplicationNode(new ExpressionHelper(treenode.Line)[0], treenode.SubLines.Select(x => Parse(x)).ToList());
            }

            var helper = new ExpressionHelper(treenode.Line);
            var subNodes = treenode.SubLines.Select(x => Parse(x)).ToList();

            if (helper[0].Equals("entry"))
            {
                return new EntryNode(helper[1], helper.GetParameterNodeFromString(), subNodes);
            }

            if (helper[0].Equals("function"))
            {
                //all the parameters
                var list = helper.GetParameterNodeFromString();

                return new IntroduceFunctionNode(helper[1], list, subNodes);
            }

            if (helper[0].Equals("var"))
            {
                if (helper.Contains("[")) {
                    return new IntroduceStateVariableNode(helper[1], helper[helper.Length-2]);
                }

                return new IntroduceStateVariableNode(helper[1]);
            }

            if (helper[0].Equals("while"))
            {
                var expressionNode = new QuestionNode(helper.GetStringInBrackets());

                return new WhileLoopNode(expressionNode, subNodes);
            }

            if (helper[0].Equals("if"))
            {

                var question = helper.GetStringInBrackets();

                if (!treenode.SubLines.Select(x => x.Line).Contains("} else {"))
                    return new IfNode(new QuestionNode(question), subNodes);

                //ifelsenode
                var nullIndex = subNodes.FindIndex(x => x != null && x.GetType() == typeof(ElseNode));

                var beginningLast = nullIndex + 1;
                var firstCount = nullIndex;
                var lastCount = subNodes.Count - (beginningLast);

                return new IfElseNode(new QuestionNode(question), subNodes.GetRange(0, firstCount),
                    subNodes.GetRange(beginningLast, lastCount));

            }

            //void function calls!
            if (helper[1].Equals("(") && helper[helper.Length - 1].Equals(")"))
            {

                var parameters = helper.GetStringInBrackets().Split(',', StringSplitOptions.RemoveEmptyEntries);

                //specialfunction outtransaction
                if (helper[0].Equals("_OUT"))
                {
                    var n = new List<ParserNode>() { new ExpressionNode(parameters[0]),
                        new ExpressionNode(parameters[1]) };
                    return new OutNode(n);
                }

                //specialfunction arraylength
                if (helper[0].Equals("_LENGTH"))
                {
                    return new LengthNode(helper[helper.Length - 2]);
                }

                //functioncall
                var nn = parameters.Select(x => new ExpressionNode(x)).Cast<ParserNode>().ToList();
                return new FunctionCallNode(helper[0], nn);
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

            //we do all the assignments
            if (helper.Contains("="))
            {

                var index = helper.IndexOf("=");

                //means that we already used that variable eg: a = 3;
                if (!helper[0].Equals("intro"))
                {
                    var expNode = new ExpressionNode(helper.GetSubList(index + 1));

                    if (helper.Contains("["))
                    {
                        return new ArrayNode(helper[0], helper[helper.IndexOf("[") + 1], expNode);
                    }

                    return new VariableNode(helper[0], expNode);
                }
            }

            //we do all the new creations
            if (helper[0].Equals("intro"))
            {

                var index = helper.IndexOf("=");


                if (helper.Contains("[") && !helper.Contains("="))
                {

                    return new IntroduceArrayNode(helper[1], helper[helper.IndexOf("[") + 1]);
                }


                var expNode = new ExpressionNode(helper.GetSubList(index + 1));

                return new IntroduceVariableNode(helper[1], expNode);

            }

            throw new Exception($"Line could not get parsed: {treenode.Line}");
        }
    }
}
