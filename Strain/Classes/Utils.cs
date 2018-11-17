using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Strain.Classes
{
    public static class Utils
    {
        public static bool Matches(this string exp, string regex)
        {
            return Regex.Match(exp, regex).Success;
        }

        public static void Print(this Strain.TreeNode node, int order = 0)
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
    }
}
