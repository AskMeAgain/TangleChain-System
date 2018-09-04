using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts {
    public static class ExtensionMethods {

        public static int _Int(this string value) {

            value = value.Substring(2, value.Length - 2);

            bool flag = int.TryParse(value, out int result);

            if (!flag)
                throw new ArgumentException("Sorry but you cant convert this to int!");

            return result;

        }

        public static string _String(this int value) {
            return "__" + value;
        }

        public static string RemoveType(this string s) {
            return s.Substring(2);
        }

    }
}
