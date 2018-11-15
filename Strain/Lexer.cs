using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public List<string> Lexing()
        {

            var removedCode = _code.Replace("\n", "").Replace(@"\k", "");

            var list = Regex.Split(removedCode, @"(?<=[{};])").ToList();

            return list.Select(x => x.Replace("  ", " ").Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();

        }
    }
}
