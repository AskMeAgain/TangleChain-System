using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts.Interfaces;

namespace TangleChainIXI.Smartcontracts.Classes
{
    public class SC_Long : ISCType
    {
        private long value;

        public SC_Long(long s)
        {
            value = s;
        }

        public T GetValueAs<T>()
        {
            return (T)(object)value;
        }

        public string GetValueAsStringWithPrefix()
        {
            return "Str_" + value;
        }

        public ISCType Add(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                return new SC_Long(value + obj.GetValueAs<long>());
            }

            if (obj.IsOfType<SC_String>())
            {
                return new SC_String(value + obj.GetValueAs<string>());
            }

            throw new ArgumentException($"you cant add {obj.GetType()} to a long");
        }

        public ISCType Multiply(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                return new SC_Long(value * obj.GetValueAs<long>());
            }

            throw new ArgumentException($"you cant multiply {obj.GetType()} to a long");
        }

        public ISCType Subtract(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                return new SC_Long(value - obj.GetValueAs<long>());
            }

            throw new ArgumentException($"you cant subtract {obj.GetType()} to a long");
        }

        public ISCType Divide(ISCType obj)
        {
            throw new NotImplementedException();
        }
    }
}
