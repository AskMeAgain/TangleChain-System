using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;

namespace TangleChain {
    public static class Core {

        public static List<TransactionTrytes> SendBlock(Block block) {

            //get sending address
            String sendTo = block.SendTo;

            //prepare data
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(block);
            TryteString blockJson = TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient("http://iotanode.party:14265"), new PoWService(new CpuPearlDiver()));

            Bundle bundle = new Bundle();
            bundle.AddTransfer(
              new Transfer {
                  Address = new Address(sendTo),
                  Tag = Tag.Empty,
                  Message = blockJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp,
              });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 27, 14);

            return result;
        }

        public static Block GetSpecificBlock(string address, string blockHash) {

            var blocks = GetAllBlocksFromAddress(address);

            foreach (Block block in blocks) {
                if (block.Hash.Equals(blockHash))
                    return block;
            }

            return null;

        }

        public static List<Block> GetAllBlocksFromAddress(string address) {

            //create objects
            List<Block> blocks = new List<Block>();
            var repository = new RestIotaRepository(new RestClient("http://iotanode.party:14265"));
            List<Address> addressList = new List<Address>() {
                new Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Block newBlock = Utils.GetBlockFromJSON(json);

                blocks.Add(newBlock);
            }

            return blocks;
        }
    }
}
