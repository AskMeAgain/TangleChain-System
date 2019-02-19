using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Classes
{
    public class ChainSettings
    {

        public int BlockReward { get; set; }
        public int RewardReduction { get; set; }
        public int ReductionFactor { get; set; }
        public int TransactionsPerBlock { get; set; }
        public int BlockTime { get; set; }
        public int TransactionPoolInterval { get; set; }
        public int DifficultyAdjustment { get; set; }

        /// <summary>
        /// The Standard constructor
        /// </summary>
        /// <param name="blockReward">The Blockreward for each miner who uploads a correct block</param>
        /// <param name="rewardReduction">WIP</param>
        /// <param name="reductionFactor">WIP</param>
        /// <param name="blocksize">The number of transactions & smartcontracts which can be stored inside a single block </param>
        /// <param name="blocktime">The targeted time between two blocks in seconds</param>
        /// <param name="poolInterval">The interval for a new pooladdress.</param>
        /// <param name="difficultyAdjustment">The number of blocks after we change the difficulty</param>
        public ChainSettings(int blockReward, int rewardReduction, int reductionFactor, int blocksize, int blocktime, int poolInterval, int difficultyAdjustment)
        {

            BlockReward = blockReward;
            RewardReduction = rewardReduction;
            ReductionFactor = reductionFactor;
            TransactionsPerBlock = blocksize;
            BlockTime = blocktime;
            TransactionPoolInterval = poolInterval;
            DifficultyAdjustment = difficultyAdjustment;
        }

        public ChainSettings(string[] args)
        {

            BlockReward = int.Parse(args[0]);
            RewardReduction = int.Parse(args[1]);
            ReductionFactor = int.Parse(args[2]);
            TransactionsPerBlock = int.Parse(args[3]);
            BlockTime = int.Parse(args[4]);
            TransactionPoolInterval = int.Parse(args[5]);
            DifficultyAdjustment = int.Parse(args[6]);

        }

        public void Print()
        {

            string s = $"BlockReward: {BlockReward} " +
                       $"\nRewardReduction: {RewardReduction} " +
                       $"\nReductionFactor: {ReductionFactor} " +
                       $"\nTransactionsPerBlock: {TransactionsPerBlock} " +
                       $"\nTransactionPoolInterval: {TransactionPoolInterval}" +
                       $"\nDifficultyAdjustment: {DifficultyAdjustment} ";

            Console.WriteLine(s);

        }

    }
}
