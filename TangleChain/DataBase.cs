using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using TangleChain.Classes;

namespace TangleChain {

    public class DataBase {

        private LiteDatabase Db { get; set; }

        public DataBase(string name) {

            //create folder structure
            if (!Directory.Exists(@"C:\TangleChain\Chains\" + name + @"\")) {
                Directory.CreateDirectory(@"C:\TangleChain\Chains\" + name + @"\");
                Console.WriteLine("created");
            }

            //assign DB
            Db = new LiteDatabase(@"C:\TangleChain\Chains\" + name + @"\chain.db");
        }

        public void AddBlock(Block block, bool storeTransactions) {

            //upserting the block changes the block somehow, we need to have a new instance ... WEIRD HACK
            Block newBlock = new Block(block);

            var collection = Db.GetCollection<Block>("Blocks");

            collection.Upsert(newBlock);
            collection.EnsureIndex("Height");

            //if storeTransaction is true we also need to store the associated transactions
            if (storeTransactions) {
                var transList = Core.GetAllTransactionsFromBlock(newBlock);
                AddTransactionToDatabase(transList);
            }
        }

        public Block GetBlock(int height) {

            var collection = Db.GetCollection<Block>("Blocks");

            return collection.FindOne(m => m.Height == height);
        }

        public void AddTransactionToDatabase(List<Transaction> list) {

            var collection = Db.GetCollection<Transaction>("Transactions");

            collection.Upsert(list);
            collection.EnsureIndex("Identity");
        }

        public Transaction GetTransaction(string sendTo, string hash) {

            var collection = Db.GetCollection<Transaction>("Transactions");

            return collection.FindOne(m => m.SendTo.Equals(sendTo) && m.Hash.Equals(hash));
        }

        public bool IsWorking() {
            return Db != null;
        }

        public int GetBalance(string user) {

            int sum = 0;

            //all fees and reduction of your address
            sum += GetAllTransactionFees(user);

            //miner gets coins for each mined block
            sum += GetAllMiningRewards(user);

            //all receiving transactions
            sum += GetAllReceivings(user);

            return sum;
        }

        private int GetAllMiningRewards(string user) {

            var collection = Db.GetCollection<Block>("Blocks");

            collection.Find(m => m.Owner.Equals(user));

            return collection.Count() * Settings.MiningReward;

        }

        public int GetAllReceivings(string user) {

            var collection = Db.GetCollection<Transaction>("Transactions");

            int sum = 0;
            var incoming = collection.Find(m => m.OutputReceiver.Contains(user));

            Console.WriteLine("count trans in: " + incoming.Count());

            foreach (Transaction trans in incoming) {
                for (int i = 0; i < trans.OutputReceiver.Count; i++) {
                    if (trans.OutputReceiver[i].Equals(user)) {
                        sum += trans.OutputValue[i];
                    }
                }
            }

            return sum;
        }

        public int GetAllTransactionFees(string user) {

            var collection = Db.GetCollection<Transaction>("Transactions");
            var outcomingTransList = collection.Find(m => m.From.Equals(user)).ToList();

            int sum = 0;

            foreach (Transaction trans in outcomingTransList) {
                sum -= int.Parse(trans.Data[0]);

                if (trans.OutputValue.Count > 0) {
                    trans.OutputValue.ForEach(m => { sum -= m; });
                }
            }

            return sum;
        }



        


    }
}
