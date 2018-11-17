using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Strain.Classes;
using TangleChainIXI.Smartcontracts;

namespace Strain
{
    public class Strain
    {

        private string _code;

        public Strain(string code)
        {
            _code = code;
        }

        public TreeNode Lexing()
        {
            //remove linebreakers
            var removedCode = _code.Replace("\n", "").Replace(@"\k", "");

            //split on ;{}
            var list = Regex.Split(removedCode, @"(?<=;|}|else?{|{)").ToList();

            //now "else" is wrongly splitted we need to put it in correct formatting
            var correctedList = new List<string>();
            foreach (var line in list)
            {

                //we need to push else into the line before
                if (line.Equals("else{"))
                {
                    correctedList[correctedList.Count - 1] += line;
                }
                else
                {
                    correctedList.Add(line);
                }
            }

            //replace empty stuff and remove empty stuff
            list = correctedList.Select(x => x.Replace("  ", " ").Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();

            //remove all comments
            list.RemoveAll(x => x.StartsWith("//"));

            //now we sort them into "order numbers"
            int currentPointer = 0;
            List<(int order, string exp)> orderList = new List<(int, string)>();
            list.ForEach(x =>
            {

                orderList.Add((currentPointer, x));

                if (x.Contains('{')) currentPointer++;
                if (x.Contains('}')) currentPointer--;

                if (currentPointer < 0) throw new ArgumentException("Wrong  order of {}");
            });

            if (currentPointer > 0) throw new ArgumentException("Wrong number of {}");

            //the stack
            List<TreeNode> stack = new List<TreeNode>();

            stack.Add(new TreeNode()
            {
                Order = -1,
                Line = "Application"
            });

            int i = 1;
            for (; i < orderList.Count; i++)
            {

                if (orderList[i].order > orderList[i - 1].order)
                {

                    //collapse now
                    Collapse(stack, orderList, i);

                    //push on stack
                    stack.Insert(0, stack[0].SubLines.Last());
                }

                if (orderList[i].order == orderList[i - 1].order)
                {

                    //collapse
                    Collapse(stack, orderList, i);

                }

                if (orderList[i].order < orderList[i - 1].order)
                {

                    //collapse
                    Collapse(stack, orderList, i);

                    //pop until order < order
                    while (stack[0].Order >= orderList[i].order)
                    {

                        stack.RemoveAt(0);
                    }
                }
            }

            //we need to collapse the last one too
            Collapse(stack, orderList, i);

            return stack.Last();

        }

        private static void Collapse(List<TreeNode> stack, List<(int order, string exp)> orderList, int i)
        {
            stack[0].SubLines.Add(new TreeNode()
            {
                Order = orderList[i - 1].order,
                Line = orderList[i - 1].exp
            });
        }

        public class TreeNode
        {
            public int Order { get; set; }
            public string Line { get; set; }
            public List<TreeNode> SubLines { get; set; } = new List<TreeNode>();
        }

        public Node Parse(TreeNode treenode)
        {

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
                return new Node(nodeList.ToArray());
            }

            if (treenode.Line.Contains("="))
            {
                //int foo = 3 + 4;
                if (helper.Length == 6)
                {

                    //we now convert to value nodes
                    var node1 = new ValueNode(helper[3]);
                    var node2 = new ValueNode(helper[5]);

                    var operationNode = new OperationNode(helper[4], node1, node2);

                    return new AssignNode(helper[0], helper[1], operationNode);
                }

                //int foo = 4;
                if (helper.Length == 4)
                {
                    return new AssignNode(helper[0], helper[1], new ValueNode(helper.Last()));
                }
            }

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
