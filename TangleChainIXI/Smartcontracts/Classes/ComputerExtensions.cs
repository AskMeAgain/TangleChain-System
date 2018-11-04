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

            if (obj.GetSCType() == typeof(SC_Long))
                return new SC_Long(obj.RemovePreFix<long>());

            throw new ArgumentException($"ERROR YOU CANT CONVERT {obj.ToString()} TO INTERNAL TYPE!");

        }

        public static ISCType GetFromRegister(this Dictionary<string, ISCType> register, string name)
        {
            if (register.ContainsKey(name))
            {
                return register[name];
            }

            throw new ArgumentException($"{name} DOESNT EXIST IN REGISTER!");
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

            if (arr[0].Equals("Lon"))
                return typeof(SC_Long);

            return null;
        }

        public static bool IsOfType<T>(this ISCType obj) where T : ISCType
        {
            return obj.GetType() == typeof(T);
        }

        public static bool IsOfType<T1, T2>(this ISCType obj) where T1 : ISCType where T2 : ISCType
        {
            return (obj.GetType() == typeof(T1) || obj.GetType() == typeof(T2));
        }

        public static T RemovePreFix<T>(this string obj)
        {
            string[] arr = obj.Split('_');

            if (arr.Length == 2)
                return (T)Convert.ChangeType(arr[1], typeof(T));

            throw new ArgumentException($"ERROR CANT REMOVE PREFIX OF {arr}");
        }
    }
}
