using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public T GetValueAs<T>()
        {

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(value);
                }
                return default(T);
            }
            catch (NotSupportedException)
            {
                throw new ArgumentException($"Sorry you cant convert {value} to {typeof(T)}");
            }
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
            return new SC_String(value + obj.GetValueAs<string>());
        }

        public ISCType Multiply(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                string s = "";

                for (int i = 0; i < obj.GetValueAs<int>(); i++)
                {
                    s += value;
                }

                return new SC_String(s);
            }

            throw new ArgumentException("You cant multiply two strings together!");
        }

        public ISCType Subtract(ISCType obj)
        {
            throw new ArgumentException("You cant subtract from a string!");
        }

        public ISCType Divide(ISCType obj)
        {
            throw new ArgumentException("You cant divide from a string!");
        }
    }
}
