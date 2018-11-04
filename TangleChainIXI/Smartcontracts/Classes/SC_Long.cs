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
            throw new NotImplementedException();
        }

        public ISCType Multiply(ISCType obj)
        {
            throw new NotImplementedException();
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
