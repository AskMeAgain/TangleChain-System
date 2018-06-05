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


            string sql = "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                         "NextAddress CHAR(81) NOT NULL, Owner CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL)";

            string sql2 = "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Hash CHAR(81), Time LONG, _From CHAR(81), Signature CHAR(81)," +
                          "Mode INT,BlockID INT NOT NULL ,FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

            string sql3 = "CREATE TABLE IF NOT EXISTS Data (ID INTEGER PRIMARY KEY AUTOINCREMENT, _ArrayIndex INT NOT NULL, " +
                          "Data CHAR, TransID ,FOREIGN KEY (TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

            string sql4 = "CREATE TABLE IF NOT EXISTS Output (ID INTEGER PRIMARY KEY AUTOINCREMENT, _Values INT NOT NULL,_ArrayIndex INT NOT NULL, " +
                          "Receiver CHAR, TransID ID NOT NULL,FOREIGN KEY(TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";


            SQLiteCommand command = new SQLiteCommand(Db);
            SQLiteCommand command2 = new SQLiteCommand(Db);
            SQLiteCommand command3 = new SQLiteCommand(Db);
            SQLiteCommand command4 = new SQLiteCommand(Db);

            command.CommandText = sql;
            command.ExecuteNonQuery();

            command2.CommandText = sql2;
            command2.ExecuteNonQuery();

            command3.CommandText = sql3;
            command3.ExecuteNonQuery();

            command4.CommandText = sql4;
            command4.ExecuteNonQuery();

        }

        public bool AddBlock(Block block, bool storeTransactions) {

            //first check if block already exists in db
            if (GetBlock(block.Height) != null) {
                UpdateBlock(block, storeTransactions);
                return false;
            }

            string sql = string.Format("INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, Owner, SendTo) " +
                                       "VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}');",
            block.Height, block.Nonce, block.Time, block.Hash, block.NextAddress, block.Owner, block.SendTo);

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            command.ExecuteNonQuery();
          
            if (storeTransactions) {
                var transList = Core.GetAllTransactionsFromBlock(block);
                AddTransaction(transList, block.Height);
            }

            return true;

        }

        public void UpdateBlock(Block block, bool storeTransactions) {

            string sql = string.Format("Update Block SET Height='{0}', Nonce='{1}', Time='{2}',Hash='{3}', NextAddress='{4}',Owner='{5}',SendTo='{6}' WHERE Height='{0}'",
                block.Height, block.Nonce, block.Time, block.Hash, block.NextAddress, block.Owner, block.SendTo);

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            command.ExecuteNonQuery();

        }

        public void AddTransaction(List<Transaction> list, int BlockID) {

            list.ForEach(t => AddTransaction(t, BlockID));
        }

        public void AddTransaction(Transaction trans, int blockID) {

            //raw transaction
            string sql = string.Format("INSERT INTO Transactions (Hash, Time, _From, Signature, Mode, BlockID) " +
                                       "VALUES ('{0}',{1},'{2}','{3}',{4},{5}); SELECT last_insert_rowid();",
                trans.Hash,trans.Time,trans.From,trans.Signature,trans.Mode, blockID);

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader();

            reader.Read();

            long TransID = (long)reader[0];

            //add data too
            for (int i = 0; i < trans.Data.Count; i++) {          
           
                string sql2 = string.Format("INSERT INTO Data (_ArrayIndex, Data, TransID) VALUES({0},'{1}',{2});",i, trans.Data[i],TransID);

                SQLiteCommand cmd = new SQLiteCommand(Db);
                cmd.CommandText = sql2;
                cmd.ExecuteNonQuery();
            }

            //add receivers + output
            for (int i = 0; i < trans.OutputReceiver.Count; i++) {          
           
                string sql2 = string.Format("INSERT INTO Output (_Values, _ArrayIndex, Receiver, TransID) VALUES({0},{1},'{2}',{3});",trans.OutputValue[i], i,trans.OutputReceiver[i],TransID);

                SQLiteCommand cmd = new SQLiteCommand(Db);
                cmd.CommandText = sql2;
                cmd.ExecuteNonQuery();
            }
        }

        public Block GetBlock(int height) {

            string sql = string.Format("SELECT * FROM Block WHERE Height={0}", height);

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            Block block = new Block(reader, name);


            // Beenden des Readers und Freigabe aller Ressourcen.
            reader.Close();
            reader.Dispose();

            command.Dispose();

            return block;

        }

        public List<string> GetData(int id) {

            SQLiteCommand command = new SQLiteCommand(Db);

            var list = new List<string>();

            string sql = string.Format("SELECT * FROM Data WHERE TransID='{0}' ORDER BY _ArrayIndex;", id);

            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            while (true) {

                list.Add((string)reader[2]);

                if (!reader.Read())
                    break;
            }

            return list;
        }

        public List<int> GetValue(int id) {
            return null;
        }

        public List<string> GetReceiver(int id) {
            return null;
        }

        public Transaction GetTransaction(string hash, string sendTo) {

            //get normal data
            string sql = string.Format("SELECT * FROM Transaction WHERE Hash={0} && SendTo='{1}'", hash, sendTo);

            SQLiteCommand command = new SQLiteCommand(Db) {
                CommandText = sql
            };

            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            int ID = (int)reader[0];

            var dataList = GetData(ID);
            var valueList = GetValue(ID);
            var receiverList = GetReceiver(ID);

            Transaction trans = new Transaction(reader, valueList, receiverList, dataList);

            // Beenden des Readers und Freigabe aller Ressourcen.
            reader.Close();
            reader.Dispose();

            command.Dispose();

            return trans;
        }

    }
}
