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
                return new ApplicationNode(new ExpressionHelper(treenode.Line)[0], treenode.SubLines.Select(x => Parse(x)).ToArray());
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
                List<ParameterNode> list = helper.GetParameters();

                return new FunctionNode(helper[1], list, subNodes);
            }

            if (helper[0].Equals("var"))
            {
                return new StateVariableNode(helper[1], subNodes);
            }

            if (helper[0].Equals("if"))
            {

                var question = helper.GetQuestion();

                if (!treenode.SubLines.Select(x => x.Line).Contains("}else{"))
                    return new IfNode(question, subNodes);
                ;
                //ifelsenode
                var nullIndex = subNodes.FindIndex(x => x != null && x.GetType() == typeof(ElseNode));

                var beginningLast = nullIndex + 1;
                var firstCount = nullIndex;
                var lastCount = subNodes.Count - (beginningLast);

                return new IfElseNode(question, subNodes.GetRange(0, firstCount),
                    subNodes.GetRange(beginningLast, lastCount));

            }

            if (helper[0].Equals("}else{"))
            {
                return new ElseNode();
            }

            return null;

        }
    }
}
