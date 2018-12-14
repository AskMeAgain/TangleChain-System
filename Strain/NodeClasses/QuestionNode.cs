using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.Classes;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.NodeClasses
{
    public class QuestionNode : Node
    {
        public string Question { get; protected set; }

        private Stack<string> _operationStack = new Stack<string>();
        private Stack<Node> _assertionStack = new Stack<Node>();


        public QuestionNode(string question)
        {
            Question = question;

            Nodes.Add(ConstructNodeFromQuestion(question));
        }

        public override List<Expression> Compile(Scope scope,ParserContext context)
        {
            return Nodes.SelectMany(x => x.Compile(scope,context)).ToList();
        }

        public Node ConstructNodeFromQuestion(string question)
        {

            var array = Regex.Split(question, @"((?<=\|\||&&)|(?=\|\||&&))").ToList();
            array.RemoveAll(x => x.Equals(""));

            for (int i = 0; i < array.Count; i++)
            {

                if (array[i].Equals("&&") || array[i].Equals("||"))
                {
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
                _assertionStack.Push(new AndNode(nodeLeft, nodeRight));
            }
            else
            {
                _assertionStack.Push(new OrNode(nodeLeft, nodeRight));
            }
        }

        private Node CreateAssertionFromString(string assertion)
        {
            var helper = new ExpressionHelper(assertion);

            if (helper.Length != 3) throw new Exception("Assertion is not correct!");

            var left = ConvertValueToNode(helper[0]);
            var right = ConvertValueToNode(helper[2]);

            if (helper[1].Equals("=="))
            {
                return new EqualNode(left, right);
            }

            if (helper[1].Equals("!="))
            {
                return new NotEqualNode(left, right);
            }

            throw new NotImplementedException("Other assertions are not implemented");
        }

        private Node ConvertValueToNode(string para)
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

            return new VariableNode(para);
        }
    }
}
