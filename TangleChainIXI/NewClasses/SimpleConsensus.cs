using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.NewClasses
{
    public class SimpleConsensus : IConsensus
    {
        public IDataAccessor _dataAccessor { get; set; }

        public SimpleConsensus(IDataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        public List<Block> FindNewBlocks(string address, long startHeight, int startDifficulty)
        {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //first we get all possible blocks
            var allBlocks = _dataAccessor.GetBlocks(address)
                .Where(b => b.Height == startHeight)
                .Where(b => b.Verify(startDifficulty))
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

            return wayList.OrderByDescending(item => item.Length).First().ToBlockList();

        }

        private List<Way> GrowWays(List<Way> ways)
        {

            var wayList = new List<Way>();

            foreach (Way way in ways)
            {

                //first we get this specific block
                Block specificBlock = _dataAccessor.GetBlock(way.CurrentBlock.SendTo, way.CurrentBlock.Hash);

                //compute now the next difficulty in case we go over the difficulty gap
                int nextDifficulty = GetTheoreticalDifficulty(way);

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

        private int GetTheoreticalDifficulty(Way way)
        {
            if (way == null)
                return 7;

            var chainSettings = _dataAccessor.GetChainSettings();
            long epochCount = chainSettings.DifficultyAdjustment;
            int goal = chainSettings.BlockTime;


            //height of last epoch before:
            long consolidationHeight = way.CurrentBlock.Height / epochCount * epochCount;

            //if we go below 0 with height, we use genesis block as HeightA, but this means we need to reduce epochcount by 1
            int flag = 0;

            //both blocktimes ... A happened before B
            long HeightOfA = consolidationHeight - 1 - epochCount;
            if (HeightOfA < 0)
            {
                HeightOfA = 0;
                flag = 1;
            }

            //both blocktimes ... A happened before B g
            long? timeA = way.GetWayViaHeight(HeightOfA)?.CurrentBlock.Time ?? _dataAccessor.GetBlock(HeightOfA)?.Time;
            long? timeB = way.GetWayViaHeight(consolidationHeight - 1)?.CurrentBlock.Time ?? _dataAccessor.GetBlock(consolidationHeight - 1)?.Time;

            if (timeA == null || timeB == null)
                return 7;

            //compute multiplier
            float multiplier = goal / (((long)timeB - (long)timeA) / (epochCount - flag));

            //get current difficulty
            int? currentDifficulty = _dataAccessor.GetBlock(consolidationHeight - 1)?.Difficulty;

            if (currentDifficulty == null)
                return 7;

            //calculate the difficulty change
            var precedingZerosChange = Cryptography.CalculateDifficultyChange(multiplier);

            //overloaded minus operator for difficulty
            return (int)currentDifficulty + precedingZerosChange;
        }
    }
}
