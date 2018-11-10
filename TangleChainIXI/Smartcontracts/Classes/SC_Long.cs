using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts.Classes
{
    public class SC_Long : ISCType
    {
        private long value;

        public SC_Long()
        {
            value = 0;
        }

        public SC_Long(long s)
        {
            value = s;
        }

        public override T GetValueAs<T>()
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public override string GetValueAsStringWithPrefix()
        {
            return "Str_" + value;
        }

        public override ISCType Add(ISCType obj)
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

        public override ISCType Multiply(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                return new SC_Long(value * obj.GetValueAs<long>());
            }

            throw new ArgumentException($"you cant multiply {obj.GetType()} to a long");
        }

        public override ISCType Subtract(ISCType obj)
        {
            if (obj.IsOfType<SC_Int, SC_Long>())
            {
                return new SC_Long(value - obj.GetValueAs<long>());
            }

            throw new ArgumentException($"you cant subtract {obj.GetType()} to a long");
        }

        public override ISCType Divide(ISCType obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsEqual(ISCType obj)
        {
            return obj.IsOfType<SC_Long, SC_Int>() && obj.GetValueAs<long>() == value;
        }

        public override string ToString()
        {
            return "Lon_" + value;
        }
    }
}
