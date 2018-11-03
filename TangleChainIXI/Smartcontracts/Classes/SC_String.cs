using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts.Interfaces;

namespace TangleChainIXI.Smartcontracts.Classes
{
    public class SC_String : ISCType
    {
        public string value { get; set; }

        public SC_String(string s)
        {
            value = s;
        }

        public SC_String()
        {
            value = "";
        }

        public static SC_String operator +(SC_String obj, SC_String obj2)
        {
            return new SC_String(obj.value + obj2.value);
        }

        public static SC_String operator *(SC_String obj, SC_String obj2)
        {
            throw new ArgumentException("YOU CANT MULTIPLY STRINGS!");

        }

        public string GetValueAsString()
        {
            return value + "";
        }

        public int GetValueAsInt()
        {

            var flag = int.TryParse(value, out int result);

            if (flag)
                return result;

            throw new ArgumentException($"Sorry you cant convert {value} to int");
        }

        public long GetValueAsLong()
        {
            var flag = long.TryParse(value, out long result);

            if (flag)
                return result;

            throw new ArgumentException($"Sorry you cant convert {value} to long");
        }

        public string GetValueAsStringWithPrefix()
        {
            return "Str_" + value;
        }

        public ISCType Add(ISCType obj)
        {
            return new SC_String(value.ToString() + obj.GetValueAsString());
        }

        public ISCType Multiply(ISCType obj)
        {
            if (obj.GetType() == typeof(SC_Int))
            {
                string s = "";

                for (int i = 0; i < obj.GetValueAsInt(); i++)
                {
                    s += value;
                }

                return new SC_String(s);
            }

            throw new ArgumentException("You cant multiply two strings together!");
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
