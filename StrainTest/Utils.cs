using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainTest
{
    public static class Utils
    {
        public static ISCType CheckRegister(this Computer comp, string name)
        {

            var keyList = comp.Register.Keys.ToList();

            foreach (var s in keyList)
            {
                if (s.EndsWith(name))
                {
                    return comp.Register.GetFromRegister(s);
                }
            }

            throw new Exception($"{name} does not exist!");
        }
    }
}
