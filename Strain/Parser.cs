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
        private string _appName;

        public Parser(string ApplicationName, TreeNode node)
        {
            _treenode = node;
            _appName = ApplicationName;
        }

        public Node Parse(TreeNode treenode = null)
        {
            //means we started with this
            if (treenode == null)
            {
                treenode = _treenode;
            }

            var helper = new ExpressionHelper(treenode.Line);
            var nodeList = new List<Node>();

            treenode.SubLines.ForEach(x =>
            {
                if (x.Line.Equals("}") || x.Line.Equals("}else{"))
                {
                    //damn breaking my head
                }
                else
                {
                    nodeList.Add(Parse(x));
                }
            });

            if (treenode.Line.Matches("^function"))
            {
                return new FunctionNode(helper[1], nodeList.ToArray());
            }

            if (treenode.Line.Equals("Application"))
            {
                return new ApplicationNode(_appName, nodeList.ToArray());
            }

            if (treenode.Line.Contains("="))
            {
                //int foo = 3 + 4;
                if (helper.Length == 6)
                {

                    //we now convert to value nodes
                    var node1 = new ValueNode(helper.Type(3), helper[3]);
                    var node2 = new ValueNode(helper.Type(5), helper[5]);

                    var operationNode = new OperationNode(helper[4], node1, node2);

                    return new AssignNode(helper[0], helper[1], operationNode);
                }

                //int foo = 4;
                if (helper.Length == 4)
                {
                    return new AssignNode(helper[0], helper[1], new ValueNode(helper.Type(helper.Length - 1), helper.Last()));
                }
            }

            //create the if nodes
            if (treenode.Line.Matches("^if"))
            {

                var indexOfElse = treenode.SubLines.Select(x => x.Line).ToList().FindIndex(0, x => x.Equals("}else{"));

                //its just if
                if (indexOfElse == -1)
                {
                    var funcNode = new FunctionNode("function" + treenode.Line.GetHashCode(),
                        treenode.SubLines.Where(x => !x.Line.Equals("}")).Select(x => Parse(x)).ToArray());

                    return new IfNode(treenode.Line, funcNode);
                }

                //we now get the first half
                var ifTrue = treenode.SubLines.GetRange(0, indexOfElse);

                //the else part
                var elsePart = treenode.SubLines.GetRange(indexOfElse + 1, treenode.SubLines.Count - (indexOfElse + 1));

                var trueNode = new FunctionNode("function" + treenode.Line.GetHashCode(), ifTrue.Select(x => Parse(x)).ToArray());
                var elseNode = new FunctionNode("function" + treenode.Line.GetHashCode(), elsePart.Where(x => !x.Line.Equals("}")).Select(x => Parse(x)).ToArray());

                return new IfElseNode(treenode.Line, trueNode, elseNode);

            }

            throw new ArgumentException("lol");

        }
    }
}
