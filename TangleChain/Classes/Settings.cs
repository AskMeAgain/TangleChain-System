using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;

namespace IXI_TangleChain.Classes {

    public static class Settings {

        public static int TransactionPoolInterval { get; private set; }
        public static int NumberOfTransactionsPerBlock { get; private set; }
        public static string NodeAddress { get; private set; }
        public static int MiningReward { get { return 200; } set { } }
        public static string Owner { get { return "ME"; } set { } }

        public static string StorePath {
            get { return @"C:\TangleChain\Chains\"; }
            set { }
        }

        public static void Default(bool testNet) {

            string addr = (testNet) ? "https://testnet140.tangle.works/" : "https://beef.iotasalad.org:14265";

            SetNodeAddress(addr);
            SetNumOfTransPerBlock(3);
            SetTransPoolInterval(10);
        }

        public static NodeInfo SetNodeAddress(string addr) {

            //no need for try block since restiotarepo throws exception anyway

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

        public static int SetNumOfTransPerBlock(int num) {

            NumberOfTransactionsPerBlock = num;

            return NumberOfTransactionsPerBlock;

        }

        public static int SetTransPoolInterval(int num) {

            TransactionPoolInterval = num;

            return TransactionPoolInterval;

        }






    }
}
