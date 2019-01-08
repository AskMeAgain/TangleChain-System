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
    public class SimpleLogicManager : ILogicManager
    {

        public string CoinName { get; set; }

        private readonly IDataAccessor _dataAccessor;

        public SimpleLogicManager(IDataAccessor dBManager)
        {
            _dataAccessor = dBManager;
        }

        public Block GetSpecificBlock(string address, string hash)
        {
            return _dataAccessor.GetBlock(address, hash);
        }

        public Way FindCorrectWay(string address, long startHeight)
        {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //preparing
            int difficulty = GetDifficulty(startHeight);

            //first we get all blocks
            var allBlocks = _dataAccessor.GetBlocks(address)
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

        public int GetDifficulty(Way way)
        {
            return DBManager.GetDifficulty(CoinName, way);
        }

        public void AddBlock(List<Block> obj)
        {
            obj.ForEach(x => _dataAccessor.AddBlock(x));
        }

        private List<Transaction> GetAllTransactionsFromBlock(Block block)
        {
            return _dataAccessor.GetTransactionsFromBlock(block);
        }

        private List<Way> GrowWays(List<Way> ways)
        {

            var wayList = new List<Way>();

            foreach (Way way in ways)
            {

                //first we get this specific block
                Block specificBlock = GetSpecificBlock(way.CurrentBlock.SendTo, way.CurrentBlock.Hash);

                //compute now the next difficulty in case we go over the difficulty gap
                int nextDifficulty = GetDifficulty(way);

                //we then download everything in the next address
                List<Block> allBlocks = _dataAccessor.GetBlocks(specificBlock.NextAddress)
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
    }
}
