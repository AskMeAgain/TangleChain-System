using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : ParserNode
    {
        public string Question { get; protected set; }

        private Stack<string> _operationStack = new Stack<string>();
        private Stack<ParserNode> _assertionStack = new Stack<ParserNode>();


        public QuestionNode(string question)
        {
            ;
            Question = question.Replace(" [ ", "[").Replace(" ]", "]");

            Nodes.Add(ConstructNodeFromQuestion(Question));
        }

        public override List<Expression> Compile(Scope scope, ParserContext context)
        {
            return Nodes.SelectMany(x => x.Compile(scope, context)).ToList();
        }

        public ParserNode ConstructNodeFromQuestion(string question)
        {

            var array = Regex.Split(question, @"((?<=\|\||&&)|(?=\|\||&&))").ToList();
            array.RemoveAll(x => x.Equals(""));

            for (int i = 0; i < array.Count; i++)
            {

                if (array[i].Equals("&&") || array[i].Equals("||"))
                {
                    ;
                    _operationStack.Push(array[i]);
                }
                else
                {
                    _assertionStack.Push(CreateAssertionFromString(array[i]));
                }

                if (_operationStack.Count == 1 && _assertionStack.Count == 2)
                {
                    Reduce();
                }

            }

            return _assertionStack.Pop();
        }

        private void Reduce()
        {

            var nodeRight = _assertionStack.Pop();
            var nodeLeft = _assertionStack.Pop();

            var opNodeString = _operationStack.Pop();

            if (opNodeString.Equals("&&"))
            {
                ;
                _assertionStack.Push(new AndNode(nodeRight, nodeLeft));
            }
            else
            {
                _assertionStack.Push(new OrNode(nodeRight, nodeLeft));
            }
        }

        private ParserNode CreateAssertionFromString(string assertion)
        {
            var helper = new ExpressionHelper(assertion.Trim());
            ;
            if (helper.Length != 3) throw new Exception("Assertion is not correct!");

            var left = ConvertValueToNode(helper[0]);
            var right = ConvertValueToNode(helper[2]);

            if (helper[1].Equals("=="))
            {
                return new EqualNode(left, right);
            }

            if (helper[1].Equals("!="))
            {
                return new NegateNode(new EqualNode(left, right));
            }

            if (helper[1].Equals("<"))
            {
                return new SmallerNode(left, right);
            }

            if (helper[1].Equals(">"))
            {
                //bigger is smaller bug reversed order of parameter
                return new SmallerNode(right, left);
            }

            if (helper[1].Equals("<="))
            {
                //smaller equal is equal to Nage(Bigger()), which is equal to negate(smaller(reverse order));
                return new NegateNode(new SmallerNode(right, left));
            }

            if (helper[1].Equals(">="))
            {
                //we just use the same node but reverse the things
                return new NegateNode(new SmallerNode(left, right));
            }

            throw new NotImplementedException("Other assertions are not implemented");
        }

        private ParserNode ConvertValueToNode(string para)
        {

            if (para.StartsWith('"') && para.EndsWith('"'))
            {
                return new ValueNode(para);
            }

            var isInteger = int.TryParse(para, out int result1);
            if (isInteger)
            {
                return new ValueNode(para);
            }

            var isLong = long.TryParse(para, out long result2);
            if (isLong)
            {
                return new ValueNode(para);
            }

            if (para.Contains("["))
            {

                var name = para.Substring(0, para.IndexOf("["));
                var index = para.Substring(para.IndexOf("[") + 1, para.Length - para.IndexOf("[") - 2);
                ;
                return new ArrayNode(name, index);
            }

            return new VariableNode(para);
        }
    }
}
