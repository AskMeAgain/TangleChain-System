using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Smartcontracts.Interfaces
{
    public interface ISCType
    {
        string GetValueAsString();
        int GetValueAsInt();
        long GetValueAsLong();

        string GetValueAsStringWithPrefix();

        ISCType Add(ISCType obj);
        ISCType Multiply(ISCType obj);
        ISCType Subtract(ISCType obj);
        ISCType Divide(ISCType obj);
    }
}
