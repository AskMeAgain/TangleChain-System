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
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
              new TangleNet::Transfer {
                  Address = new TangleNet::Address(sendTo),
                  Tag = new TangleNet::Tag("TANGLECHAIN"),
                  Message = blockJson,
                  Timestamp = Timestamp.UnixSecondsTimestamp
              });

            bundle.Finalize();
            bundle.Sign();

            return repository.SendTrytes(bundle.Transactions, 10, 14);

        }

        public static List<TangleNet::TransactionTrytes> UploadTransaction(Transaction trans) {

            if (trans.Hash == null)
                throw new ArgumentException("Transaction is not finalized. Did you forget to Final() the Transaction?");

            //get sending address
            String sendTo = trans.SendTo;

            //prepare data
            string json = trans.ToJSON();
            var transJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
                new TangleNet::Transfer {
                    Address = new TangleNet::Address(sendTo),
                    Tag = new TangleNet::Tag("TANGLECHAIN"),
                    Message = transJson,
                    Timestamp = Timestamp.UnixSecondsTimestamp
                });

            bundle.Finalize();
            bundle.Sign();

            var result = repository.SendTrytes(bundle.Transactions, 10, 14);

            return result;

        }

        public static Difficulty GetDifficultyViaHeight(string CoinName, long? Height) {

            if (Height == null || Height == 0)
                return new Difficulty(7);

            DataBase Db = new DataBase(CoinName);

            if (!Db.ExistedBefore)
                throw new ArgumentException("Database is certainly empty!");

            long epochCount = IXISettings.GetChainSettings(CoinName).DifficultyAdjustment;
            int goal = IXISettings.GetChainSettings(CoinName).BlockTime;

            //height of last epoch before:
            long consolidationHeight = (long)Height / epochCount * epochCount;

            //if we go below 0 with height, we use genesis block as HeightA, but this means we need to reduce epochcount by 1
            int flag = 0;

            //both blocktimes ... A happened before B
            long HeightOfA = consolidationHeight - 1 - epochCount;
            if (HeightOfA < 0) {
                HeightOfA = 0;
                flag = 1;
            }

            long? timeA = Db.GetBlock(HeightOfA)?.Time;
            long? timeB = Db.GetBlock(consolidationHeight - 1)?.Time;

            //if B is not null, then we can compute the new difficulty
            if (timeB == null || timeA == null)
                return new Difficulty(7);

            //compute multiplier
            float multiplier = goal / (((long)timeB - (long)timeA) / (epochCount - flag));

            //get current difficulty
            Difficulty currentDifficulty = Db.GetBlock(consolidationHeight - 1)?.Difficulty;

            if (currentDifficulty == null)
                return new Difficulty(7);

            //calculate the difficulty change
            var precedingZerosChange = Utils.CalculateDifficultyChange(multiplier);

            //overloaded minus operator for difficulty
            return currentDifficulty + precedingZerosChange;

        }

        public static Difficulty GetDifficultyViaWay(string CoinName, Way way) {

            if (way == null)
                return new Difficulty(7);

            DataBase Db = new DataBase(CoinName);

            long epochCount = IXISettings.GetChainSettings(CoinName).DifficultyAdjustment;
            int goal = IXISettings.GetChainSettings(CoinName).BlockTime;


            //height of last epoch before:
            long heightB = way.BlockHeight / epochCount * epochCount;

            //both blocktimes ... A happened before B
            long? timeA = way.GetWayViaHeight(heightB - 1)?.Time;
            long? timeB = way.GetWayViaHeight(heightB - 1 - epochCount)?.Time;

            if (timeA == null || timeB == null)
                return new Difficulty(7);

            //compute multiplier
            float multiplier = goal / (((long)timeB - (long)timeA) / epochCount);

            //get current difficulty
            Difficulty currentDifficulty = Db.GetLatestBlock()?.Difficulty;

            if (currentDifficulty == null)
                return new Difficulty(7);

            //find nearest power of 3 from multiplier
            var nearestPower = Utils.CalculateDifficultyChange(multiplier);

            //overloaded - operator of difficulty
            return currentDifficulty + nearestPower;

        }


        public static Block GetSpecificBlock(string address, string blockHash, Difficulty difficulty, bool verifyBlock) {

            var blockList = GetAllBlocksFromAddress(address, difficulty, null);

            foreach (Block block in blockList) {
                if (block.Hash.Equals(blockHash)) {

                    //if difficulty is null, then we dont care about the block
                    if (verifyBlock && difficulty != null) {
                        if (Utils.VerifyBlock(block, difficulty))
                            return block;
                    } else
                        return block;
                }
            }

            return null;
        }

        public static List<Block> GetAllBlocksFromAddress(string address, Difficulty difficulty, long? height) {

            //create objects
            var blockList = new List<Block>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles) {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                Block newBlock = Block.FromJSON(json);

                //verify block too
                if (height == null || height == newBlock.Height) {
                    if (difficulty != null) {
                        if (Utils.VerifyHashAndNonceAgainstDifficulty(newBlock, difficulty))
                            blockList.Add(newBlock);
                    } else
                        blockList.Add(newBlock);
                }

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

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
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



        public static Block DownloadChain(string address, string hash, bool storeDB, Action<Block> Hook, string CoinName) {

            Difficulty difficulty = GetDifficultyViaHeight(CoinName, null);

            Block block = GetSpecificBlock(address, hash, difficulty, true);

            Hook?.Invoke(block);

            //we store first block! stupid hack
            if (storeDB) {
                DataBase db = new DataBase(CoinName);
                db.AddBlock(block, true);
            }

            while (true) {

                //first we need to get the correct way
                Way way = FindCorrectWay(block.NextAddress, block.CoinName, block.Height + 1, CoinName);

                //we repeat the whole until we dont have a newer way
                if (way == null)
                    break;

                //we then download this whole chain
                if (storeDB)
                    DownloadBlocksFromWay(way);

                //we just jump to the latest block
                block = GetSpecificBlock(way.Address, way.BlockHash, difficulty, false);

                Hook?.Invoke(block);

            }

            return block;

        }

        private static List<Way> GrowWays(List<Way> ways, string coinName) {

            var wayList = new List<Way>();

            foreach (Way way in ways) {

                //first we get this specific block
                //we dont need to check for correct difficulty because we did it before
                Block specificBlock = GetSpecificBlock(way.Address, way.BlockHash, null, true);

                //compute now the next difficulty in case we go over the difficulty gap
                Difficulty nextDifficulty = GetDifficultyViaWay(coinName, way);

                //we then download everything in the next address
                List<Block> allBlocks = GetAllBlocksFromAddress(specificBlock.NextAddress, nextDifficulty, specificBlock.Height + 1);

                foreach (Block block in allBlocks) {

                    Way temp = new Way(block.Hash, block.SendTo, block.Height, block.Time);
                    temp.AddOldWay(way);

                    wayList.Add(temp);
                }
            }

            if (wayList.Count == 0)
                return ways;

            return wayList;

        }

        private static Way FindCorrectWay(string address, string name, long startHeight, string coinName) {

            //this function finds the "longest" chain of blocks when given an address

            //preparing
            Difficulty difficulty = GetDifficultyViaHeight(coinName, startHeight);

            //first we get all blocks
            var allBlocks = GetAllBlocksFromAddress(address, difficulty, startHeight);

            //we then generate a list of all ways from this block list
            var wayList = Utils.ConvertBlocklistToWays(allBlocks);

            //we then grow the list until we find the longest way
            while (wayList.Count > 1) {

                //we get the size before and if we add not a single more way, it means we only need to compare the sum of all lengths.
                //If the difference is 1 or less we only added a single way => longest chain for now
                int size = wayList.Count;

                int sumBefore = 0;
                wayList.ForEach(obj => { sumBefore += obj.Length; });

                //here happens the magic
                wayList = GrowWays(wayList, coinName);

                int sumAfter = 0;
                wayList.ForEach(obj => { sumAfter += obj.Length; });

                if (size == wayList.Count && sumAfter <= (sumBefore + 1))
                    break;
            }

            //growth stopped now because we only added a single block
            //we choosed now the longest way

            if (wayList.Count == 0)
                return null;

            return wayList.OrderByDescending(item => item.Length).First();

        }

        private static void DownloadBlocksFromWay(Way way) {

            Block block = GetSpecificBlock(way.Address, way.BlockHash, null, false);
            DataBase db = new DataBase(block.CoinName);
            db.AddBlock(block, true);

            while (true) {
                if (way.Before == null)
                    break;

                way = way.Before;
                block = GetSpecificBlock(way.Address, way.BlockHash, null, false);
                db.AddBlock(block, true);
            }
        }

    }
}
