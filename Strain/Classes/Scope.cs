using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrainLanguage.Classes
{
    public class Scope
    {
        private Dictionary<string, List<string>> Scopes { get; set; }
        private Dictionary<string, List<string>> FunctionParameters { get; set; }

        public List<string> StateVariables { get; set; }

        public Scope()
        {
            Scopes = new Dictionary<string, List<string>>();
            FunctionParameters = new Dictionary<string, List<string>>();
            StateVariables = new List<string>();
        }

        public void AddVariable(string name, string context)
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

        public string GetHighestContext(string name, string currentContext)
        {

            if (!Scopes.ContainsKey(name)) throw new ArgumentException($"Sorry but {name} does not exist in the current context");

            var num = currentContext.Count(x => x == '-');
            
            for (int i = 0; i <= num; i++)
            {
                
                if (Scopes[name].Contains(currentContext))
                {
                    return currentContext;
                }

                if (i != num)
                    currentContext = Utils.JumpContextUp(currentContext);

            }

            throw new ArgumentException($"{name} is not in scope");

        }

        public List<string> GetFunctionParameter(string name)
        {
            if (FunctionParameters.Count == 0) return new List<string>();
            return FunctionParameters[name].ToList();
        }

        public void AddFunctionParameter(string parameterName, string functionName) {
            ;
            if (FunctionParameters.ContainsKey(functionName))
            {
                FunctionParameters[functionName].Add(parameterName);
            }
            else
            {
                FunctionParameters.Add(functionName, new List<string>() { parameterName });
            }

        }
    }
}
