using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Tangle.Net.Repository;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleNet = Tangle.Net.Entity;

namespace TangleChainIXI
{
    public class SimpleBlockManager : IBlockManager
    {

        public string CoinName { get; set; }

        public Block GetSpecificBlock(string address, string hash)
        {
            var objList = GetAllFromAddress<Block>(address);

            foreach (var obj in objList)
            {
                if (obj.Hash.Equals(hash))
                {
                    return obj;
                }
            }

            return null;
        }

        private List<T> GetAllFromBlock<T>(Block block) where T : IDownloadable
        {
            var list = new List<string>();

            if (typeof(T) == typeof(Transaction))
                list = block.TransactionHashes;
            else if (typeof(T) == typeof(Smartcontract))
                list = block.SmartcontractHashes;
            else
                throw new ArgumentException("You cant get the given object from a block. Type is not correct");

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

        private List<T> GetAllFromAddress<T>(string address) where T : IDownloadable
        {
            //create object list
            var list = new List<T>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, false);

            foreach (TangleNet::Bundle bundle in bundles)
            {

                string json = bundle.AggregateFragments().ToUtf8String();

                T newTrans = Utils.FromJSON<T>(json);

                if (newTrans != null)
                    list.Add(newTrans);

            }

            return list;
        }

        private List<Way> GrowWays(List<Way> ways)
        {

            var wayList = new List<Way>();

            foreach (Way way in ways)
            {

                //first we get this specific block
                Block specificBlock = GetSpecificBlock(way.CurrentBlock.SendTo, way.CurrentBlock.Hash);

                //compute now the next difficulty in case we go over the difficulty gap
                int nextDifficulty = DBManager.GetDifficulty(CoinName, way);

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

        public Way FindCorrectWay(string address, long startHeight)
        {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //preparing
            int difficulty = DBManager.GetDifficulty(CoinName, startHeight);

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
                wayList = GrowWays(wayList);

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

        public int GetDifficulty(long height)
        {
            return DBManager.GetDifficulty(CoinName, height);
        }

        public void AddBlock(List<Block> obj)
        {
            obj.ForEach(x => DBManager.Add(x));
        }

        public Block GetBlock(int height)
        {
            return DBManager.GetBlock(CoinName, height);
        }
    }
}
