using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace StrainLanguage.Classes
{
    public static class Factory
    {
        public static Expression Copy(string args1, string result)
        {
            return new Expression(00, args1, result, result);
        }

        public static Expression Add(string args1, string args2, string result)
        {
            return new Expression(03, args1, args2, result);
        }

        public static Expression Label(string labenName)
        {
            return new Expression(05, labenName);
        }

        public static Expression Exit()
        {
            return new Expression(99);
        }

        public static Expression IntroduceValue(string args1, string destination)
        {
            return new Expression(01, args1, destination, destination);
        }

        public static Expression Goto(string destination)
        {
            return new Expression(13, destination);
        }
    }
}
