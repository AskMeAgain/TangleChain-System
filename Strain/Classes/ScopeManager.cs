using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.Classes
{
    public static class ScopeManager
    {
        private static Dictionary<string, List<string>> Scopes = new Dictionary<string, List<string>>();
        public static List<string> StateVariables = new List<string>();

        public static void AddVariable(string name, string context)
        {

            if (Scopes.ContainsKey(name))
            {
                Scopes[name].Add(context);
            }
            else
            {
                Scopes.Add(name, new List<string>() { context });
            }
        }

        public static string GetHighestContext(string name, string currentContext)
        {
            ;
            if (!Scopes.ContainsKey(name)) throw new ArgumentException($"Sorry but {name} does not exist in the current context");

            var num = currentContext.Count(x => x == '-');
            ;
            for (int i = 0; i <= num; i++)
            {
                ;
                if (Scopes[name].Contains(currentContext))
                {
                    return currentContext;
                }

                if (i != num)
                    currentContext = Utils.JumpContextUp(currentContext);

            }

            ;
            throw new ArgumentException($"{name} is not in scope");

        }
    }
}
