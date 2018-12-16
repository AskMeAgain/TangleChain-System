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
                return new EntryNode(helper[1], subNodes);
            }

            if (helper[0].Equals("function"))
            {
                //all the parameters
                var list = helper.GetParameterNodes();

                return new IntroduceFunctionNode(helper[1], list, subNodes);
            }

            if (helper[0].Equals("var"))
            {
                return new StateVariableNode(helper[1]);
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

            if (helper[1].Equals("(")  && helper[helper.Length-1].Equals(")"))
            {
                
                //functioncall
                return new FunctionCallNode(helper.ToString());

            }

            if (helper[0].Equals("}") && helper[1].Equals("else") && helper[2].Equals("{"))
            {
                return new ElseNode();
            }

            if (helper.IndexOf("=") > -1)
            {

                var index = helper.IndexOf("=");

                //means that we already used that variable eg: a = 3;
                if (index == 1)
                {
                    var expNode = new ExpressionNode(helper.GetSubList(2));

                    return new VariableNode(helper[0], expNode);
                }

                //means that we create new variable eg: int a = 3;
                if (index == 2)
                {
                    var expNode = new ExpressionNode(helper.GetSubList(3));

                    return new IntroduceVariableNode(helper[1], expNode);
                }

            }

            ;
            return null;
        }
    }
}
