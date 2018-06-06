using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TangleChain.Classes;

namespace TangleChain {

    public class DataBase {

        private SQLiteConnection Db { get; set; }
        public string CoinName { get; set; }

        public DataBase(string name) {

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

            string sql = string.Format("INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, Owner, SendTo) " +
                                       "VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}');",
            block.Height, block.Nonce, block.Time, block.Hash, block.NextAddress, block.Owner, block.SendTo);

            NoQuerySQL(sql);

            if (storeTransactions) {
                var transList = Core.GetAllTransactionsFromBlock(block);
                AddTransaction(transList, block.Height);
            }

            return flag;

        }

        public void DeleteBlock(int height) {

            //delete block
            string sql = string.Format("DELETE FROM Block WHERE Height={0}", height);

            NoQuerySQL(sql);
        }

        public Block GetBlock(int height) {

            string sql = string.Format("SELECT * FROM Block WHERE Height={0}", height);
            SQLiteDataReader reader = QuerySQL(sql);

            if (!reader.Read())
                return null;

            Block block = new Block(reader, CoinName);

            return block;

        }


        public Transaction GetTransaction(string hash, int height) {

            //get normal data
            string sql = string.Format("SELECT * FROM Transactions WHERE Hash='{0}' AND BlockID='{1}'", hash, height);

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

        public void AddTransaction(List<Transaction> list, int BlockID) {

            list.ForEach(t => AddTransaction(t, BlockID));
        }

        public void AddTransaction(Transaction trans, int blockID) {

            //raw transaction
            string sql = string.Format("INSERT INTO Transactions (Hash, Time, _From, Signature, Mode, BlockID) " +
                                       "VALUES ('{0}',{1},'{2}','{3}',{4},{5}); SELECT last_insert_rowid();",
                trans.Hash, trans.Time, trans.From, trans.Signature, trans.Mode, blockID);

            SQLiteDataReader reader = QuerySQL(sql);
            reader.Read();

            long TransID = (long)reader[0];

            //add data too
            for (int i = 0; i < trans.Data.Count; i++) {

                string sql2 = string.Format("INSERT INTO Data (_ArrayIndex, Data, TransID) VALUES({0},'{1}',{2});", i, trans.Data[i], TransID);

                NoQuerySQL(sql2);
            }

            //add receivers + output
            for (int i = 0; i < trans.OutputReceiver.Count; i++) {

                string sql2 = string.Format("INSERT INTO Output (_Values, _ArrayIndex, Receiver, TransID) VALUES({0},{1},'{2}',{3});", trans.OutputValue[i], i, trans.OutputReceiver[i], TransID);

                NoQuerySQL(sql2);
            }
        }

        private List<string> GetTransactionData(long id) {

            //i keep the structure here because data could be zero and i need to correctly setup everything

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

        private (List<int>, List<string>) GetTransactionOutput(long id) {
            //i keep the structure here because data could be zero and i need to correctly setup everything

            SQLiteCommand command = new SQLiteCommand(Db);

            var listReceiver = new List<string>();
            var listValue = new List<int>();

            string sql = string.Format("SELECT * FROM Output WHERE TransID={0};", id);

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
            string transFees = string.Format("SELECT IFNULL(SUM(Data),0) FROM Transactions JOIN Data ON Transactions.ID = Data.TransID WHERE _From='{0}'", user);
            SQLiteDataReader reader = QuerySQL(transFees);
            reader.Read();

            return (long) reader[0] * -1;
        }

        private long GetOutgoingOutputs(string user) {
            string sql_Outgoing =
                string.Format(
                    "SELECT IFNULL(SUM(_Values),0) FROM Transactions JOIN Output ON Transactions.ID = Output.TransID WHERE _From='{0}';",
                    user);
            SQLiteDataReader reader = QuerySQL(sql_Outgoing);
            reader.Read();

            return (long) reader[0] * -1;

        }

        private long GetIncomingOutputs(string user) {
            string sql_Outputs = string.Format("SELECT IFNULL(SUM(_Values),0) FROM Output WHERE Receiver='{0}';", user);
            SQLiteDataReader reader = QuerySQL(sql_Outputs);
            reader.Read();

            return (long) reader[0];
        }

        private long GetBlockReward(string user) {

            SQLiteDataReader reader;

            //Every block gives a reward
            string sql_blockRewards = string.Format("SELECT IFNULL(COUNT(Owner),0) FROM Block WHERE Owner='{0}'", user);
            reader = QuerySQL(sql_blockRewards);
            reader.Read();

            long blockReward = (long) reader[0];

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

        private SQLiteDataReader QuerySQL(string sql) {
            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }

        private void NoQuerySQL(string sql) {
            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;
            command.ExecuteNonQuery();

            command.Dispose();
        }

        #endregion
    }


}
