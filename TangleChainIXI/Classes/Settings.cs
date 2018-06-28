using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors;
using Nethereum.Signer;

namespace TangleChainIXI.Classes {

    public static class Settings {

        public static string NodeAddress { get; private set; }
        public static string PublicKey { get { return GetPublicKey(); }}
        public static string PrivateKey { get; set; }
        public static string StorePath { get; private set; }

        public static Dictionary<string, ChainSettings> ChainSettings { get; set; }


        public static void Default(bool IRIFlag) {

            SetNodeAddress("https://potato.iotasalad.org:14265");
            PrivateKey = "secure";
            SetStorePath(@"C:\TangleChain\Chains\");

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

            ChainSettings.Add(name, settings);
        }

        public static ChainSettings GetChainSettings(string name) {
            if (ChainSettings != null && ChainSettings.ContainsKey(name))
                return ChainSettings[name];

            return new ChainSettings();
        }





    }
}
