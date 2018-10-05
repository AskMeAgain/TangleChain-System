namespace TangleChainIXI.Smartcontracts {
    public class Variable {

        public string Name { get; set; }
        public string Value { get; set; }

        public Variable() { }

        /// <summary>
        /// Creates a variable. If added included in a smartcontract it will hold persistent data.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Variable(string name, string value = "__0") {
            Name = name;
            Value = value;
        }

        public override string ToString() {
            return $"Name: {Name} Value: {Value}";
        }
    }
}