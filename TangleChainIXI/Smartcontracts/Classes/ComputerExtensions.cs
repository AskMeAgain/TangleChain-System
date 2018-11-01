using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts.Classes;
using TangleChainIXI.Smartcontracts.Interfaces;

namespace TangleChainIXI.Smartcontracts
{
    public static class ComputerExtensions
    {

        public static ISCType ConvertToInternalType(this string obj)
        {

            if (obj.GetSCType() == typeof(SC_Int))
                return new SC_Int(obj.RemovePreFix<int>());

            if (obj.GetSCType() == typeof(SC_String))
                return new SC_String(obj.RemovePreFix<string>());

            throw new ArgumentException("ERROR YOU CANT CONVERT THIS TO INTERNAL TYPE!");

        }

        public static ISCType GetFromRegister(this Dictionary<string, ISCType> register, string name)
        {
            if (register.ContainsKey(name))
            {
                return register[name];
            }

            throw new ArgumentException("THIS ITEM DOESNT EXIST IN REGISTER!");
        }

        public static void AddToRegister(this Dictionary<string, ISCType> register, string name, ISCType obj)
        {
            if (register.ContainsKey(name))
                register[name] = obj;
            else
                register.Add(name, obj);
        }

        public static Type GetSCType(this string obj)
        {

            string[] arr = obj.Split('_');

            if (arr[0].Equals("Int"))
                return typeof(SC_Int);

            if (arr[0].Equals("Str"))
                return typeof(SC_String);

            return null;
        }

        public static T RemovePreFix<T>(this string obj)
        {
            string[] arr = obj.Split('_');
            return (T)Convert.ChangeType(arr[0], typeof(T));
        }
    }
}
