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

            //create now the tables:
            //block table
            string sql =
                "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                "NextAddress CHAR(81) NOT NULL, Owner CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL)";
                                       
            Console.WriteLine(sql);

            Db.Execute(sql);

        }

        public void AddBlock(Block block, bool storeTransactions) {

            string sql = string.Format("INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, Owner, SendTo) VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}')",
            block.Height, block.Nonce,block.Time,block.Hash,block.NextAddress,block.Owner,block.SendTo);

            Db.Execute(sql);
        }

        //public void AddTransaction(Transaction trans) {

        //    Db.CreateTable<Transaction>();

        //    Transaction checkTrans = Db.Table<Transaction>().FirstOrDefault(m => m.Hash.Equals(trans.Hash) && m.SendTo.Equals(trans.SendTo));

        //    if (checkTrans == null)
        //        Db.Insert(trans);
        //    else
        //        Db.Update(trans);

        //}

        public void GetBlock(int height) {

            string sql = string.Format("SELECT * FROM Block WHERE Height=2;");
            Db.Execute(sql);
        }

        //public Transaction GetTransaction(string hash, string sendTo) {
        //    return Db.Table<Transaction>().FirstOrDefault(m => m.Hash.Equals(hash) && m.SendTo.Equals(sendTo));
        //}

    }
}
