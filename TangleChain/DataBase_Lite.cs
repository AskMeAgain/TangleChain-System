using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TangleChain.Classes;

namespace TangleChain {

    public class DataBase_Lite {

        private SQLiteConnection Db { get; set; }

        public DataBase_Lite(string name) {

            //first we create file structure
            if (!Directory.Exists(@"C:\TangleChain\Chains\" + name + @"\")) {
                Directory.CreateDirectory(@"C:\TangleChain\Chains\" + name + @"\");
            }

            //assign DB
            Db = new SQLiteConnection(@"C:\TangleChain\Chains\" + name + @"\chain.db");

        }

        public void AddBlock(Block block, bool storeTransactions) {

            Db.CreateTable<Block>();

            Block checkBlock = Db.Table<Block>().FirstOrDefault(m => m.Height.Equals(block.Height));

            if (checkBlock == null)
                Db.Insert(block);
            else
                Db.Update(block);

            //if storeTransaction is true we also need to store the associated transactions
            if (storeTransactions) {
                //var transList = Core.GetAllTransactionsFromBlock(newBlock);
                //AddTransactionToDatabase(transList);
            }
        }

        public void AddTransaction(Transaction trans) {

            Db.CreateTable<Transaction>();

            Transaction checkTrans = Db.Table<Transaction>().Where(m => m.Identity.Equals(trans.Identity)).FirstOrDefault();

            if (checkTrans == null)
                Db.Insert(trans);
            else
                Db.Update(trans);

        }

        public Block GetBlock(int height) {
            return Db.Table<Block>().Where(m => m.Height == height).FirstOrDefault();
        }

        public Transaction GetTransaction(Transaction.ID identity) {
            return Db.Table<Transaction>().FirstOrDefault(m => m.Identity.Equals(identity));
        }

    }
}
