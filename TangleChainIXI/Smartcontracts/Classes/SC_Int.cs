using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts.Interfaces;

namespace TangleChainIXI.Smartcontracts.Classes
{
    public class SC_Int : ISCType
    {

        private int value;

        public SC_Int(string s)
        {

            bool flag = int.TryParse(s, out int result);

            if (flag)
                value = result;
            else
                throw new ArgumentException("CANT CONVERT TO INT!");

        }

        public SC_Int(int i)
        {
            value = i;
        }

        public SC_Int()
        {
            value = 0;
        }

        public static SC_Int operator +(SC_Int obj, SC_Int obj2)
        {
            return new SC_Int(obj.value + obj2.value);
        }

        public static SC_Int operator *(SC_Int obj, SC_Int obj2)
        {
            return new SC_Int(obj.value * obj2.value);
        }

        public string GetValueAsString()
        {
            return value + "";
        }

        public int GetValueAsInt()
        {
            return value;
        }
    }
}
