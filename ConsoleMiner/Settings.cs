using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleMiner {

    [Serializable]
    public class Settings {

        public  List<string> NodeAddress { get; set; }
        public  string PublicKey { get; set; }
        public  string DatabasePath { get; set; }
        public  (string Hash, string Address, string CoinName) Chain { get; set; }
        public  int CoreNumber { get; set; }

    }
}
