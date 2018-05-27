using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TangleChain.Classes {

    public class DataBase {

        LiteDatabase Db;

        public DataBase(string name) {

            //create folder structure
            if (!Directory.Exists(@"C:\TangleChain\Chains\" + name + @"\")) {
                Directory.CreateDirectory(@"C:\TangleChain\Chains\" + name + @"\");
                Console.WriteLine("created");
            }

            //assign DB
            Db = new LiteDatabase(@"C:\TangleChain\Chains\" + name + @"\chain.db");
        }

        public bool IsWorking() {
            return (Db != null) ? true : false;
        }

        public void AddBlock(Block block, bool storeTransactions) {

            //upserting the block changes the block somehow, we need to have a new instance ... WEIRD HACK
            Block newBlock = new Block(block);

            LiteCollection<Block> collection = Db.GetCollection<Block>("Blocks");

            collection.Upsert(newBlock);
            //collection.EnsureIndex("Height");

            //if storeTransaction is true we also need to store the associated transactions
            if (storeTransactions) {
                List<string> hashes = newBlock.TransactionHashes;
                List<Transaction> transList = Core.GetAllTransactionsFromBlock(newBlock);
                AddTransactionToDatabase(transList);
            }
        }

        public Block GetBlock(int height) {

            LiteCollection<Block> collection = Db.GetCollection<Block>("Blocks");

            return collection.FindOne(m => m.Height == height);
        }

        public void AddTransactionToDatabase(List<Transaction> list) {

            LiteCollection<Transaction> collection = Db.GetCollection<Transaction>("Transactions");

            collection.Upsert(list);
            collection.EnsureIndex("Identity");
        }

        public Transaction GetTransaction(string sendTo, string Hash) {

            LiteCollection<Transaction> collection = Db.GetCollection<Transaction>("Transactions");

            return collection.FindOne(m => m.Identity.SendTo.Equals(sendTo) && m.Identity.Hash.Equals(Hash));
        }

        public int GetBalance(string user) {

            LiteCollection<Transaction> collection = Db.GetCollection<Transaction>("Transactions");

            int sum = 0;

            //all fees and reduction of your address
            sum += GetAllTransactionFees(user, collection);


            //all receiving transactions
            sum += GetAllReceivings(user, collection);

            return sum;
        }

        private int GetAllReceivings(string user, LiteCollection<Transaction> collection) {

            int sum = 0;

            var incoming = collection.Find(m => m.Output_Receiver.Contains(user));

            Console.WriteLine("count trans in: " + incoming.Count());


            foreach (Transaction trans in incoming) {

                for (int i = 0; i < trans.Output_Receiver.Count; i++) {
                    if (trans.Output_Receiver[i].Equals(user)) {
                        sum += trans.Output_Value[i];
                    }
                }
            }

            return sum;
        }

        public int GetAllTransactionFees(string user, LiteCollection<Transaction> collection) {

            List<Transaction> outcoming = collection.Find(m => m.From.Equals(user)).ToList();

            int sum = 0;

            foreach (Transaction trans in outcoming) {
                sum -= int.Parse(trans.Data[0]);

                if (trans.Output_Value.Count > 0) {
                    trans.Output_Value.ForEach(m => { sum -= m; });
                }
            }

            return sum;
        }
    }
}
