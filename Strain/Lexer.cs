using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.Classes;

namespace StrainLanguage
{
    public class Lexer
    {

        private string _code;

        public Lexer(string code)
        {
            _code = code;
        }

        public TreeNode Lexing()
        {

            //we get the name
            var name = _code.Substring(0, _code.IndexOf("{"));

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

            //we remove the first and last in the list
            list.RemoveAt(0);
            list.RemoveAt(list.Count - 1);

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

            //we remove empty }}
            orderList.RemoveAll(x => x.exp.Equals("}"));

            //the stack
            Stack<TreeNode> stack = new Stack<TreeNode>();

            stack.Push(new TreeNode()
            {
                Order = -1,
                Line = name
            });

            int i = 1;
            for (; i < orderList.Count; i++)
            {

                if (orderList[i].order > orderList[i - 1].order)
                {

                    //collapse now
                    Collapse(stack, orderList, i);

                    //push on stack
                    stack.Push(stack.Peek().SubLines.Last());
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
                    while (stack.Peek().Order >= orderList[i].order)
                    {
                        stack.Pop();
                    }
                }
            }

            //we need to collapse the last one too
            Collapse(stack, orderList, i);

            return stack.Last();

        }

        private static void Collapse(Stack<TreeNode> stack, List<(int order, string exp)> orderList, int i)
        {
            stack.Peek().SubLines.Add(new TreeNode()
            {
                Order = orderList[i - 1].order,
                Line = orderList[i - 1].exp
            });
        }
    }
}
