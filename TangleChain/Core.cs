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

        public static List<TransactionTrytes> UploadBlock(Block block) {

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

        public static Block CreateAndUploadGenesisBlock() {

            int difficulty = 5;
            string sendTo = Utils.Hash_Curl(Timestamp.UnixSecondsTimestamp.ToString(),243);

            //first we need to create the block
            Block genesis = CreateBlock(0, sendTo);

            //generate hash from the block
            genesis.GenerateHash();

            //then we find the correct nonce
            genesis.Nonce = Utils.ProofOfWork(genesis.Hash, difficulty);

            //we check the nonce first in case of wrong computation
            if (!Utils.VerifyHash(genesis.Hash, genesis.Nonce, difficulty))
                throw new ArgumentException("Nonce didnt got correctly computed");


            //then we upload the block
            UploadBlock(genesis);

            return genesis;
        }

        public static Block CreateBlock(int height, string SendTo) {

            Block block = new Block() {
                Height = height,
                Time = Timestamp.UnixSecondsTimestamp,
                SendTo = SendTo,
                Owner = "ME",
                NextAddress = Utils.GenerateNextAddr(height, SendTo)
            };

            //generate hash from the insides
            block.GenerateHash();

            return block;

        }

        public static Block MineBlock(int height, string NextAddress, int difficulty) {

            //this function straight mines a block to a specific address with a difficulty.

            //create block first
            Block block = CreateBlock(height, NextAddress);

            //generate hash
            block.GenerateHash();

            //do proof of work
            block.Nonce = Utils.ProofOfWork(block.Hash, difficulty);

            //send block
            Core.UploadBlock(block);

            return block;
        }
    }
}
