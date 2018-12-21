using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;

namespace StrainLanguage
{
    public class Parser
    {

        private TreeNode _treenode;

        public Parser(TreeNode node)
        {
            _treenode = node;
        }

        public Node Parse(TreeNode treenode = null)
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
                return new EntryNode(helper[1], helper.GetParameterNodesFromFunctionCreation(),subNodes);
            }

            if (helper[0].Equals("function"))
            {
                //all the parameters
                var list = helper.GetParameterNodesFromFunctionCreation();

                return new IntroduceFunctionNode(helper[1], list, subNodes);
            }

            if (helper[0].Equals("var"))
            {
                return new StateVariableNode(helper[1]);
            }

            if (helper[0].Equals("while")) {
                ;
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
                ;

                var stringInBrackets = helper.GetStringInBrackets();
                var strings = stringInBrackets.Split(',', StringSplitOptions.RemoveEmptyEntries);

                //specialfunction outtransaction
                if (helper[0].StartsWith("_OUT"))
                {
                    var n = new List<Node>() { new ExpressionNode(strings[0]),
                        new ExpressionNode(strings[1]) };
                    return new OutNode("--", n);
                }

                //specialfunction arraylength
                if (helper[0].StartsWith("_LENGTH"))
                {
                    ;
                    return new LengthNode(helper[1]);
                }

                //functioncall
                var nn = strings.Select(x => new ExpressionNode(x)).Cast<Node>().ToList();
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

            return null;
        }

        private List<Node> ConvertStringToValues(string parameters)
        {

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
    }
}
