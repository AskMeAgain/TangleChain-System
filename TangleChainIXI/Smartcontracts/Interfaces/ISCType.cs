using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts.Interfaces
{
    public interface ISCType
    {
        T GetValueAs<T>();

        string GetValueAsStringWithPrefix();

        ISCType Add(ISCType obj);
        ISCType Multiply(ISCType obj);
        ISCType Subtract(ISCType obj);
        ISCType Divide(ISCType obj);
    }
}
