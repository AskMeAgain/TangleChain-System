using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors;
using Nethereum.Signer;

namespace TangleChainIXI.Classes {

    public static class Settings {

        public static string NodeAddress { get; private set; }
        public static string Owner { get { return "ME"; } set { } }
        public static Dictionary<string, ChainSettings> ChainSettings { get; set; }
        public static string PrivateKey { get; set; }

        public static string StorePath {
            get { return @"C:\TangleChain\Chains\"; }
            set { }
        }

        public static void Default(bool IRIFlag) {

            string oldIRI = "https://node1.iotaner.org:443";
            string newIRI = "http://node02.iotatoken.nl:14265";

            string addr = (IRIFlag) ? newIRI : oldIRI;

            SetNodeAddress(addr);
            PrivateKey = "secure";

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

            ChainSettings.Add(name, settings);
        }

        public static ChainSettings GetChainSettings(string name) {
            if (ChainSettings != null && ChainSettings.ContainsKey(name))
                return ChainSettings[name];

            return new ChainSettings();
        }





    }
}
