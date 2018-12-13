using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StrainLanguage.Classes;
using StrainLanguage.NodeClasses;
using TangleChainIXI.Smartcontracts;

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

        public TreeNode Lexing(string code)
        {
            return new Lexer(code).Lexing();
        }

        public Node Parse(TreeNode node)
        {
            return new Parser(node).Parse();
        }
    }
}
