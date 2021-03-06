﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace IXIComponents.Simple
{
    public class SimpleDataAccessor : IDataAccessor
    {

        private readonly string _coinName;
        private readonly IXISettings _settings;

        private ITangleAccessor _tangleAccessor;

        private SQLiteConnection Db { get; set; }

        public SimpleDataAccessor(CoinName coinName, IXISettings settings, ITangleAccessor tangleAccessor)
        {
            _coinName = coinName.Name;
            _tangleAccessor = tangleAccessor;
            _settings = settings;

            InitDB();
        }

        #region private private private private private private private 

        private Maybe<Block> GetBlock(long height)
        {
            Block block;

            //the normal block!
            string sql = $"SELECT * FROM Block WHERE Height={height}";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                {
                    return Maybe<Block>.None;
                }

                block = reader.ToBlock(_coinName);
            }

            //transactions!
            string sqlTrans = $"SELECT Hash FROM Transactions WHERE BlockID={height}";

            using (SQLiteDataReader reader = QuerySQL(sqlTrans))
            {
                var transList = new List<string>();

                while (reader.Read())
                {
                    transList.Add((string)reader[0]);
                }

                block.Add<Transaction>(transList);
            }

            //smartcontracts
            string sqlSmart = $"SELECT Hash FROM Smartcontracts WHERE BlockID={height}";
            using (SQLiteDataReader reader = QuerySQL(sqlSmart))
            {
                var smartList = new List<string>();

                while (reader.Read())
                {
                    smartList.Add((string)reader[0]);
                }

                block.Add<Smartcontract>(smartList);
            }

            return Maybe<Block>.Some(block);
        }

        private Maybe<Transaction> GetTransaction(string hash, long height)
        {
            //get normal data
            string sql = $"SELECT * FROM Transactions WHERE Hash='{hash}' AND BlockID='{height}'";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                    return Maybe<Transaction>.None;

                var id = (long)reader[0];

                var maybeOutput = GetTransactionOutput(id);
                var maybeTransactionData = GetTransactionData(id);

                var trans = reader.ToTransaction(maybeOutput.Value.Item1, maybeOutput.Value.Item2, maybeTransactionData.Value);
                trans.SendTo = Utils.GetTransactionPoolAddress(height, _coinName, GetChainSettings().Value.TransactionPoolInterval);


                return Maybe<Transaction>.Some(trans);
            }
        }

        private Maybe<Smartcontract> GetSmartcontract(string receivingAddr)
        {
            string sql = $"SELECT * FROM Smartcontracts WHERE ReceivingAddress='{receivingAddr}';";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                {
                    return Maybe<Smartcontract>.None;
                }

                Smartcontract smart = reader.ToSmartcontract();

                //we also need to add the variables:
                //using(SQLiteDataReader reader = QuerySQL($"SELECT * FROM Variables WHERE "))
                smart.Code.Variables = GetVariablesFromDB((long)reader[0]);
                return Maybe<Smartcontract>.Some(smart);
            }
        }

        private void InitDB()
        {

            string path = _settings.DataBasePath;

            //first we create file structure
            if (!Directory.Exists($@"{path}{_coinName}\") || !File.Exists($@"{path}{_coinName}\chain.db"))
            {
                Directory.CreateDirectory($@"{path}{_coinName}\");

                string sql =
                    "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                    "NextAddress CHAR(81) NOT NULL, PublicKey CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL, Difficulty INT NOT NULL);";

                string sql2 =
                    "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Hash CHAR(81), Time LONG, _From CHAR(81), Signature CHAR(81)," +
                    "Mode INT,BlockID INT ,MinerReward INT NOT NULL,FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

                string sql3 =
                    "CREATE TABLE IF NOT EXISTS Data (ID INTEGER PRIMARY KEY AUTOINCREMENT, _ArrayIndex INT NOT NULL, " +
                    "Data CHAR, TransID ,FOREIGN KEY (TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

                string sql4 =
                    "CREATE TABLE IF NOT EXISTS Output (ID INTEGER PRIMARY KEY AUTOINCREMENT, _Values INT NOT NULL,_ArrayIndex INT NOT NULL, " +
                    "Receiver CHAR, TransID ID NOT NULL,FOREIGN KEY(TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

                string sql5 =
                    "CREATE TABLE IF NOT EXISTS Smartcontracts (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name CHAR NOT NULL, Hash CHAR NOT NULL," +
                    " Code CHAR NOT NULL, _FROM CHAR(81) NOT NULL, Signature CHAR NOT NULL, Fee INT NOT NULL" +
                    ", SendTo CHAR(81) NOT NULL,ReceivingAddress CHAR(81) NOT NULL ,BlockID INT, FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

                string sql6 =
                    "CREATE TABLE IF NOT EXISTS Variables (ID INTEGER PRIMARY KEY AUTOINCREMENT,Name CHAR, Value CHAR, SmartID INT,FOREIGN KEY(SmartID) REFERENCES" +
                    " Smartcontracts(ID) ON DELETE CASCADE);";

                NoQuerySQL(sql);
                NoQuerySQL(sql2);
                NoQuerySQL(sql3);
                NoQuerySQL(sql4);
                NoQuerySQL(sql5);
                NoQuerySQL(sql6);

            }
        }

        private Maybe<(List<int>, List<string>)> GetTransactionOutput(long id)
        {

            SQLiteCommand command = new SQLiteCommand(Db);

            var listReceiver = new List<string>();
            var listValue = new List<int>();

            string sql = $"SELECT _Values,Receiver FROM Output WHERE TransID={id};";

            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {

                if (!reader.Read())
                {
                    return Maybe<(List<int>, List<string>)>.None;
                }

                while (true)
                {

                    listValue.Add((int)reader[0]);
                    listReceiver.Add((string)reader[1]);

                    if (!reader.Read())
                        break;
                }

            }

            return Maybe<(List<int> valueList, List<string> receiverList)>.Some((listValue, listReceiver));
        }

        private Maybe<List<string>> GetTransactionData(long id)
        {

            SQLiteCommand command = new SQLiteCommand(Db);

            var list = new List<string>();

            string sql = $"SELECT * FROM Data WHERE TransID={id} ORDER BY _ArrayIndex;";

            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {

                if (!reader.Read())
                    return Maybe<List<string>>.None;

                while (true)
                {

                    list.Add((string)reader[2]);

                    if (!reader.Read())
                        break;
                }
            }

            return Maybe<List<string>>.Some(list);
        }

        private Dictionary<string, ISCType> GetVariablesFromDB(long id)
        {

            string sql = $"SELECT Name,Value FROM Variables WHERE SmartID={id}";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {
                var list = new Dictionary<string, ISCType>();

                while (reader.Read())
                {
                    list.Add((string)reader[0], ((string)reader[1]).ConvertToInternalType());
                }
                return list;
            }
        }

        private void DeleteBlock(long height)
        {
            string sql = $"PRAGMA foreign_keys = ON;DELETE FROM Block WHERE Height={height}";

            NoQuerySQL(sql);
        }

        private void AddTransaction(Transaction trans, long height)
        {

            //data
            long transID = -1;

            string insertPool = "INSERT INTO Transactions (Hash, Time, _FROM, Signature, Mode, BlockID, MinerReward) " +
                                $"SELECT'{trans.Hash}', {trans.Time}, '{trans.From}', '{trans.Signature}', {trans.Mode}, {height}, {trans.ComputeMinerReward()}" +
                                $" WHERE NOT EXISTS (SELECT 1 FROM Transactions WHERE Hash='{trans.Hash}' AND Time={trans.Time}); SELECT last_insert_rowid();";


            using (SQLiteDataReader reader = QuerySQL(insertPool))
            {
                reader.Read();
                transID = (long)reader[0];
            }

            //output & data
            StoreTransactionData(trans, transID);

            //if transaction triggers smartcontract
            if (trans.Mode == 2 && trans.OutputReceiver.Count == 1)
            {

                var maybeSmart = GetSmartcontract(trans.OutputReceiver[0]);

                //if the transaction has a dead end, nothing happens, but money is lost
                if (maybeSmart.HasValue)
                {
                    var smart = maybeSmart.Value;

                    Computer comp = new Computer(smart);

                    //the smartcontract could be buggy or the transaction could be not correctly calling the smartcontract
                    try
                    {
                        var result = comp.Run(trans);

                        var state = comp.GetCompleteState();

                        smart.ApplyState(state);

                        //we need to check if the result is correct and spendable:
                        //we include this handmade transaction in our DB if true
                        var output = result.HasValue ? result.Value.ComputeOutgoingValues() : 0;
                        if (GetBalance(smart.ReceivingAddress) > output)
                        {
                            if (result.HasValue)
                            {
                                AddTransaction(result.Value, height);
                            }

                            UpdateSmartcontract(smart);
                        }
                    }
                    catch (Exception e)
                    {
                        //nothing happens... you just lost money
                        Console.WriteLine("TRANSACTION ERROR (NO PROBLEM MOST OF THE TIME, COULD BE USERERROR\n\n\n" + e);
                    }
                }
            }
        }

        private void StoreTransactionData(Transaction trans, long transID)
        {
            //add data too
            for (int i = 0; i < trans.Data.Count; i++)
            {
                var sql2 = $"INSERT INTO Data (_ArrayIndex, Data, TransID) VALUES({i},'{trans.Data[i]}',{transID});";

                NoQuerySQL(sql2);
            }

            //add receivers + output
            for (int i = 0; i < trans.OutputReceiver.Count; i++)
            {
                string sql2 = $"INSERT INTO Output (_Values, _ArrayIndex, Receiver, TransID) VALUES({trans.OutputValue[i]},{i},'{trans.OutputReceiver[i]}',{transID});";

                NoQuerySQL(sql2);
            }
        }

        private void UpdateSmartcontract(Smartcontract smart)
        {
            //get smart id first
            var maybeID = GetSmartcontractID(smart.ReceivingAddress);
            if (!maybeID.HasValue)
            {
                throw new ArgumentException("Smartcontract with the given receiving address doesnt exist");
            }

            var id = maybeID.Value;

            //update the states:
            var state = smart.Code.Variables;

            foreach (var key in state.Keys)
            {
                string updateVars =
                    $"UPDATE Variables SET Value='{state[key].GetValueAs<string>()}' WHERE  ID={id} AND Name='{key}';";
                NoQuerySQL(updateVars);
            }
        }

        private Maybe<long> GetSmartcontractID(string receivingAddr)
        {
            string query = $"SELECT ID FROM Smartcontracts WHERE ReceivingAddress='{receivingAddr}';";

            using (SQLiteDataReader reader = QuerySQL(query))
            {
                if (!reader.Read())
                    return Maybe<long>.None;

                return Maybe<long>.Some((long)reader[0]);
            }
        }

        private void AddSmartcontract(Smartcontract smart, long height)
        {

            string insertPool = "INSERT INTO Smartcontracts (Name, Hash, Code, _FROM, Signature, Fee, SendTo, ReceivingAddress, BlockID) " +
                $"SELECT'{smart.Name}', '{smart.Hash}', '{smart.Code}', '{smart.From}','{smart.Signature}',{smart.TransactionFee},'{smart.SendTo}','{smart.ReceivingAddress}'," +
                $" {IsNull(height)}" +
                $" WHERE NOT EXISTS (SELECT 1 FROM Smartcontracts WHERE ReceivingAddress='{smart.ReceivingAddress}'); SELECT last_insert_rowid();";

            string sql = $"UPDATE Smartcontracts SET ID={height} WHERE ReceivingAddress='{smart.ReceivingAddress}';";

            int numOfAffected = NoQuerySQL(sql);

            //means we didnt had a smartcontract before
            if (numOfAffected == 0)
            {
                long smartID = -1;

                using (SQLiteDataReader reader = QuerySQL(insertPool))
                {
                    reader.Read();
                    smartID = (long)reader[0];
                }

                var state = smart.Code.Variables;

                foreach (var key in state.Keys)
                {
                    string updateVars = $"INSERT INTO Variables (Name,Value,SmartID) VALUES ('{key}','{state[key].GetValueAs<string>()}',{smartID});";
                    NoQuerySQL(updateVars);
                }
            }
        }

        #endregion

        public Maybe<Block> GetLatestBlock()
        {
            //we first get highest ID
            using (SQLiteDataReader reader = QuerySQL($"SELECT IFNULL(MAX(Height),0) FROM Block"))
            {
                if (!reader.Read())
                    return Maybe<Block>.None;

                return GetBlock((long)reader[0]);
            }
        }

        public List<T> GetFromBlock<T>(Block block) where T : ISignable, IDownloadable
        {

            var hashList = new List<string>();

            if (typeof(T) == typeof(Transaction))
            {
                hashList = block.TransactionHashes;
            }
            if (typeof(T) == typeof(Smartcontract))
            {
                hashList = block.SmartcontractHashes;
            }

            var objList = new List<T>();

            hashList.ForEach(x =>
            {
                var maybeChainSettings = GetChainSettings();
                int interval = maybeChainSettings.HasValue ? maybeChainSettings.Value.TransactionPoolInterval : -1;
                var addr = Utils.GetTransactionPoolAddress(block.Height, _coinName, interval);

                var specificFromAddress = _tangleAccessor.GetSpecificFromAddress<T>(x, addr);

                if (specificFromAddress.HasValue)
                {
                    objList.Add(specificFromAddress.Value);
                }
            });

            return objList;

        }

        public Maybe<T> Get<T>(object info = null, long? height = null) where T : IDownloadable
        {

            if (typeof(T) == typeof(Transaction))
            {
                // ReSharper disable once PossibleInvalidOperationException
                return (Maybe<T>)(object)GetTransaction((string)info, (long)height);
            }

            if (typeof(T) == typeof(Smartcontract))
            {
                return (Maybe<T>)(object)GetSmartcontract((string)info);
            }

            if (typeof(T) == typeof(Block))
            {
                // ReSharper disable once PossibleNullReferenceException
                return (Maybe<T>)(object)GetBlock((long)info);
            }

            throw new NotImplementedException("oops");
        }

        public Maybe<ChainSettings> GetChainSettings()
        {
            string sql = "SELECT Data FROM Block AS b " +
                "JOIN Transactions AS t ON b.Height = t.BlockID " +
                "JOIN Data as d ON t.ID = d.TransID " +
                "WHERE Height = 0 " +
                "ORDER BY _ArrayIndex;";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read()) return Maybe<ChainSettings>.None; //important!!

                ChainSettings settings = reader.ToChainSettings();
                return Maybe<ChainSettings>.Some(settings);
            }
        }

        public void AddBlock(Block block)
        {

            var checkBlock = GetBlock(block.Height);

            //no update when genesis block because of concurrency stuff (hack)
            if (block.Height == 0 && checkBlock.HasValue)
                return;

            //first check if block already exists in db in a different version
            if (checkBlock.HasValue && !checkBlock.Value.Hash.Equals(block.Hash))
            {
                DeleteBlock(block.Height);
            }

            //if block doesnt exists we add
            if (!checkBlock.HasValue)
            {
                string sql = $"INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, PublicKey, SendTo, Difficulty) " +
                    $"VALUES ({block.Height},{block.Nonce},{block.Time},'{block.Hash}','{block.NextAddress}','{block.Owner}','{block.SendTo}', {block.Difficulty.ToString()});";

                NoQuerySQL(sql);

                //add smartcontracts
                if (block.SmartcontractHashes != null && block.SmartcontractHashes.Count > 0) {
                    var smartList = GetFromBlock<Smartcontract>(block);
                    smartList?.ForEach(s => AddSmartcontract(s, block.Height));
                }

                //add transactions!
                if (block.TransactionHashes != null && block.TransactionHashes.Count > 0)
                {
                    var transList = GetFromBlock<Transaction>(block);

                    if (transList.Count > 0)
                        AddSignable(transList, block.Height);
                }
            }
        }

        public void AddSignable<T>(List<T> list, long height) where T : ISignable
        {
            if (typeof(T) == typeof(Smartcontract))
            {
                list.ForEach(x => AddSmartcontract((Smartcontract)(object)x, height));
            }
            else if (typeof(T) == typeof(Transaction))
            {
                list.ForEach(x => AddTransaction((Transaction)(object)x, height));
            }
            else
            {
                throw new NotImplementedException("oops");
            }
        }

        #region SQL Utils

        private SQLiteDataReader QuerySQL(string sql)
        {

            Db = new SQLiteConnection($@"Data Source={_settings.DataBasePath}{_coinName}\chain.db; Version=3;");
            Db.Open();

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

            return reader;
        }

        private int NoQuerySQL(string sql)
        {

            Db = new SQLiteConnection($@"Data Source={_settings.DataBasePath}{_coinName}\chain.db; Version=3;");
            Db.Open();
            
            int num = 0;

            using (SQLiteCommand command = new SQLiteCommand(Db))
            {
                command.CommandText = sql;
                num = command.ExecuteNonQuery();
            }

            Db.Close();

            return num;
        }

        private string IsNull(long? num)
        {

            if (num == null)
                return "NULL";
            return num.ToString();

        }

        #endregion

        #region BalanceStuff

        public long GetBalance(string user)
        {

            //count all incoming money
            //get all block rewards & transfees
            var blockReward = GetBlockReward(user);

            //get all outputs who point towards you
            var outputSum = GetIncomingOutputs(user);

            //count now all removing money
            //remove all outputs which are outgoing
            var outgoingOutputs = GetOutgoingOutputs(user);

            //remove all transaction fees (transactions)
            var outgoingTransfees = GetOutcomingTransFees(user);

            //remove all smartcontract fees
            var outgoingSmartfees = GetOutcomingSmartFees(user);

            return blockReward + outputSum + outgoingOutputs + outgoingTransfees + outgoingSmartfees;
        }

        private long GetOutcomingSmartFees(string user)
        {
            string transFees = $"SELECT IFNULL(SUM(Fee),0) From Smartcontracts WHERE _FROM='{user}'";

            using (SQLiteDataReader reader = QuerySQL(transFees))
            {
                reader.Read();
                return (long)reader[0] * -1;
            }
        }

        private long GetOutcomingTransFees(string user)
        {
            string transFees = $"SELECT IFNULL(SUM(Data),0) FROM Transactions JOIN Data ON Transactions.ID = Data.TransID WHERE _From='{user}' AND _ArrayIndex = 0";

            using (SQLiteDataReader reader = QuerySQL(transFees))
            {
                reader.Read();
                return (long)reader[0] * -1;
            }
        }

        private long GetOutgoingOutputs(string user)
        {
            string sqlOutgoing = $"SELECT IFNULL(SUM(_Values),0) FROM Transactions JOIN Output ON Transactions.ID = Output.TransID WHERE _From='{user}' AND NOT Mode = -1;";

            using (SQLiteDataReader reader = QuerySQL(sqlOutgoing))
            {
                reader.Read();
                return (long)reader[0] * -1;
            }
        }

        private long GetIncomingOutputs(string user)
        {
            string sql_Outputs = $"SELECT IFNULL(SUM(_Values),0) FROM Output WHERE Receiver='{user}';";

            using (SQLiteDataReader reader = QuerySQL(sql_Outputs))
            {
                reader.Read();
                return (long)reader[0];
            }
        }

        private long GetBlockReward(string user)
        {

            string sqlBlockRewards = $"SELECT IFNULL(COUNT(PublicKey),0) FROM Block WHERE PublicKey='{user}'";

            string sqlTransFees = "SELECT IFNULL(SUM(Data),0) FROM Block AS b JOIN Transactions AS t ON " +
                "b.Height = t.BlockID JOIN Data as d ON t.ID = d.TransID " +
                $"WHERE PublicKey = '{user}' AND _ArrayIndex = 0;";

            using (SQLiteDataReader reader = QuerySQL(sqlBlockRewards), reader2 = QuerySQL(sqlTransFees))
            {
    
                reader.Read();
                reader2.Read();

                var blockReward = (long)reader[0] * GetChainSettings().Value.BlockReward;

                var transFees = (long)reader2[0];

                return blockReward + transFees;
            }
        }

        #endregion

    }
}
