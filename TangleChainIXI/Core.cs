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

        /// <summary>
        /// Returns an object with the given hash from a specific address.
        /// </summary>
        /// <typeparam name="T">Needs to implement IDownloadable (Smartcontract, Block, Transaction)</typeparam>
        /// <param name="address">Iota address to look at</param>
        /// <param name="hash">The hash of the object</param>
        /// <returns></returns>
        public static T GetSpecificFromAddress<T>(string address, string hash) where T : IDownloadable
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

        /// <summary>
        /// Returns an object from a block. Takes some time because it will download from the internet
        /// </summary>
        /// <typeparam name="T">Can be Transaction or Smartcontract</typeparam>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<T> GetAllFromBlock<T>(Block block) where T : IDownloadable
        {
            var list = new List<string>();

            if (typeof(T) == typeof(Transaction))
                list = block.TransactionHashes;
            else if (typeof(T) == typeof(Smartcontract))
                list = block.SmartcontractHashes;
            else
                throw new ArgumentException("You cant get the given object from a block. From type");

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

        /// <summary>
        /// Gets all Objects T from an address
        /// </summary>
        /// <typeparam name="T">Needs to be Block,Smartcontract or Transaction</typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Uploads the object to the specified SendTo address inside the object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Upload<T>(this T obj) where T : IDownloadable
        {

            if (!obj.IsFinalized)
            {
                throw new ArgumentException("Object not finalized");
            }

            //prepare data
            var transJson = TangleNet::TryteString.FromUtf8String(Utils.ToJSON(obj));

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
                new TangleNet::Transfer
                {
                    Address = new TangleNet::Address(obj.SendTo),
                    Tag = new TangleNet::Tag("TANGLECHAIN"),
                    Message = transJson,
                    Timestamp = Timestamp.UnixSecondsTimestamp
                });

            bundle.Finalize();
            bundle.Sign();

            repository.SendTrytes(bundle.Transactions, 10, 14);

            return obj;

        }

        #endregion

        #region advanced functionality

        /// <summary>
        /// Downloads the chain from a given address. Doesnt need to start at the genesis block
        /// </summary>
        /// <param name="CoinName">The name of the coin</param>
        /// <param name="address">The starting address where to look at</param>
        /// <param name="hash">The hash of the first block</param>
        /// <param name="storeDB">Specifies whether you want to store all the blocks which are from the given chain</param>
        /// <param name="includeSmartcontracts">Should be always true</param>
        /// <param name="Hook">The hook which will be executed are each downloaded block/step. This will skip blocks sometimes due to the algorithm</param>
        /// <returns></returns>
        public static Block DownloadChain(string CoinName, string address, string hash, bool storeDB, Action<Block> Hook)
        {

            //difficulty doesnt matter. we need to assume address and hash are correct from calculations before
            Block block = GetSpecificFromAddress<Block>(address, hash);
            if (!block.Verify(DBManager.GetDifficulty(CoinName, block.Height)))
                throw new ArgumentException("Provided Block is NOT VALID!");

            Hook?.Invoke(block);

            //we store first block! stupid hack
            if (storeDB)
            {
                DBManager.AddBlock(block);
            }

            while (true)
            {

                //first we need to get the correct way
                Way way = FindCorrectWay(block.NextAddress, block.Height + 1, CoinName);

                //we repeat the whole until we dont have a newer way
                if (way == null)
                    break;

                //we then download this whole chain
                if (storeDB)
                {
                    List<Block> list = way.ToBlockList();
                    DBManager.AddBlocks(CoinName, list);
                }

                //we just jump to the latest block
                block = way.CurrentBlock;

                Hook?.Invoke(block);

            }

            return block;

        }

        /// <summary>
        /// Takes a list with ways and grows each way into another list of ways, for each possible block in the chain
        /// </summary>
        /// <param name="ways">The ways</param>
        /// <param name="coinName">Coinname </param>
        /// <returns></returns>
        private static List<Way> GrowWays(List<Way> ways, string coinName)
        {

            var wayList = new List<Way>();

            foreach (Way way in ways)
            {

                //first we get this specific block
                Block specificBlock = GetSpecificFromAddress<Block>(way.CurrentBlock.SendTo, way.CurrentBlock.Hash);

                //compute now the next difficulty in case we go over the difficulty gap
                int nextDifficulty = DBManager.GetDifficulty(coinName, way);

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

                if (allBlocks == null)
                    wayList.Add(way);
            }

            return wayList;

        }

        /// <summary>
        /// Finds the correct way from a given address with the longest chain
        /// </summary>
        /// <param name="address">The address to look at</param>
        /// <param name="startHeight">The starting height of the new blocks</param>
        /// <param name="coinName">The name of the coin</param>
        /// <returns></returns>
        private static Way FindCorrectWay(string address, long startHeight, string coinName)
        {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //preparing
            int difficulty = DBManager.GetDifficulty(coinName, startHeight);

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


    }
}
