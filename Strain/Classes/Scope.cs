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
        public List<(string,string)> FunctionNames { get; set; }

        public Dictionary<string, int> ArrayIndex { get; set; }

        public List<string> StateVariables { get; set; }

        public Scope()
        {
            Scopes = new Dictionary<string, List<string>>();
            FunctionParameters = new Dictionary<string, List<string>>();
            StateVariables = new List<string>();
            ArrayIndex = new Dictionary<string, int>();
            FunctionNames = new List<(string, string)>();

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



        public ParserContext GetHighestContext(string name, ParserContext currentContext)
        {

            if (!Scopes.ContainsKey(name)) throw new ArgumentException($"Sorry but {name} does not exist in the current context");

            var num = currentContext.ToString().Count(x => x == '-');

            for (int i = 0; i <= num; i++)
            {

                if (Scopes[name].Contains(currentContext.ToString()))
                {
                    return currentContext;
                }

                if (i != num) currentContext = currentContext.OneContextUp();

            }

            throw new ArgumentException($"{name} is not in scope");

        }

        public string GetFunctionNameFromContext(string context) {

            foreach (var tuple in FunctionNames) {

                if (context.StartsWith(tuple.Item2)) {
                    return tuple.Item1;
                }
            }

            throw new ArgumentException("doesnt work like that sorry");
        }

        public List<string> GetFunctionParameter(string name)
        {
            if (name.Equals("_LENGTH")) {
                return new List<string>() {"name"};
            }

            if (FunctionParameters.Count == 0) return new List<string>();
            return FunctionParameters[name].ToList();
        }

        public void AddFunctionParameter(string parameterName, string functionName)
        {

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
