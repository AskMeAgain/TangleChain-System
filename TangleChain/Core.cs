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
            var blockJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
              new TangleNet::Transfer {
                  Address = new TangleNet::Address(sendTo),
                  Tag = TangleNet::Tag.Empty,
                  Message = blockJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 27, 14);

            return result;
        }

        public static Block GetSpecificBlock(string address, string blockHash, int difficulty) {

            var blockList = GetAllBlocksFromAddress(address, difficulty, null);

            foreach (Block block in blockList) {
                if (block.Hash.Equals(blockHash)) {
                    block.GenerateHash();
                    return block;
                }
            }

            return null;
        }

        public static List<Block> GetAllBlocksFromAddress(string address, int difficulty, int? height) {

            //create objects
            var blockList = new List<Block>();

            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
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
                        blockList.Add(newBlock);
            }

            return blockList;
        }

        public static Block CreateAndUploadGenesisBlock(string coinName, string receiverAddress, int amount) {

            int difficulty = 5;
            string sendTo = Utils.Hash_Curl(Timestamp.UnixSecondsTimestamp.ToString(), 243);

            //first we need to create the block
            Block genesis = new Block(0, sendTo, coinName);


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

            //this function straight mines a block to a specific address with a given difficulty.

            //create block first
            Block block = new Block(height, nextAddress, coinName);

            //add transactions to block
            var transList = GetAllTransactionsFromAddress(Utils.GetTransactionPoolAddress(height, coinName));
            block.AddTransactions(transList, Settings.NumberOfTransactionsPerBlock);

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
            var allBlocks = GetAllBlocksFromAddress(address, difficulty, startHeight);

            //we then generate a list of all ways from this block list
            var wayList = Utils.ConvertBlocklistToWays(allBlocks);

            //we then grow the list until we find the longst way
            while (wayList.Count > 1) {

                //we get the size before and if we add not a single more way, it means we only need to compare the sum of all lengths.
                //If the difference is 1 or less we only added a single way => longest chain for now
                int size = wayList.Count;

                int sumBefore = 0;
                wayList.ForEach(obj => { sumBefore += obj.Length; });

                wayList = GrowWays(wayList);

                int sumAfter = 0;
                wayList.ForEach(obj => { sumAfter += obj.Length; });


                if (size == wayList.Count && sumAfter <= (sumBefore + 1))
                    break;
            }

            //growth stopped now because we only added a single block
            //we choose now the longest way

            if (wayList.Count == 0)
                return null;

            return wayList.OrderByDescending(item => item.Length).First();

        }

        public static List<Way> GrowWays(List<Way> ways) {

            int difficulty = 5;
            var wayList = new List<Way>();

            foreach (Way way in ways) {

                //first we get this specific block
                Block specificBlock = GetSpecificBlock(way.Address, way.BlockHash, difficulty);

                //we then download everything in the next address
                List<Block> allBlocks = GetAllBlocksFromAddress(specificBlock.NextAddress, difficulty, specificBlock.Height + 1);

                foreach (Block block in allBlocks) {

                    Way temp = new Way(block.Hash, block.SendTo, block.Height);
                    temp.AddOldWay(way);

                    wayList.Add(temp);
                }
            }

            return wayList;

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
            var hashList = block.TransactionHashes;

            //transactions are on this address
            string searchAddr = Utils.GetTransactionPoolAddress(block.Height, block.CoinName);

            //we now need to get the transactions from the hashes:
            var transList = GetAllTransactionsFromAddress(searchAddr);

            var returnList = new List<Transaction>();

            for (int i = 0; i < transList.Count; i++) {
                if (hashList.Contains(transList[i].Identity.Hash))
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
            var transJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
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
            var transList = new List<Transaction>();

            var repository = new RestIotaRepository(new RestClient(Settings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Transaction newTrans = Transaction.FromJSON(json);

                //verify block too
                if (newTrans != null)
                    transList.Add(newTrans);
            }

            return transList;
        }
    }
}
