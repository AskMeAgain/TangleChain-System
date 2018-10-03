using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TangleNet = Tangle.Net.Entity;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI
{

    public static class Core
    {

        #region basic functionality

        public static T GetSpecific<T>(string address, string hash) where T : IDownloadable
        {

            var objList = GetAllFromAddress<T>(address);

            foreach (T obj in objList)
            {
                if (obj.Hash.Equals(hash))
                {
                    return obj;
                }
            }

            return default(T);
        }

        public static List<T> GetAllFromBlock<T>(Block block) where T : IDownloadable
        {

            var list = new List<string>();

            if (typeof(T) == typeof(Transaction))
                list = block.TransactionHashes;
            if (typeof(T) == typeof(Smartcontract))
                list = block.SmartcontractHashes;

            //stuff is on this address
            string searchAddr = Utils.GetTransactionPoolAddress(block.Height, block.CoinName);

            //we now need to get the objects from the hashes:
            var objectList = GetAllFromAddress<T>(searchAddr);

            var returnList = new List<T>();

            if (list != null)
                for (int i = 0; i < objectList.Count; i++)
                {
                    if (list.Contains(objectList[i].Hash))
                        returnList.Add(objectList[i]);
                }

            return returnList;
        }

        //private static List<Block> GetBlocksFromWay(Way way)
        //{

        //    List<Block> blockList = new List<Block>();

        //    Block block = GetSpecific<Block>(way.Address, way.BlockHash);

        //    blockList.Add(block);

        //    while (true)
        //    {
        //        if (way.Before == null)
        //            break;

        //        way = way.Before;
        //        block = GetSpecific<Block>(way.Address, way.BlockHash);
        //        blockList.Add(block);
        //    }

        //    return blockList;
        //}

        #endregion

        #region advanced functionality

        public static Block DownloadChain(string CoinName, string address, string hash, bool storeDB, bool includeSmartcontracts, Action<Block> Hook)
        {

            //difficulty doesnt matter. we need to assume address and hash are correct from calculations before
            Block block = GetSpecific<Block>(address, hash);
            if (!block.Verify(DBManager.GetDifficulty(CoinName, block.Height)))
                throw new ArgumentException("Provided Block is NOT VALID!");

            Hook?.Invoke(block);

            //we store first block! stupid hack
            if (storeDB)
            {
                DBManager.AddBlock(block, true, includeSmartcontracts);
            }

            while (true)
            {

                //first we need to get the correct way
                Way way = FindCorrectWay(block.NextAddress, block.CoinName, block.Height + 1, CoinName);

                //we repeat the whole until we dont have a newer way
                if (way == null)
                    break;

                //we then download this whole chain
                if (storeDB) {
                    List<Block> list = way.ToBlockList();
                    DBManager.AddBlocks(CoinName, list, true, includeSmartcontracts);
                }

                //we just jump to the latest block
                block = way.CurrentBlock;

                Hook?.Invoke(block);

            }

            return block;

        }

        private static List<Way> GrowWays(List<Way> ways, string coinName)
        {

            var wayList = new List<Way>();

            foreach (Way way in ways)
            {

                //first we get this specific block
                Block specificBlock = GetSpecific<Block>(way.CurrentBlock.SendTo, way.CurrentBlock.Hash);

                //compute now the next difficulty in case we go over the difficulty gap
                Difficulty nextDifficulty = DBManager.GetDifficulty(coinName, way);

                //we then download everything in the next address
                List<Block> allBlocks = GetAllFromAddress<Block>(specificBlock.NextAddress)
                    .Where(b => b.Height == specificBlock.Height + 1)
                    .Where(b => b.Verify(nextDifficulty))
                    .ToList();

                foreach (Block block in allBlocks)
                {

                    Way temp = new Way(block);
                    temp.AddOldWay(way);

                    wayList.Add(temp);
                }
            }

            return wayList;

        }

        private static Way FindCorrectWay(string address, string name, long startHeight, string coinName)
        {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //preparing
            Difficulty difficulty = DBManager.GetDifficulty(coinName, startHeight);

            //first we get all blocks
            var allBlocks = GetAllFromAddress<Block>(address)
                .Where(b => b.Height == startHeight)
                .Where(b => b.Verify(difficulty))
                .ToList();

            //we then generate a list of all possible ways from this block list
            var wayList = allBlocks.ToWayList();

            //we then grow the list until we find the longest way
            while (wayList.Count > 1)
            {

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

        #endregion

        public static List<TangleNet::TransactionTrytes> Upload(this IDownloadable obj)
        {

            if (string.IsNullOrEmpty(obj.SendTo))
            {
                throw new ArgumentException("Smartcontract doesnt have SENDTO set");
            }

            //get sending address
            String sendTo = obj.SendTo;

            //prepare data
            string json = Utils.ToJSON(obj);
            var transJson = TangleNet::TryteString.FromUtf8String(json);

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
                new TangleNet::Transfer
                {
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

        public static List<T> GetAllFromAddress<T>(string address) where T : IDownloadable
        {
            //create objects
            var list = new List<T>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, true);

            foreach (TangleNet::Bundle bundle in bundles)
            {
                string json = bundle.Transactions.Where(t => t.IsTail).Single().Fragment.ToUtf8String();
                T newTrans = Utils.FromJSON<T>(json);

                //verify block too
                if (newTrans != null)
                    list.Add(newTrans);
            }

            return list;
        }
    }
}
