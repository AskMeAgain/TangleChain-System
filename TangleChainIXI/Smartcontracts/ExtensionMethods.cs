using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts {
    public static class ExtensionMethods {

        public static int ToInt(this string value) {

            bool flag = int.TryParse(value, out int result);

            if (!flag)
                throw new ArgumentException("Sorry but you cant convert this to int!");

            return result;

        }

        public static string _String(this int value) {
            return value + "";
        }

    }
}
