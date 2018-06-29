using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;

namespace TangleChainIXI {

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

            return repository.SendTrytes(bundle.Transactions, 10, 14);

        }

        public static List<TangleNet::TransactionTrytes> UploadTransaction(Transaction trans) {

            //get sending address
            String sendTo = trans.SendTo;

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

            var result = repository.SendTrytes(bundle.Transactions, 10, 14);

            return result;

        }



        public static Block GetSpecificBlock(string address, string blockHash, int difficulty) {

            var blockList = GetAllBlocksFromAddress(address, difficulty, null);

            foreach (Block block in blockList) {
                if (block.Hash.Equals(blockHash))
                    return block;
            }

            return null;
        }

        public static List<Block> GetAllBlocksFromAddress(string address, int difficulty, long? height) {

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

                //verify block too
                if (Utils.VerifyBlock(newBlock, difficulty))
                    if (height == null || height == newBlock.Height)
                        blockList.Add(newBlock);
            }

            return blockList;
        }

        public static List<Transaction> GetAllTransactionsFromBlock(Block block) {

            //all hashes of the transactions included in the block
            var hashList = block.TransactionHashes;

            //transactions are on this address
            string searchAddr = Utils.GetTransactionPoolAddress(block.Height, block.CoinName);

            //we now need to get the transactions from the hashes:
            var transList = GetAllTransactionsFromAddress(searchAddr);

            var returnList = new List<Transaction>();

            if (hashList != null)
                for (int i = 0; i < transList.Count; i++) {
                    if (hashList.Contains(transList[i].Hash))
                        returnList.Add(transList[i]);
                }

            return returnList;
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



        public static Block DownloadChain(string address, string hash, int difficulty, bool storeDB, Action<Block> Hook) {

            Block block = GetSpecificBlock(address, hash, difficulty);

            Hook?.Invoke(block);

            //we store first block! stupid hack
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

                Hook?.Invoke(block);

            }

            return block;

        }

        private static List<Way> GrowWays(List<Way> ways) {

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

            if (wayList.Count == 0)
                return ways;

            return wayList;

        }

        private static Way FindCorrectWay(string address, string name, long startHeight) {

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

        private static void DownloadBlocksFromWay(Way way, int difficulty) {

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

    }
}
