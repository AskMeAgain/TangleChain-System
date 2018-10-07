using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TangleChainIXI.Smartcontracts
{
    public static class SmartcontractUtils
    {
        /// <summary>
        /// A function which allows to convert a flattened code to back to code again.
        /// Is the Reverse of Code.ToFlatString();
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Code Object</returns>
        public static Code StringToCode(string s)
        {
            Code code = new Code();

            s = s.Replace("\n", "");
            s = s.Replace("  ", " ");
            s = s.Replace(" ;", ";");
            var expArray = s.Split(';');

            foreach (string exp in expArray)
            {

                var repl = exp.Split(' ');

                //dirty
                if (repl[0].Contains("Name:"))
                {
                    code.AddVariable(repl[1], repl[3].Substring(2));

                }
                else if (repl.Count() == 2)
                {
                    code.AddExpression(new Expression(int.Parse(repl[0]), repl[1]));
                }
                else if (repl.Count() == 3)
                {
                    code.AddExpression(new Expression(int.Parse(repl[0]), repl[1], repl[2]));
                }
                else if (repl.Count() == 4)
                {
                    code.AddExpression(new Expression(int.Parse(repl[0]), repl[1], repl[2], repl[3]));
                }
            }

            return code;
        }
    }
}
