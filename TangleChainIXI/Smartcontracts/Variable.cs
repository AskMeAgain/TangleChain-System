namespace TangleChainIXI.Smartcontracts {
    public class Variable {
        private object v1;
        private object v2;

        public string Name { get; set; }
        public string Value { get; set; }

        public Variable() { }

        public Variable(string name) {
            Name = name;
            Value = "0";
        }

        public Variable(string name, string value) {
            Name = name;
            Value = value;
        }

        public override string ToString() {
            return $"Name: {Name} Value: {Value}";
        }
    }
}