using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using System.Collections.Generic;

namespace TangleChainIXI.Classes {

    public static class Settings {

        public static int TransactionPoolInterval { get; private set; }
        public static string NodeAddress { get; private set; }
        public static string Owner { get { return "ME"; } set { } }
        public static Dictionary<string, ChainSettings> ChainSettings {get;set; }

        public static string StorePath {
            get { return @"C:\TangleChain\Chains\"; }
            set { }
        }

        public static void Default(bool testNet) {

            string addr = (testNet) ? "https://testnet140.tangle.works/" : "https://beef.iotasalad.org:14265";

            SetNodeAddress(addr);
            SetTransPoolInterval(10);
        }

        public static NodeInfo SetNodeAddress(string addr) {

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

        public static int SetTransPoolInterval(int num) {

            TransactionPoolInterval = num;

            return TransactionPoolInterval;

        }

        public static void AddChainSettings(string name, ChainSettings settings) {

            if (ChainSettings == null)
                ChainSettings = new Dictionary<string, ChainSettings>();

            ChainSettings.Add(name, settings);
        }





    }
}
