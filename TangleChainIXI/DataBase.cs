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

                string sql =
                    "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                    "NextAddress CHAR(81) NOT NULL, PublicKey CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL);";

                string sql2 =
                    "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Hash CHAR(81), Time LONG, _From CHAR(81), Signature CHAR(81)," +
                    "Mode INT,BlockID INT ,OutputValue INT NOT NULL,PoolHeight INT, FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

                string sql3 =
                    "CREATE TABLE IF NOT EXISTS Data (ID INTEGER PRIMARY KEY AUTOINCREMENT, _ArrayIndex INT NOT NULL, " +
                    "Data CHAR, TransID ,FOREIGN KEY (TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

                string sql4 =
                    "CREATE TABLE IF NOT EXISTS Output (ID INTEGER PRIMARY KEY AUTOINCREMENT, _Values INT NOT NULL,_ArrayIndex INT NOT NULL, " +
                    "Receiver CHAR, TransID ID NOT NULL,FOREIGN KEY(TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

                NoQuerySQL(sql);
                NoQuerySQL(sql2);
                NoQuerySQL(sql3);
                NoQuerySQL(sql4);

            }

        }

        public bool AddBlock(Block block, bool storeTransactions) {

            bool flag = true;

            //no update when genesis block because of concurrency stuff (stupid hack)
            if (block.Height == 0 && GetBlock(block.Height) != null) {
                return false;
            }

            //first check if block already exists in db in a different version
            Block b = GetBlock(block.Height);
            if (b != null && !b.Hash.Equals(block.Hash)) {
                DeleteBlock(block.Height);
                flag = false;
            }


            string sql = $"INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, PublicKey, SendTo) " +
                         $"VALUES ({block.Height},{block.Nonce},{block.Time},'{block.Hash}','{block.NextAddress}','{block.Owner}','{block.SendTo}');";
            //TODO IF NOT EXISTS!

            NoQuerySQL(sql);

            if (storeTransactions && block.TransactionHashes != null) {
                var transList = Core.GetAllTransactionsFromBlock(block);

                if (transList != null)
                    AddTransaction(transList, block.Height, null);

                if (block.Height == 0) {
                    //we set settings too
                    ChainSettings settings = GetChainSettings();
                    Settings.AddChainSettings(CoinName, settings);
                }
            }



            return flag;

        }

        public void DeleteDatabase() {

            string path = Settings.StorePath;

            Directory.Delete($@"{path}{CoinName}\", true);

        }

        public void DeleteBlock(long height) {

            //delete block
            string sql = $"PRAGMA foreign_keys = ON;DELETE FROM Block WHERE Height={height}";

            NoQuerySQL(sql);
        }

        public Block GetBlock(long height) {

            string sql = $"SELECT * FROM Block WHERE Height={height}";

            using (SQLiteDataReader reader = QuerySQL(sql)) {

                if (!reader.Read()) {
                    return null;
                }

                return new Block(reader, CoinName);
            }

        }

        public Transaction GetTransaction(string hash, long height) {

            //get normal data
            string sql = $"SELECT * FROM Transactions WHERE Hash='{hash}' AND BlockID='{height}'";

            using (SQLiteDataReader reader = QuerySQL(sql)) {

                if (!reader.Read())
                    return null;

                long ID = (long)reader[0];
                var output = GetTransactionOutput(ID);

                Transaction trans = new Transaction(reader, output.Item1, output.Item2, GetTransactionData(ID)) {
                    SendTo = Utils.GetTransactionPoolAddress(height, CoinName)
                };

                return trans;
            }
        }

        public List<Transaction> GetTransPool(long height, int num) {

            //get normal data
            string sql = $"SELECT * FROM Transactions WHERE PoolHeight={height} ORDER BY OutputValue DESC LIMIT {num};";

            var transList = new List<Transaction>();

            using (SQLiteDataReader reader = QuerySQL(sql)) {
                for (int i = 0; i < num; i++) {

                    if (!reader.Read())
                        return null;

                    long ID = (long)reader[0];
                    var output = GetTransactionOutput(ID);

                    Transaction trans = new Transaction(reader, output.Item1, output.Item2, GetTransactionData(ID)) {
                        SendTo = Utils.GetTransactionPoolAddress(height, CoinName)
                    };

                    transList.Add(trans);

                }
            }

            return transList;
        }

        public void AddTransaction(List<Transaction> list, long? blockID, long? poolHeight) {
            list.ForEach(t => AddTransaction(t, blockID, poolHeight));
        }

        public void AddTransaction(Transaction trans, long? blockID, long? poolHeight) {

            //data
            long TransID = -1;

            string insertPool = "INSERT INTO Transactions (Hash, Time, _FROM, Signature, Mode, BlockID, OutputValue, PoolHeight) " +
                                $"SELECT'{trans.Hash}', {trans.Time}, '{trans.From}', '{trans.Signature}', {trans.Mode}, {IsNull(blockID)}, {trans.ComputeOutgoingValues()}, {IsNull(poolHeight)}" +
                                $" WHERE NOT EXISTS (SELECT 1 FROM Transactions WHERE Hash='{trans.Hash}' AND Time={trans.Time}); SELECT last_insert_rowid();";

            //Case 1 i insert a transpool transaction
            if (poolHeight != null) {

                using (SQLiteDataReader reader = QuerySQL(insertPool)) {
                    reader.Read();
                    TransID = (long)reader[0];
                }

                StoreData(trans, TransID);

            }

            //Case 2 i insert a normal trans from block:
            if (blockID != null) {

                //if normal trans is already there because of it was included in transpool, we need to update it first.
                string sql = $"UPDATE Transactions SET BlockID={blockID}, PoolHeight=NULL WHERE Hash='{trans.Hash}' AND Time={trans.Time} AND PoolHeight IS NOT NULL;";

                int numOfAffected = NoQuerySQL(sql);

                if (numOfAffected == 0) {
                    using (SQLiteDataReader reader = QuerySQL(insertPool)) {
                        reader.Read();
                        TransID = (long)reader[0];
                    }

                    StoreData(trans, TransID);

                }

            }


        }

        private void StoreData(Transaction trans, long TransID) {
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

            using (SQLiteDataReader reader = command.ExecuteReader()) {

                if (!reader.Read())
                    return null;

                while (true) {

                    list.Add((string)reader[2]);

                    if (!reader.Read())
                        break;
                }
            }

            return list;
        }

        private (List<int>, List<string>) GetTransactionOutput(long id) {
            //i keep the structure here because data could be zero and i need to correctly setup everything

            SQLiteCommand command = new SQLiteCommand(Db);

            var listReceiver = new List<string>();
            var listValue = new List<int>();

            string sql = $"SELECT _Values,Receiver FROM Output WHERE TransID={id};";

            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader()) {

                if (!reader.Read()) {
                    return (null, null);
                }

                while (true) {

                    listValue.Add((int)reader[0]);
                    listReceiver.Add((string)reader[1]);

                    if (!reader.Read())
                        break;
                }

            }

            return (listValue, listReceiver);
        }

        public Block GetLatestBlock() {

            //we first get highest ID
            using (SQLiteDataReader reader = QuerySQL($"SELECT IFNULL(MAX(Height),0) FROM Block")) {

                if (!reader.Read())
                    return null;

                return GetBlock((long)reader[0]);
            }
        }

        public ChainSettings GetChainSettings() {

            string sql = "SELECT Data FROM Block AS b " +
                         "JOIN Transactions AS t ON b.Height = t.BlockID " +
                         "JOIN Data as d ON t.ID = d.TransID " +
                         "WHERE Height = 0 " +
                         "ORDER BY _ArrayIndex;";

            using (SQLiteDataReader reader = QuerySQL(sql)) {
                ChainSettings settings = new ChainSettings(reader);
                return settings;
            }
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
            string transFees = $"SELECT IFNULL(SUM(Data),0) FROM Transactions JOIN Data ON Transactions.ID = Data.TransID WHERE _From='{user}' AND _ArrayIndex = 0";

            using (SQLiteDataReader reader = QuerySQL(transFees)) {
                reader.Read();
                return (long)reader[0] * -1;
            }
        }

        private long GetOutgoingOutputs(string user) {
            string sql_Outgoing = $"SELECT IFNULL(SUM(_Values),0) FROM Transactions JOIN Output ON Transactions.ID = Output.TransID WHERE _From='{user}' AND NOT Mode = -1;";

            using (SQLiteDataReader reader = QuerySQL(sql_Outgoing)) {
                reader.Read();
                return (long)reader[0] * -1;
            }
        }

        private long GetIncomingOutputs(string user) {
            string sql_Outputs = $"SELECT IFNULL(SUM(_Values),0) FROM Output WHERE Receiver='{user}';";

            using (SQLiteDataReader reader = QuerySQL(sql_Outputs)) {
                reader.Read();
                return (long)reader[0];
            }
        }

        private long GetBlockReward(string user) {

            string sql_blockRewards = $"SELECT IFNULL(COUNT(PublicKey),0) FROM Block WHERE PublicKey='{user}'";

            string sql_TransFees = "SELECT IFNULL(SUM(Data),0) FROM Block AS b JOIN Transactions AS t ON " +
                "b.Height = t.BlockID JOIN Data as d ON t.ID = d.TransID " +
                $"WHERE PublicKey = '{user}' AND _ArrayIndex = 0;";


            using (SQLiteDataReader reader = QuerySQL(sql_blockRewards), reader2 = QuerySQL(sql_TransFees)) {

                reader.Read();
                reader2.Read();

                long blockReward = (long)reader[0] * Settings.GetChainSettings(CoinName).BlockReward;

                long TransFees = (long)reader2[0];

                return blockReward + TransFees;
            }
        }

        #endregion

        #region SQL Utils

        public SQLiteDataReader QuerySQL(string sql) {


            Db = new SQLiteConnection($@"Data Source={Settings.StorePath}{CoinName}\chain.db; Version=3;");
            Db.Open();

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

            return reader;
        }

        public int NoQuerySQL(string sql) {

            Db = new SQLiteConnection($@"Data Source={Settings.StorePath}{CoinName}\chain.db; Version=3;");
            Db.Open();
            int num = 0;

            using (SQLiteCommand command = new SQLiteCommand(Db)) {
                command.CommandText = sql;
                num = command.ExecuteNonQuery();
            }

            Db.Close();

            return num;
        }

        private string IsNull(long? num) {

            if (num == null)
                return "NULL";
            return num.ToString();

        }


        #endregion
    }


}
