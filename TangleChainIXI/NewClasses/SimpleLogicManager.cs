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
        private readonly IBalance _balanceComputer;
        private readonly IConsensus _consensus;

        public SimpleLogicManager(IDataAccessor dataAccessor, IBalance balanceComputer, IConsensus consensus)
        {
            _dataAccessor = dataAccessor;
            _balanceComputer = balanceComputer;
            _consensus = consensus;
        }

        public Block GetSpecificBlock(string address, string hash)
        {
            return _dataAccessor.GetBlock(address, hash);
        }

        public List<Block> FindCorrectWay(string address, long startHeight)
        {
            return _consensus.FindNewBlocks(address, startHeight, GetDifficulty(startHeight));
        }

        public int GetDifficulty(long? height)
        {
            if (height == null || height == 0)
                return 7;

            var chainSettings = _dataAccessor.GetChainSettings();
            long epochCount = chainSettings.DifficultyAdjustment;
            int goal = chainSettings.BlockTime;

            //height of last epoch before:
            long consolidationHeight = (long)height / epochCount * epochCount;

            //if we go below 0 with height, we use genesis block as HeightA, but this means we need to reduce epochcount by 1
            int flag = 0;

            //both blocktimes ... A happened before B
            long HeightOfA = consolidationHeight - 1 - epochCount;
            if (HeightOfA < 0)
            {
                HeightOfA = 0;
                flag = 1;
            }

            long? timeA = _dataAccessor.GetBlock(HeightOfA)?.Time;
            long? timeB = _dataAccessor.GetBlock(consolidationHeight - 1)?.Time;

            //if B is not null, then we can compute the new difficulty
            if (timeB == null || timeA == null)
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

        public void AddBlock(List<Block> obj)
        {
            obj.ForEach(x => _dataAccessor.AddBlock(x));
        }
    }
}
