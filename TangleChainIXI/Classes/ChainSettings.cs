using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace TangleChainIXI.Classes {
    public class ChainSettings {

        public ChainSettings(SQLiteDataReader reader) {
        
            reader.Read();
            BlockReward = int.Parse(reader.GetValue(0).ToString());
            reader.Read();
            RewardReduction = int.Parse(reader.GetValue(0).ToString());
            reader.Read();
            ReductionFactor = int.Parse(reader.GetValue(0).ToString());
            reader.Read();
            TransactionsPerBlock = int.Parse(reader.GetValue(0).ToString());
            reader.Read();
            BlockTime = int.Parse(reader.GetValue(0).ToString());
            reader.Read();
            TransactionPoolInterval = int.Parse(reader.GetValue(0).ToString());
            reader.Read();
            DifficultyAdjustment = int.Parse(reader.GetValue(0).ToString());
        }

        public int BlockReward { get; set; }
        public int RewardReduction { get; set; }
        public int ReductionFactor { get; set; }
        public int TransactionsPerBlock { get; set; }
        public int BlockTime { get; set; }
        public int TransactionPoolInterval { get; set; }
        public int DifficultyAdjustment { get; set; }

        public ChainSettings(int blockReward, int rewardReduction, int reductionFactor, int blocksize, int blocktime, int poolInterval, int difficultyAdjustment) {

            BlockReward = blockReward;
            RewardReduction = rewardReduction;
            ReductionFactor = reductionFactor;
            TransactionsPerBlock = blocksize;
            BlockTime = blocktime;
            TransactionPoolInterval = poolInterval;
            DifficultyAdjustment = difficultyAdjustment;
        }

        public void Print() {

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
