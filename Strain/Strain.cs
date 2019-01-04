using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainLanguage
{
    public class Strain
    {

        private string _code;

        public Strain(string code)
        {
            _code = code;
        }

        public List<Expression> Compile()
        {

            //we first take the code and lex it
            var lexedCode = Lexing(_code);

            //we then parse it into nodes
            var parsedCode = Parse(lexedCode);

            //we then compile the code into expressions
            var expList = parsedCode.Compile();

            return expList;

        }

        public Smartcontract GenerateSmartcontract(string sendTo)
        {

            //we first take the code and lex it
            var lexedCode = Lexing(_code);

            var name = lexedCode.Line.Split(' ')[0];

            var smart = new Smartcontract(name, sendTo);

            var list = new List<string>();
            SearchForNode(list, lexedCode);

            list.SelectMany(x => {

                var helper = new ExpressionHelper(x);

                if (helper.Contains("[")) {
                    var arrayList = new List<string>();
                    for (int i = 0; i < int.Parse(helper[helper.Length - 2]); i++) {
                        arrayList.Add(helper[1] + "_" + i);
                    }

                    return arrayList;
                }
                else {
                    return new List<string>() {
                        helper[1]
                    };
                }
            }).ToList().ForEach(x => smart.AddVariable(x, new SC_Int()));

            return smart.AddExpression(Parse(lexedCode).Compile()).Final();

        }

        public void SearchForNode(List<string> list, LexerNode node)
        {

            if (node.Line.StartsWith("state"))
            {
                list.Add(node.Line);
            }
            else
            {
                if (node.SubLines.Count > 0)
                {
                    node.SubLines.ForEach(x => SearchForNode(list, x));
                }
            }
        }

        public LexerNode Lexing(string code)
        {
            return new Lexer(code).Lexing();
        }

        public Node Parse(LexerNode node)
        {
            return new Parser(node).Parse();
        }
    }
}
