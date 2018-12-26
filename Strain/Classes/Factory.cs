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
            return new Expression(28, labenName);
        }

        public static Expression Entry(string entryName)
        {
            return new Expression(05, entryName);
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

        public static Expression BranchIfOne(string destination, string args1)
        {
            return new Expression(21, destination, args1, args1);
        }

        public static Expression SetState(string source, string destination)
        {
            return new Expression(06, source, destination, destination);
        }

        public static Expression CopyState(string nameOfVar, string destination)
        {
            return new Expression(10, nameOfVar, destination, destination);
        }

        public static Expression Negate(string args1)
        {
            return new Expression(26, args1, args1, args1);
        }

        public static Expression JumpAndLink(string destination)
        {
            return new Expression(19, destination, destination, destination);
        }

        public static Expression IntroduceData(string index, string destination)
        {
            return new Expression(15, index, destination, destination);
        }

        public static Expression SetOutTransaction(string receiver, string amount)
        {
            return new Expression(09, receiver, amount);
        }

        public static Expression PopAndJump()
        {
            return new Expression(20);
        }
    }
}
