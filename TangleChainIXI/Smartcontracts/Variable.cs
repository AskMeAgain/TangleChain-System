namespace TangleChainIXI.Smartcontracts {
    public class Variable {

        public string Name { get; set; }
        public string Value { get; set; }

        public Variable(string name) {
            Name = name;
            Value = "0";
        }

    }
}