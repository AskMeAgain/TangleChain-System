using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors;
using Nethereum.Signer;

namespace TangleChainIXI.Classes
{

    public static class IXISettings
    {

        public static string NodeAddress { get; private set; }

        private static string _publicKey;
        public static string PublicKey {
            get => _publicKey ?? GetPublicKey();
            set => _publicKey = value;
        }

        public static string PrivateKey { get; private set; }
        public static string DataBasePath { get; private set; }

        /// <summary>
        /// initializes the settings with default values
        /// </summary>
        /// <param name="mainNet"></param>
        public static void Default(bool mainNet)
        {

            string addr = mainNet ? "https://nodes.thetangle.org:443" : "https://nodes.testnet.iota.org:443/";

            SetNodeAddress(addr);

            SetPrivateKey("secure");
            SetDataBasePath(@"C:\TangleChain\Chains\");

        }

        /// <summary>
        /// sets the private key
        /// </summary>
        /// <param name="key"></param>
        public static void SetPrivateKey(string key)
        {
            PrivateKey = key;
            PublicKey = GetPublicKey();
        }

        public static void SetDataBasePath(string path)
        {
            DataBasePath = path;
        }

        public static string GetPublicKey()
        {
            return PrivateKey.GetPublicKey();
        }

        public static NodeInfo SetNodeAddress(string addr)
        {

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

    }
}
