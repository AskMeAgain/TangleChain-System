using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors;
using Nethereum.Signer;

namespace TangleChainIXI.Classes
{

    public class IXISettings
    {

        public string NodeAddress { get; private set; }

        private string _publicKey;
        public string PublicKey {
            get => _publicKey ?? PrivateKey.GetPublicKey();
            set => _publicKey = value;
        }

        public string PrivateKey { get; private set; }
        public string DataBasePath { get; private set; }

        /// <summary>
        /// initializes the settings with default values
        /// </summary>
        /// <param name="mainNet"></param>
        public IXISettings Default(bool mainNet)
        {

            string addr = mainNet ? "https://turnip.iotasalad.org:14265" : "https://nodes.testnet.iota.org:443/";

            SetNodeAddress(addr);

            SetPrivateKey("secure");
            SetDataBasePath(@"C:\TangleChain\Chains\");

            return this;
        }

        /// <summary>
        /// sets the private key
        /// </summary>
        /// <param name="key"></param>
        public void SetPrivateKey(string key)
        {
            PrivateKey = key;
            PublicKey = key.GetPublicKey();
        }

        public void SetDataBasePath(string path)
        {
            DataBasePath = path;
        }

        public NodeInfo SetNodeAddress(string addr)
        {

            var repository = new RestIotaRepository(new RestClient(addr));
            var info = repository.GetNodeInfo();

            NodeAddress = addr;

            return info;
        }

    }
}
