using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Parses the given string with any prefix (no prefix too) to an integer. Throws if parsing is not possible.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int _Int(this string value)
        {
            if (value[1].Equals("_"))
                value = value.Substring(2);

            bool flag = int.TryParse(value, out int result);

            if (!flag)
                throw new ArgumentException("Sorry but you cant convert this to int!");

            return result;

        }

        /// <summary>
        /// Parses the given integer value to a string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string _String(this int value)
        {
            return "__" + value;
        }

        /// <summary>
        /// Removes the prefix/type of a given string if possible.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveType(this string s)
        {
            if (s[1].Equals('_'))
                return s.Substring(2);
            return s;
        }

    }
}
