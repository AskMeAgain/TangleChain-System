using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts.Interfaces;

namespace TangleChainIXI.Smartcontracts.Classes
{
    public static class OperatorUtils
    {
        public static ISCType Add(ISCType obj1, ISCType obj2)
        {

            //we now the left one is an int
            if (obj1.GetType() == typeof(SC_Int))
            {

                if (obj2.GetType() == typeof(SC_Int))
                {
                    return (SC_Int)obj1 + (SC_Int)obj2;
                }
            }

            throw new ArgumentException("THIS SHOULD NOT HAPPEN... MISSING OPERATOR ADD!");

        }

        public static ISCType Multiply(ISCType obj1, ISCType obj2)
        {

            //we now the left one is an int
            if (obj1.GetType() == typeof(SC_Int))
            {

                //right one is int!
                if (obj2.GetType() == typeof(SC_Int))
                {
                    return (SC_Int)obj1 * (SC_Int)obj2;
                }
            }

            throw new ArgumentException("THIS SHOULD NOT HAPPEN... MISSING OPERATOR multiply!");

        }
    }
}
