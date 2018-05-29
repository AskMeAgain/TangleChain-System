using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChain.Classes;

namespace TangleChain {

    public static class Core {

        public static List<TangleNet::TransactionTrytes> UploadBlock(Block block) {

            //get sending address
            String sendTo = block.SendTo;

            //prepare data
            string json = block.ToJSON();
            TangleNet::TryteString blockJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress), new PoWService(new CpuPearlDiver()));

            TangleNet::Bundle bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
              new TangleNet::Transfer {
                  Address = new TangleNet::Address(sendTo),
                  Tag = TangleNet::Tag.Empty,
                  Message = blockJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            List<TangleNet::TransactionTrytes> result = repository.SendTrytes(bundle.Transactions, 27, 14);

            return result;
        }

        public static Block GetSpecificBlock(string address, string blockHash, int difficulty) {

            var blocks = GetAllBlocksFromAddress(address, difficulty, null);

            foreach (Block block in blocks) {
                if (block.Hash.Equals(blockHash))
                    return block;
            }

            return null;

        }

        public static List<Block> GetAllBlocksFromAddress(string address, int difficulty, int? height) {

            //create objects
            List<Block> blocks = new List<Block>();

            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress));
            List<TangleNet::Address> addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Block newBlock = Block.FromJSON(json);
                newBlock.GenerateHash();

                //verify block too
                if (Utils.VerifyBlock(newBlock, difficulty))
                    if (height == null || height == newBlock.Height)
                        blocks.Add(newBlock);
            }

            return blocks;
        }

        public static Block CreateAndUploadGenesisBlock(string coinName, string receiverAddress, int amount) {

            int difficulty = 5;
            string sendTo = Utils.Hash_Curl(Timestamp.UnixSecondsTimestamp.ToString(), 243);

            //first we need to create the block
            Block genesis = Block.CreateBlock(0, sendTo, coinName);


            //create genesis Transaction
            Transaction trans = new Transaction("GENESIS", -1, Utils.GetTransactionPoolAddress(0, coinName));
            trans.AddOutput(amount, receiverAddress);
            trans.Signature = "LOL";
            trans.GenerateHash();

            //upload transaction
            UploadTransaction(trans);

            //add transaction to block
            genesis.TransactionHashes.Add(trans.Identity.Hash);

            //generate hash from the block
            genesis.GenerateHash();

            //then we find the correct nonce
            genesis.Nonce = Utils.ProofOfWork(genesis.Hash, difficulty);

            //then we upload the block
            UploadBlock(genesis);

            return genesis;
        }

        public static Block MineBlock(string coinName, int height, string nextAddress, int difficulty, bool storeDB) {

            //this function straight mines a block to a specific address with a difficulty.

            //create block first
            Block block = Block.CreateBlock(height, nextAddress, coinName);

            //generate hash
            block.GenerateHash();

            //do proof of work
            block.Nonce = Utils.ProofOfWork(block.Hash, difficulty);

            //send block
            UploadBlock(block);

            if (storeDB) {
                DataBase db = new DataBase(block.CoinName);
                db.AddBlock(block, true);
            }

            return block;
        }

        public static Way FindCorrectWay(string address, string name, int startHeight) {

            //this function finds the "longest" chain of blocks when given an address

            int difficulty = 5;

            //general container

            //first we get all blocks
            List<Block> allBlocks = GetAllBlocksFromAddress(address, difficulty, startHeight);

            //we then generate a list of all ways from this block list
            List<Way> ways = Utils.ConvertBlocklistToWays(allBlocks);

            //we then grow the list until we find the longst way
            while (ways.Count > 1) {

                //we get the size before and if we add not a single more way, it means we only need to compare the sum of all lengths.
                //If the difference is 1 or less we only added a single way => longest chain for now
                int size = ways.Count;

                int sumBefore = 0;
                ways.ForEach(obj => { sumBefore += obj.Length; });

                ways = GrowWays(ways);

                int sumAfter = 0;
                ways.ForEach(obj => { sumAfter += obj.Length; });


                if (size == ways.Count && sumAfter <= (sumBefore + 1))
                    break;
            }

            //growth stopped now because we only added a single block
            //we choose now the longest way

            if (ways.Count == 0)
                return null;

            Way rightWay = ways.OrderByDescending(item => item.Length).First();

            return rightWay;

        }

        public static List<Way> GrowWays(List<Way> ways) {

            int difficulty = 5;
            List<Way> list = new List<Way>();

            foreach (Way way in ways) {

                //first we get this specific block
                Block specificBlock = GetSpecificBlock(way.Address, way.BlockHash, difficulty);

                //we then download everything in the next address
                List<Block> allBlocks = GetAllBlocksFromAddress(specificBlock.NextAddress, difficulty, specificBlock.Height + 1);

                foreach (Block block in allBlocks) {

                    Way temp = new Way(block.Hash, block.SendTo, block.Height);
                    temp.AddOldWay(way);

                    list.Add(temp);
                }
            }

            return list;

        }

        public static Block DownloadChain(string address, string hash, int difficulty, bool storeDB) {

            Block block = GetSpecificBlock(address, hash, difficulty);

            //we store genesis block! stupid hack
            if (storeDB) {
                DataBase db = new DataBase(block.CoinName);
                db.AddBlock(block, true);
            }

            while (true) {

                //first we need to get the correct way
                Way way = FindCorrectWay(block.NextAddress, block.CoinName, block.Height + 1);

                //we repeat the whole until we dont have a newer way
                if (way == null)
                    break;

                //we then download this whole chain
                if (storeDB)
                    DownloadBlocksFromWay(way, difficulty);

                //we just jump to the latest block
                block = GetSpecificBlock(way.Address, way.BlockHash, difficulty);

                //if (block == null)
                //    break;

            }

            return block;

        }

        public static void DownloadBlocksFromWay(Way way, int difficulty) {

            Block block = GetSpecificBlock(way.Address, way.BlockHash, difficulty);
            DataBase db = new DataBase(block.CoinName);
            db.AddBlock(block, true);

            while (true) {
                if (way.Before == null)
                    break;

                way = way.Before;
                block = GetSpecificBlock(way.Address, way.BlockHash, difficulty);
                db.AddBlock(block, true);
            }
        }

        public static List<Transaction> GetAllTransactionsFromBlock(Block block) {

            //all hashes of the transactions included in the block
            List<string> list = block.TransactionHashes;

            //transactions are on this address
            string searchAddr = Utils.GetTransactionPoolAddress(block.Height, block.CoinName);

            //we now need to get the transactions from the hashes:
            List<Transaction> transList = GetAllTransactionsFromAddress(searchAddr);

            List<Transaction> returnList = new List<Transaction>();

            for (int i = 0; i < transList.Count; i++) {
                if (list.Contains(transList[i].Identity.Hash))
                    returnList.Add(transList[i]);
            }

            return returnList;
        }



        public static Block OneClickMining(string genesis, string hash, int difficulty) {

            //first we need to get to the latest block
            Block latest = DownloadChain(genesis, hash, difficulty, true);

            //we then mine a block ontop of this block
            Block newBlock = MineBlock(latest.CoinName, latest.Height + 1, latest.NextAddress, difficulty, true);

            return newBlock;
        }

        public static List<TangleNet::TransactionTrytes> UploadTransaction(Transaction trans) {

            //get sending address
            String sendTo = trans.Identity.SendTo;

            //prepare data
            string json = trans.ToJSON();
            TangleNet::TryteString transJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress), new PoWService(new CpuPearlDiver()));

            TangleNet::Bundle bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
              new TangleNet::Transfer {
                  Address = new TangleNet::Address(sendTo),
                  Tag = TangleNet::Tag.Empty,
                  Message = transJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 27, 14);

            return result;

        }

        public static List<Transaction> GetAllTransactionsFromAddress(string address) {

            //create objects
            List<Transaction> transactions = new List<Transaction>();

            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress));
            List<TangleNet::Address> addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Transaction newTrans = Transaction.FromJSON(json);

                //verify block too
                if (newTrans != null)
                    transactions.Add(newTrans);
            }

            return transactions;
        }
    }
}
