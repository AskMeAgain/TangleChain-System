using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace StrainTest
{
    public static class TestUtils
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

        public static List<ISCType> CheckRegisterCollection(this Computer comp, string name)
        {

            var keyList = comp.Register.Keys.ToList();

            var resultList = new List<ISCType>();

            foreach (var s in keyList)
            {
                if (s.EndsWith(name))
                {
                    resultList.Add(comp.Register.GetFromRegister(s));
                }
            }

            return resultList;
        }
    }
}
