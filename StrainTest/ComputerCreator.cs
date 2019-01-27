using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainTest
{
    public static class ComputerCreator
    {
        public static Computer CreateComputer(List<Expression> expList, Dictionary<string, ISCType> varDict = null, IXISettings settings = null)
        {
            var smart = new Smartcontract("Foo", Utils.GenerateRandomString(81))
                .AddExpression(expList)
                .AddVariable(varDict)
                .SetFee(0);

            if (settings != null)
            {
                smart.Final(settings);
            }

            return new Computer(smart);
        }
    }
}
