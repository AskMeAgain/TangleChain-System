using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts.Interfaces
{
    public abstract class ISCType
    {
        public abstract T GetValueAs<T>();

        public abstract string GetValueAsStringWithPrefix();

        public abstract ISCType Add(ISCType obj);
        public abstract ISCType Multiply(ISCType obj);
        public abstract ISCType Subtract(ISCType obj);
        public abstract ISCType Divide(ISCType obj);

        public abstract bool IsEqual(ISCType obj);
    }
}
