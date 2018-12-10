using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainLanguage.Classes
{
    public static class Utils
    {
        public static bool Matches(this string exp, string regex)
        {
            return Regex.Match(exp, regex).Success;
        }

        public static void Print(this TreeNode node, int order = 0)
        {

            if (node.Line.Equals("}"))
                order--;

            for (int i = 0; i < order; i++)
            {
                Console.Write("    ");
            }
            Console.WriteLine(node.Line);

            node.SubLines.ForEach(x => Print(x, order + 1));
        }

        public static string JumpContextUp(string context) {

            var index = context.LastIndexOf("-");

            return context.Substring(0, index);

        }

        public static ISCType CheckRegister(this Computer comp, string name) {

            var keyList = comp.Register.Keys.ToList();

            foreach (var s in keyList) {
                if (s.EndsWith(name)) {
                    return comp.Register.GetFromRegister(s);
                }
            }

            throw new Exception($"{name} does not exist!");
        }
    }
}
