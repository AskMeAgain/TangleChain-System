using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Smartcontracts
{
    [Serializable]
    public class Code
    {

        public Dictionary<string, ISCType> Variables { set; get; }
        public List<Expression> Expressions { set; get; }

        /// <summary>
        /// Standard constructor for JSON converter
        /// </summary>
        public Code()
        {
            Expressions = new List<Expression>();
            Variables = new Dictionary<string, ISCType>();
        }


        /// <summary>
        /// Converts the code to string. Will include formatting like "\n"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            string s = "";

            Expressions.ForEach(exp => s += exp.ToString());

            if (Variables != null)
                Variables.Keys.ToList().ForEach(x => s += "Name: " + x + " " + Variables[x].ToString() + ";\n");

            return s;
        }

        /// <summary>
        /// The reduced string. Use this if you want to store the code in a DB. 
        /// </summary>
        /// <returns></returns>
        public string ToFlatString()
        {
            string s = ToString();

            return s.Replace("  ", " ").Replace(" ;", ";").Replace("\n", "");

        }

        /// <summary>
        /// Equality comparer for Code
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Code code = obj as Code;

            if (code == null)
                return false;

            if (code.ToFlatString().Equals(ToFlatString()))
                return true;
            return false;

        }

    }
}
