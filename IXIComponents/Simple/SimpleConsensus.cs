using System.Collections.Generic;
using System.Linq;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Interfaces;

namespace IXIComponents.Simple
{
    public class SimpleConsensus : IConsensus
    {
        private IDataAccessor _dataAccessor;
        private ITangleAccessor _tangleAccessor;
        private IXISettings _settings;

        public SimpleConsensus(IXISettings settings, IDataAccessor dataAccessor, ITangleAccessor tangleAccessor)
        {
            _dataAccessor = dataAccessor;
            _tangleAccessor = tangleAccessor;
            _settings = settings;
        }

        public List<Block> FindNewBlocks(string address, long startHeight, int startDifficulty)
        {

            //this function finds the "longest" chain of blocks when given an address incase of a chainsplit

            //first we get all possible blocks
            var allBlocks = _tangleAccessor.GetAllFromAddress<Block>(address)
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

                if (size == wayList.Count && sumAfter <= (sumBefore + 1)) break;
            }

            //growth stopped now because we only added a single block
            //we choosed now the longest way
            if (wayList.Count == 0) return new List<Block>();

            return wayList.OrderByDescending(item => item.Length).First().ToBlockList();

        }

        private List<Way> GrowWays(List<Way> ways)
        {

            var wayList = new List<Way>();

            foreach (Way way in ways)
            {

                //first we get this specific block
                var specificBlock = _tangleAccessor.GetSpecificFromAddress<Block>(way.CurrentBlock.SendTo, way.CurrentBlock.Hash).Value;

                //compute now the next difficulty in case we go over the difficulty gap
                var nextDifficulty = GetTheoreticalDifficulty(way);

                //we then download everything in the next address
                var allBlocks = _tangleAccessor.GetAllFromAddress<Block>(specificBlock.NextAddress)
                    .Where(b => b.Height == specificBlock.Height + 1)
                    .Where(b => b.Verify(nextDifficulty))
                    .ToList();

                foreach (Block block in allBlocks)
                {
                    var temp = new Way(block);
                    temp.AddOldWay(way);

                    wayList.Add(temp);
                }
            }

            return wayList;

        }

        private int GetTheoreticalDifficulty(Way way)
        {
            if (way == null) return 7;

            var chainSettings = _dataAccessor.GetChainSettings().Value;
            var epochCount = chainSettings.DifficultyAdjustment;
            var goal = chainSettings.BlockTime;


            //height of last epoch before:
            var consolidationHeight = way.CurrentBlock.Height / epochCount * epochCount;

            //if we go below 0 with height, we use genesis block as HeightA, but this means we need to reduce epochcount by 1
            var flag = 0;

            //both blocktimes ... A happened before B
            var heightOfA = consolidationHeight - 1 - epochCount;
            if (heightOfA < 0)
            {
                heightOfA = 0;
                flag = 1;
            }

            //both blocktimes ... A happened before B
            var maybeBlockA = _dataAccessor.Get<Block>(heightOfA);
            var maybeBlockB = _dataAccessor.Get<Block>(consolidationHeight - 1);

            if (!maybeBlockA.HasValue || !maybeBlockB.HasValue) return 7;

            var timeA = way.GetWayViaHeight(heightOfA)?.CurrentBlock.Time ?? maybeBlockA.Value.Time;
            var timeB = way.GetWayViaHeight(consolidationHeight - 1)?.CurrentBlock.Time ?? maybeBlockB.Value.Time;

            //compute multiplier
            var multiplier = goal / ((timeB - timeA) / (float) (epochCount - flag));

            //get current difficulty
            var currentDifficulty = maybeBlockB.Value.Difficulty;

            //calculate the difficulty change
            var precedingZerosChange = Cryptography.CalculateDifficultyChange(multiplier);

            return currentDifficulty + precedingZerosChange;
        }

        public int GetDifficulty(long? height)
        {
            if (height == null || height == 0) return 7;

            var chainSettings = _dataAccessor.GetChainSettings().Value;

            long epochCount = chainSettings.DifficultyAdjustment;
            int goal = chainSettings.BlockTime;

            //height of last epoch before:
            long consolidationHeight = (long) height / epochCount * epochCount;

            //if we go below 0 with height, we use genesis block as HeightA, but this means we need to reduce epochcount by 1
            int flag = 0;

            //both blocktimes ... A happened before B
            long heightOfA = consolidationHeight - 1 - epochCount;
            if (heightOfA < 0)
            {
                heightOfA = 0;
                flag = 1;
            }

            var maybeBlockA = _dataAccessor.Get<Block>(heightOfA);
            var maybeBlockB = _dataAccessor.Get<Block>(consolidationHeight - 1);

            //if B is not null, then we can compute the new difficulty
            if (!maybeBlockA.HasValue || !maybeBlockB.HasValue) return 7;

            var timeA = maybeBlockA.Value.Time;
            var timeB = maybeBlockB.Value.Time;

            //compute multiplier
            var multiplier = goal / (float) (timeB - timeA) / (epochCount - flag);

            //get current difficulty
            var maybeCurrentBlock = _dataAccessor.Get<Block>(consolidationHeight - 1);

            if (!maybeCurrentBlock.HasValue) return 7;

            var currentDifficulty = maybeCurrentBlock.Value.Difficulty;

            //calculate the difficulty change
            var precedingZerosChange = Cryptography.CalculateDifficultyChange(multiplier);

            return currentDifficulty + precedingZerosChange;
        }
    }
}