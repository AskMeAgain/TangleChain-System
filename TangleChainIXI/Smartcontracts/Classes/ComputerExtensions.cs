﻿using System;
using System.Collections.Generic;
using TangleChainIXI.Classes.Helper;

namespace TangleChainIXI.Smartcontracts.Classes
{
    public static class ComputerExtensions
    {

        /// <summary>
        /// Converts a string to an ISCType
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ISCType ConvertToInternalType(this string obj)
        {
            //we first check if they all have a prefix
            var maybe = obj.GetSCType();
            if (maybe.HasValue)
            {
                var type = maybe.Value;

                if (type == typeof(SC_Int)) return new SC_Int(obj.RemovePreFix<int>());

                if (type == typeof(SC_String)) return new SC_String(obj.RemovePreFix<string>());

                if (type == typeof(SC_Long)) return new SC_Long(obj.RemovePreFix<long>());
            }

            //we check now without prefix
            var flag = int.TryParse(obj, out int result);
            if (flag) return new SC_Int(result);

            flag = long.TryParse(obj, out long result2);
            if (flag) return new SC_Long(result);

            return new SC_String(obj);
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

        public static Maybe<Type> GetSCType(this string obj)
        {

            string[] arr = obj.Split('_');

            if (arr[0].Equals("Int"))
                return Maybe<Type>.Some(typeof(SC_Int));

            if (arr[0].Equals("Str"))
                return Maybe<Type>.Some(typeof(SC_String));

            if (arr[0].Equals("Lon"))
                return Maybe<Type>.Some(typeof(SC_Long));

            return Maybe<Type>.None;
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
            string[] arr = obj.Split(new[] { '_' }, 2);

            if (arr.Length == 2)
                return (T)Convert.ChangeType(arr[1], typeof(T));

            throw new ArgumentException($"ERROR CANT REMOVE PREFIX OF {arr.ToString()}");
        }
    }
}
