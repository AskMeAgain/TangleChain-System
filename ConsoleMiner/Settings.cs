using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleMiner {

    [Serializable]
    public class Settings {

        public List<string> NodeAddress { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string DatabasePath { get; set; }
        public int CoreNumber { get; set; }
        public Chain MinedChain { get; set; }


        public class Chain {

            public string Hash { get; set; }
            public string Address { get; set; }
            public string CoinName { get; set; }

            public Chain(string h, string a, string n) {
                Hash = h;
                Address = a;
                CoinName = n;
            }
        }
    }
}
