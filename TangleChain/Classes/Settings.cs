using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChain.Classes {

    public static class Settings {

        public static int TransactionPoolInterval { get { return 20; } private set { } }
        public static int NumberOfTransactionsPerBlock { get { return 5; } private set { } }
        public static string NodeAddress { get { return "http://node05.iotatoken.nl:16265"; } private set { } }

    }
}
