using System;
using System.Collections.Generic;
using System.Text;
using Strain.Classes;

namespace Strain
{
    public class Lexer
    {

        private string _code;

        public Lexer(string code)
        {
            _code = code;
        }

        public Node Lexing()
        {

            return new Node();
        }
    }
}
