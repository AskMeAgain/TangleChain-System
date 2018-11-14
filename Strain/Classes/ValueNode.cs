using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Smartcontracts;

namespace Strain.Classes
{
    public class ValueNode : Node
    {
        private string _value;

        public ValueNode(string value)
        {
            _value = value;
        }

        public override string GetValue()
        {
            return _value;
        }
    }
}
