using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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

        public override T GetValueAs<T>()
        {

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(value);
                }

                throw new ArgumentException("Convertion failed");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override string GetValueAsStringWithPrefix()
        {
            return "Str_" + value;
        }

        public override ISCType Add(ISCType obj)
        {
            return new SC_String(value + obj.GetValueAs<string>());
        }

        public override ISCType Multiply(ISCType obj)
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

        public override ISCType Subtract(ISCType obj)
        {
            throw new NotSupportedException("You cant subtract from a string!");
        }

        public override ISCType Divide(ISCType obj)
        {
            throw new NotSupportedException("You cant divide from a string!");
        }

        public override bool IsEqual(ISCType obj)
        {
            return obj.IsOfType<SC_String>() && obj.GetValueAs<string>().Equals(value);
        }

        public override string ToString()
        {
            return "Str_" + value;
        }

        public override bool IsLower(ISCType obj)
        {
            throw new NotSupportedException("You cant use islower on a string");
        }
    }
}
