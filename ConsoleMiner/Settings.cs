using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleMiner
{

    [Serializable]
    public class Settings
    {

        public List<string> NodeList { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string DatabasePath { get; set; }
        public int CoreNumber { get; set; }

        public string CoinName { get; set; }
        public string GenesisHash { get; set; }
        public string GenesisAddress { get; set; }

        public bool Validate()
        {
            return true;
        }
    }
}
