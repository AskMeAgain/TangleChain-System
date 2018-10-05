﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace TangleChainIXI.Smartcontracts
{
    [Serializable]
    public class Code
    {

        public List<Variable> Variables { set; get; }
        public List<Expression> Expressions { set; get; }

        /// <summary>
        /// Standard constructor for JSON converter
        /// </summary>
        public Code()
        {
            Expressions = new List<Expression>();
            Variables = new List<Variable>();
        }

        /// <summary>
        /// Adds an Expression to the Code.
        /// </summary>
        /// <param name="exp"></param>
        public void AddExpression(Expression exp)
        {
            Expressions.Add(exp);
        }

        /// <summary>
        /// Adds a statevariable to the code. If you want persistent storage, you need to set these vars
        /// </summary>
        /// <param name="name">The name of the State. Internally will always have "S_" prefix</param>
        /// <param name="value">The startvalue</param>
        public void AddVariable(string name, string value = "0")
        {
            Variables.Add(new Variable("S_"+name, value));
        }

        /// <summary>
        /// Converts the code to string. Will include formatting like "\n"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            string s = "";

            Expressions.ForEach(exp => s += exp.ToString());

            s += "\n";

            if (Variables != null)
                Variables.ForEach(v => s += v.ToString() + ";\n");

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

            if (code.ToFlatString().Equals(this.ToFlatString()))
                return true;
            return false;

        }

    }
}
