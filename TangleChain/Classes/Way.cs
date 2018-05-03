using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChain.Classes {

    public class Way {

        public string BlockHash { get; set; }
        public string Address { get; set; }

        public Way Before { get; set; }

        public Way(string hash, string addr) {
            BlockHash = hash;
            Address = addr;
            Before = null;
        }

    }
}
