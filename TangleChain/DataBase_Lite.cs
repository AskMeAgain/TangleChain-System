using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TangleChain.Classes;

namespace TangleChain {

    public class DataBase_Lite {

        private SQLiteConnection Db { get; set; }
        public string CoinName { get; set; }

        public DataBase_Lite(string name) {

            CoinName = name;

            //first we create file structure
            if (!Directory.Exists(@"C:\TangleChain\Chains\" + name + @"\")) {
                Directory.CreateDirectory(@"C:\TangleChain\Chains\" + name + @"\");
            }

            Db = new SQLiteConnection();

            Db.ConnectionString = @"Data Source=c:\TangleChain\Chains\" + name + @"\chain.db; Version=3;";
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

            bool flag = true;

            //first check if block already exists in db
            if (GetBlock(block.Height) != null) {
                DeleteBlock(block, storeTransactions);
                flag = false;
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

            return flag;

        }

        public void DeleteBlock(Block block, bool storeTransactions) {

            //delete block
            string sql = string.Format("DELETE FROM Block WHERE Height={0}", block.Height);     

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

            Block block = new Block(reader, CoinName);


            // Beenden des Readers und Freigabe aller Ressourcen.
            reader.Close();
            reader.Dispose();

            command.Dispose();

            return block;

        }

        public List<string> GetData(long id) {

            SQLiteCommand command = new SQLiteCommand(Db);

            var list = new List<string>();

            string sql = string.Format("SELECT * FROM Data WHERE TransID={0} ORDER BY _ArrayIndex;", id);

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

        public (List<int>, List<string>) GetOutput(long id) {

            SQLiteCommand command = new SQLiteCommand(Db);

            var listReceiver = new List<string>();
            var listValue = new List<int>();

            string sql = string.Format("SELECT * FROM Output WHERE TransID={0};", id);

            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return (null,null);

            while (true) {

                listValue.Add((int)reader[1]);
                listReceiver.Add((string)reader[3]);

                if (!reader.Read())
                    break;
            }

            return (listValue,listReceiver);
        }

        public Transaction GetTransaction(string hash, int height) {

            //get normal data
            string sql = string.Format("SELECT * FROM Transactions WHERE Hash='{0}' AND BlockID='{1}'", hash, height);

            SQLiteCommand command = new SQLiteCommand(Db) {
                CommandText = sql
            };

            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            long ID = (long)reader[0];
            var output = GetOutput(ID);

            Transaction trans = new Transaction(reader, output.Item1, output.Item2, GetData(ID));

            trans.SendTo = Utils.GetTransactionPoolAddress(height, CoinName);


            reader.Close();
            reader.Dispose();
            command.Dispose();

            return trans;
        }



    }
}
