using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors;
using Nethereum.Signer;

namespace TangleChainIXI.Classes {

    public static class IXISettings {

        public static string NodeAddress { get; private set; }
        public static string PublicKey { get; set; }
        public static string PrivateKey { get; private set; }
        public static string StorePath { get; private set; }

        public static void Default(bool mainNet) {

            string addr = mainNet ? "https://turnip.iotasalad.org:14265" : "https://nodes.testnet.iota.org:443/";

            SetNodeAddress(addr);

            SetPrivateKey("secure");
            SetStorePath(@"C:\TangleChain\Chains\");

        }

        public static void SetPrivateKey(string key) {
            PrivateKey = key;
            PublicKey = GetPublicKey();
        }

        public static void SetStorePath(string path) {
            StorePath = path;
        }

        public static string GetPublicKey() {
            return PrivateKey.GetPublicKey();
        }

        public static NodeInfo SetNodeAddress(string addr) {

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

    }
}
