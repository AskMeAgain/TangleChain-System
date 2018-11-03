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

        public long GetValueAsLong()
        {
            return (long)value;
        }

        public string GetValueAsStringWithPrefix()
        {
            return "Int_" + value;
        }

        public ISCType Add(ISCType obj)
        {
            if (obj.GetType() == typeof(SC_Int))
            {
                return new SC_Int(value + obj.GetValueAsInt());
            }

            return new SC_String(value.ToString() + obj.GetValueAsString());

        }

        public ISCType Multiply(ISCType obj)
        {
            if (obj.GetType() == typeof(SC_Int))
            {
                return new SC_Int(value * obj.GetValueAsInt());
            }

            throw new ArgumentException("Sorry you cant multiply a string with an int");

        }

        public ISCType Subtract(ISCType obj)
        {
            throw new NotImplementedException();
        }

        public ISCType Divide(ISCType obj)
        {
            throw new NotImplementedException();
        }
    }
}
