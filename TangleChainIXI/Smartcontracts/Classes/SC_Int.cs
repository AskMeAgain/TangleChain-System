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

        public T GetValueAs<T>()
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public string GetValueAsStringWithPrefix()
        {
            return "Int_" + value;
        }

        public ISCType Add(ISCType obj)
        {
            if (obj.IsOfType<SC_Int>())
            {
                return new SC_Int(value + obj.GetValueAs<int>());
            }

            if (obj.IsOfType<SC_Long>())
            {
                return new SC_Long(value + obj.GetValueAs<long>());
            }

            return new SC_String(value.ToString() + obj.GetValueAs<string>());

        }

        public ISCType Multiply(ISCType obj)
        {
            if (obj.IsOfType<SC_Int>())
            {
                return new SC_Long(value * obj.GetValueAs<int>());
            }

            if (obj.IsOfType<SC_Long>())
            {
                return new SC_Long(value * obj.GetValueAs<long>());
            }

            throw new ArgumentException("Sorry you cant multiply a string with an int");

        }

        public ISCType Subtract(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                return new SC_Long(value - obj.GetValueAs<long>());
            }

            throw new ArgumentException($"Sorry you cant subtract {obj.GetType()} with an int");

        }

        public ISCType Divide(ISCType obj)
        {
            throw new ArgumentException("Sorry this may never be supported!");
        }
    }
}
