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

        public static Dictionary<string, ChainSettings> ChainSettings { get; set; }


        public static void Default(bool devNet) {

            string addr = devNet ? "https://nodes.testnet.iota.org:443/" : "https://balancer.iotatoken.nl:4433";

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
            return Cryptography.GetPublicKey(PrivateKey);
        }

        public static NodeInfo SetNodeAddress(string addr) {

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

        public static void AddChainSettings(string name, ChainSettings settings) {

            if (ChainSettings == null)
                ChainSettings = new Dictionary<string, ChainSettings>();

            if (!ChainSettings.ContainsKey(name))
                ChainSettings.Add(name, settings);
        }

        public static ChainSettings GetChainSettings(string name) {
            if (ChainSettings != null && ChainSettings.ContainsKey(name))
                return ChainSettings[name];

            //in case we didnt set the chain settings before
            DataBase Db = new DataBase(name);

            ChainSettings cSett = Db.GetChainSettings();

            if (cSett == null)
                throw new System.ArgumentException("You forgot to set ChainSettings");

            AddChainSettings(name, cSett);

            return cSett;

        }





    }
}
