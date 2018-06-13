using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace TangleChainIXI.Classes {
    public class ChainSettings {

        public ChainSettings(SQLiteDataReader reader) {

            reader.Read();
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

        }

        public int BlockReward { get; set; }
        public int RewardReduction { get; set; }
        public int ReductionFactor { get; set; }
        public int TransactionsPerBlock { get; set; }
        public int BlockTime { get; set; }
        public int TransactionPoolInterval { get; set; }

        public ChainSettings() {
            BlockReward = 100;
            RewardReduction = -1;
            ReductionFactor = 0;
            TransactionsPerBlock = 4;
            BlockTime = 100;
            TransactionPoolInterval = 5;
        }

        public void Print() {

            string s = $"BlockReward: {BlockReward} " +
                       $"\nRewardReduction: {RewardReduction} " +
                       $"\nReductionFactor: {ReductionFactor} " +
                       $"\nTransactionsPerBlock: {TransactionsPerBlock}";

            Console.WriteLine(s);

        }

    }
}
