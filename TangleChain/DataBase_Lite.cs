using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TangleChain.Classes;

namespace TangleChain {

    public class DataBase_Lite {

        private SQLiteConnection Db { get; set; }
        public string name { get; set; }

        public DataBase_Lite(string n) {

            name = n;

            //first we create file structure
            if (!Directory.Exists(@"C:\TangleChain\Chains\" + n + @"\")) {
                Directory.CreateDirectory(@"C:\TangleChain\Chains\" + n + @"\");
            }

            Db = new SQLiteConnection();
 
            Db.ConnectionString = @"Data Source=c:\TangleChain\Chains\" + n + @"\chain.db; Version=3;";
            Db.Open();

            SQLiteCommand command = new SQLiteCommand(Db);

            //create now the tables:
            //block table
            string sql =
                "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                "NextAddress CHAR(81) NOT NULL, Owner CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL)";
               
            command.CommandText = sql;
            command.ExecuteNonQuery();

        }

        public bool AddBlock(Block block, bool storeTransactions) {

            //first check if block already exists in db
            if(GetBlock(block.Height) != null)
                return false;
            
            string sql = string.Format("INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, Owner, SendTo) VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}')",
            block.Height, block.Nonce,block.Time,block.Hash,block.NextAddress,block.Owner,block.SendTo);

            SQLiteCommand command = new SQLiteCommand(Db);              
            command.CommandText = sql;
            command.ExecuteNonQuery();

            return true;
            
        }

        //public void AddTransaction(Transaction trans) {

        //    Db.CreateTable<Transaction>();

        //    Transaction checkTrans = Db.Table<Transaction>().FirstOrDefault(m => m.Hash.Equals(trans.Hash) && m.SendTo.Equals(trans.SendTo));

        //    if (checkTrans == null)
        //        Db.Insert(trans);
        //    else
        //        Db.Update(trans);

        //}

        public Block GetBlock(int height) {

            string sql = string.Format("SELECT * FROM Block WHERE Height={0}", height);

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();

            if(!reader.Read())
                return null;

            Block block = new Block(reader, name);
            

            // Beenden des Readers und Freigabe aller Ressourcen.
            reader.Close();
            reader.Dispose();
 
            command.Dispose();

            return block;

        }

        //public Transaction GetTransaction(string hash, string sendTo) {
        //    return Db.Table<Transaction>().FirstOrDefault(m => m.Hash.Equals(hash) && m.SendTo.Equals(sendTo));
        //}

    }
}
