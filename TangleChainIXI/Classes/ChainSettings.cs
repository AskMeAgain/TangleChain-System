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

        }

        public int BlockReward { get; set; }
        public int RewardReduction { get; set; }
        public int ReductionFactor { get; set; }
        public int TransactionsPerBlock { get; set; }


        public void Print() {

            string s = $"BlockReward: {BlockReward} " +
                       $"\nRewardReduction: {RewardReduction} " +
                       $"\nReductionFactor: {ReductionFactor} " +
                       $"\nTransactionsPerBlock: {TransactionsPerBlock}";

            Console.WriteLine(s);

        }

    }
}
