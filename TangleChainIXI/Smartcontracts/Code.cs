using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.Classes {
    [Serializable]
    public class Code {

        public int Balance { get; set; }

        public List<Variable> Variables { get; set; }
        public List<Method> Code_ { get; set; }

        public void AddMethod(Method method) {

            if (Code_ == null)
                Code_ = new List<Method>();

            Code_.Add(method);
        }

        public void AddVariable(Variable var) {

            if (Variables == null)
                Variables = new List<Variable>();

            Variables.Add(var);
        }
    }
}
