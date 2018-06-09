﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TangleChainIXI.Classes;

namespace TangleChainIXI {

    public class DataBase {

        private SQLiteConnection Db { get; set; }
        public string CoinName { get; set; }

        public DataBase(string name) {

            CoinName = name;
            string path = Settings.StorePath;

            //first we create file structure
            if (!Directory.Exists($@"{path}{name}\")) {
                Directory.CreateDirectory($@"{path}{name}\");
            }

            Db = new SQLiteConnection($@"Data Source={path}{name}\chain.db; Version=3;");
            Db.Open();


            string sql = "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                         "NextAddress CHAR(81) NOT NULL, Owner CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL)";

            string sql2 = "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Hash CHAR(81), Time LONG, _From CHAR(81), Signature CHAR(81)," +
                          "Mode INT,BlockID INT NOT NULL ,FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

            string sql3 = "CREATE TABLE IF NOT EXISTS Data (ID INTEGER PRIMARY KEY AUTOINCREMENT, _ArrayIndex INT NOT NULL, " +
                          "Data CHAR, TransID ,FOREIGN KEY (TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

            string sql4 = "CREATE TABLE IF NOT EXISTS Output (ID INTEGER PRIMARY KEY AUTOINCREMENT, _Values INT NOT NULL,_ArrayIndex INT NOT NULL, " +
                          "Receiver CHAR, TransID ID NOT NULL,FOREIGN KEY(TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

            NoQuerySQL(sql);
            NoQuerySQL(sql2);
            NoQuerySQL(sql3);
            NoQuerySQL(sql4);

        }

        public bool AddBlock(Block block, bool storeTransactions) {

            bool flag = true;

            //first check if block already exists in db
            if (GetBlock(block.Height) != null) {
                DeleteBlock(block.Height);
                flag = false;
            }

            string sql = $"INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, Owner, SendTo) " +
                         $"VALUES ({block.Height},{block.Nonce},{block.Time},'{block.Hash}','{block.NextAddress}','{block.Owner}','{block.SendTo}');";

            NoQuerySQL(sql);

            if (storeTransactions) {
                var transList = Core.GetAllTransactionsFromBlock(block);

                if (transList != null)
                    AddTransaction(transList, block.Height);
            }

            return flag;

        }

        public void DeleteBlock(long height) {

            //delete block
            string sql = $"DELETE FROM Block WHERE Height={height}";

            NoQuerySQL(sql);
        }

        public Block GetBlock(long height) {

            string sql = $"SELECT * FROM Block WHERE Height={height}";
            SQLiteDataReader reader = QuerySQL(sql);

            if (!reader.Read())
                return null;

            Block block = new Block(reader, CoinName);

            return block;

        }


        public Transaction GetTransaction(string hash, long height) {

            //get normal data
            string sql = $"SELECT * FROM Transactions WHERE Hash='{hash}' AND BlockID='{height}'";

            SQLiteDataReader reader = QuerySQL(sql);

            if (!reader.Read())
                return null;

            long ID = (long)reader[0];
            var output = GetTransactionOutput(ID);

            Transaction trans = new Transaction(reader, output.Item1, output.Item2, GetTransactionData(ID));

            trans.SendTo = Utils.GetTransactionPoolAddress(height, CoinName);

            reader.Close();
            reader.Dispose();

            return trans;
        }

        public void AddTransaction(List<Transaction> list, long BlockID) {

            list.ForEach(t => AddTransaction(t, BlockID));
        }

        public void AddTransaction(Transaction trans, long blockID) {

            //raw transaction
            string sql = $"INSERT INTO Transactions (Hash, Time, _From, Signature, Mode, BlockID) " +
                         $"VALUES ('{trans.Hash}',{trans.Time},'{trans.From}','{trans.Signature}',{trans.Mode},{blockID}); SELECT last_insert_rowid();";

            SQLiteDataReader reader = QuerySQL(sql);
            reader.Read();

            long TransID = (long)reader[0];

            //add data too
            for (int i = 0; i < trans.Data.Count; i++) {

                string sql2 = $"INSERT INTO Data (_ArrayIndex, Data, TransID) VALUES({i},'{trans.Data[i]}',{TransID});";

                NoQuerySQL(sql2);
            }

            //add receivers + output
            for (int i = 0; i < trans.OutputReceiver.Count; i++) {

                string sql2 = $"INSERT INTO Output (_Values, _ArrayIndex, Receiver, TransID) VALUES({trans.OutputValue[i]},{i},'{trans.OutputReceiver[i]}',{TransID});";

                NoQuerySQL(sql2);
            }
        }

        private List<string> GetTransactionData(long id) {

            //i keep the structure here because data could be zero and i need to correctly setup everything

            SQLiteCommand command = new SQLiteCommand(Db);

            var list = new List<string>();

            string sql = $"SELECT * FROM Data WHERE TransID={id} ORDER BY _ArrayIndex;";

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

        private (List<int>, List<string>) GetTransactionOutput(long id) {
            //i keep the structure here because data could be zero and i need to correctly setup everything

            SQLiteCommand command = new SQLiteCommand(Db);

            var listReceiver = new List<string>();
            var listValue = new List<int>();

            string sql = $"SELECT * FROM Output WHERE TransID={id};";

            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return (null, null);

            while (true) {

                listValue.Add((int)reader[1]);
                listReceiver.Add((string)reader[3]);

                if (!reader.Read())
                    break;
            }

            return (listValue, listReceiver);
        }

        public Block GetLatestBlock() {

            //we first get highest ID
            SQLiteDataReader reader = QuerySQL($"SELECT MAX(Height) FROM Block");

            reader.Read();

            return GetBlock((long)reader[0]);

        }

#region Get Balance stuff

        public long GetBalance(string user) {

            //count all incoming money
            //get all block rewards & transfees
            var blockReward = GetBlockReward(user);

            //get all outputs who point towards you
            var OutputSum = GetIncomingOutputs(user);


            //count now all removing money
            //remove all outputs which are outgoing
            var OutgoingOutputs = GetOutgoingOutputs(user);

            ////remove all transaction fees
            var OutgoingTransfees = GetOutcomingTransFees(user);

            return blockReward + OutputSum + OutgoingOutputs + OutgoingTransfees;
        }

        private long GetOutcomingTransFees(string user) {
            string transFees = $"SELECT IFNULL(SUM(Data),0) FROM Transactions JOIN Data ON Transactions.ID = Data.TransID WHERE _From='{user}'";
            SQLiteDataReader reader = QuerySQL(transFees);
            reader.Read();

            return (long)reader[0] * -1;
        }

        private long GetOutgoingOutputs(string user) {
            string sql_Outgoing = $"SELECT IFNULL(SUM(_Values),0) FROM Transactions JOIN Output ON Transactions.ID = Output.TransID WHERE _From='{user}';";
            SQLiteDataReader reader = QuerySQL(sql_Outgoing);
            reader.Read();

            return (long)reader[0] * -1;

        }

        private long GetIncomingOutputs(string user) {
            string sql_Outputs = $"SELECT IFNULL(SUM(_Values),0) FROM Output WHERE Receiver='{user}';";
            SQLiteDataReader reader = QuerySQL(sql_Outputs);
            reader.Read();

            return (long)reader[0];
        }

        private long GetBlockReward(string user) {

            SQLiteDataReader reader;

            //Every block gives a reward
            string sql_blockRewards = string.Format("SELECT IFNULL(COUNT(Owner),0) FROM Block WHERE Owner='{0}'", user);
            reader = QuerySQL(sql_blockRewards);
            reader.Read();

            long blockReward = (long)reader[0];

            ////you also get transaction fees
            string sql_TransFees = string.Format("SELECT IFNULL(SUM(Data),0) FROM Block AS b JOIN Transactions AS t ON " +
                                                "b.Height = t.BlockID JOIN Data as d ON t.ID = d.TransID " +
                                                "WHERE Owner = '{0}';", user);
            reader = QuerySQL(sql_TransFees);
            reader.Read();

            long TransFees = (long)reader[0];

            return blockReward + TransFees;
        }

        #endregion

        #region SQL Utils

        public SQLiteDataReader QuerySQL(string sql) {
            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }

        public void NoQuerySQL(string sql) {
            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            command.ExecuteNonQuery();

            command.Dispose();
        }

        #endregion
    }


}
