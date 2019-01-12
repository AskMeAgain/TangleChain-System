using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.NewClasses
{
    public class SimpleDataAccessor : IDataAccessor
    {

        private readonly string _coinName;

        private ChainSettings cSett;
        private ChainSettings _chainSetting {
            get => cSett ?? GetChainSettings();
            set => cSett = value;
        }

        private ITangleAccessor _tangleAccessor;

        private SQLiteConnection Db { get; set; }

        public SimpleDataAccessor(CoinName coinName, ITangleAccessor tangleAccessor) {
            _coinName = coinName.Name;
            _tangleAccessor = tangleAccessor;

            InitDB();
        }

        private void InitDB()
        {

            string path = IXISettings.DataBasePath;

            //first we create file structure
            if (!Directory.Exists($@"{path}{_coinName}\") || !File.Exists($@"{path}{_coinName}\chain.db"))
            {
                Directory.CreateDirectory($@"{path}{_coinName}\");

                string sql =
                    "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
                    "NextAddress CHAR(81) NOT NULL, PublicKey CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL, Difficulty INT NOT NULL);";

                string sql2 =
                    "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Hash CHAR(81), Time LONG, _From CHAR(81), Signature CHAR(81)," +
                    "Mode INT,BlockID INT ,MinerReward INT NOT NULL,PoolHeight INT, FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

                string sql3 =
                    "CREATE TABLE IF NOT EXISTS Data (ID INTEGER PRIMARY KEY AUTOINCREMENT, _ArrayIndex INT NOT NULL, " +
                    "Data CHAR, TransID ,FOREIGN KEY (TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

                string sql4 =
                    "CREATE TABLE IF NOT EXISTS Output (ID INTEGER PRIMARY KEY AUTOINCREMENT, _Values INT NOT NULL,_ArrayIndex INT NOT NULL, " +
                    "Receiver CHAR, TransID ID NOT NULL,FOREIGN KEY(TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

                string sql5 =
                    "CREATE TABLE IF NOT EXISTS Smartcontracts (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name CHAR NOT NULL, Hash CHAR NOT NULL," +
                    " Balance INT NOT NULL, Code CHAR NOT NULL, _FROM CHAR(81) NOT NULL, Signature CHAR NOT NULL, Fee INT NOT NULL" +
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

        public Block GetBlock(long height)
        {
            Block block = null;

            //the normal block!
            string sql = $"SELECT * FROM Block WHERE Height={height}";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                {
                    return null;
                }

                block = new Block(reader, _coinName);
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

            return block;
        }

        private (List<int>, List<string>) GetTransactionOutput(long id)
        {
            //i keep the structure here because data could be zero and i need to correctly setup everything

            SQLiteCommand command = new SQLiteCommand(Db);

            var listReceiver = new List<string>();
            var listValue = new List<int>();

            string sql = $"SELECT _Values,Receiver FROM Output WHERE TransID={id};";

            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {

                if (!reader.Read())
                {
                    return (null, null);
                }

                while (true)
                {

                    listValue.Add((int)reader[0]);
                    listReceiver.Add((string)reader[1]);

                    if (!reader.Read())
                        break;
                }

            }

            return (listValue, listReceiver);
        }

        public Transaction GetTransaction(string hash, long height)
        {
            //get normal data
            string sql = $"SELECT * FROM Transactions WHERE Hash='{hash}' AND BlockID='{height}'";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                    return null;

                long ID = (long)reader[0];
                var output = GetTransactionOutput(ID);

                Transaction trans = new Transaction(reader, output.Item1, output.Item2, GetTransactionData(ID))
                {
                    SendTo = Utils.GetTransactionPoolAddress(height, _chainSetting.TransactionPoolInterval, _coinName)
                };

                return trans;
            }
        }

        private List<string> GetTransactionData(long id)
        {
            //i keep the structure here because data could be zero and i need to correctly setup everything

            SQLiteCommand command = new SQLiteCommand(Db);

            var list = new List<string>();

            string sql = $"SELECT * FROM Data WHERE TransID={id} ORDER BY _ArrayIndex;";

            command.CommandText = sql;

            using (SQLiteDataReader reader = command.ExecuteReader())
            {

                if (!reader.Read())
                    return null;

                while (true)
                {

                    list.Add((string)reader[2]);

                    if (!reader.Read())
                        break;
                }
            }

            return list;
        }

        public Smartcontract GetSmartcontract(string receivingAddr)
        {
            string sql = $"SELECT * FROM Smartcontracts WHERE ReceivingAddress='{receivingAddr}';";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                {
                    return null;
                }

                Smartcontract smart = new Smartcontract(reader);

                //we also need to add the variables:
                //using(SQLiteDataReader reader = QuerySQL($"SELECT * FROM Variables WHERE "))
                smart.Code.Variables = GetVariablesFromDB((long)reader[0]);
                return smart;
            }
        }

        private Dictionary<string, ISCType> GetVariablesFromDB(long ID)
        {

            string sql = $"SELECT Name,Value FROM Variables WHERE SmartID={ID}";

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

            //delete block
            string sql = $"PRAGMA foreign_keys = ON;DELETE FROM Block WHERE Height={height}";

            NoQuerySQL(sql);
        }

        private List<Smartcontract> GetAllSmartcontractsFromBlock(Block block)
        {

            var hashList = block.SmartcontractHashes;
            var smartList = new List<Smartcontract>();

            hashList.ForEach(x =>
            {
                var smart = _tangleAccessor.GetSmartcontract(x,
                    Utils.GetTransactionPoolAddress(block.Height, _chainSetting.TransactionPoolInterval, _coinName));
                smartList.Add(smart);
            });

            return smartList;

        }

        private List<Transaction> GetTransactionsFromBlock(Block block)
        {
            var hashList = block.TransactionHashes;
            var transList = new List<Transaction>();

            hashList.ForEach(x =>
            {
                var trans = _tangleAccessor.GetTransaction(x, Utils.GetTransactionPoolAddress(block.Height, _chainSetting.TransactionPoolInterval, _coinName));
                transList.Add(trans);
            });

            return transList;
        }

        public void AddBlock(Block block)
        {

            //no update when genesis block because of concurrency stuff (hack)
            if (block.Height == 0 && GetBlock(block.Height) != null)
                return;


            //first check if block already exists in db in a different version
            Block checkBlock = GetBlock(block.Height);
            if (checkBlock != null && !checkBlock.Hash.Equals(block.Hash))
            {
                DeleteBlock(block.Height);
            }

            //if block doesnt exists we add
            if (GetBlock(block.Height) == null)
            {
                string sql = $"INSERT INTO Block (Height, Nonce, Time, Hash, NextAddress, PublicKey, SendTo, Difficulty) " +
                             $"VALUES ({block.Height},{block.Nonce},{block.Time},'{block.Hash}','{block.NextAddress}','{block.Owner}','{block.SendTo}', {block.Difficulty.ToString()});";

                NoQuerySQL(sql);

                //add smartcontracts
                if (block.SmartcontractHashes != null && block.SmartcontractHashes.Count > 0)
                {
                    var smartList = GetAllSmartcontractsFromBlock(block);
                    smartList?.ForEach(s => AddSmartcontract(s, block.Height));
                }

                //add transactions!
                if (block.TransactionHashes != null && block.TransactionHashes.Count > 0)
                {
                    var transList = GetTransactionsFromBlock(block);

                    if (transList != null)
                        AddTransaction(transList, block.Height);

                    if (block.Height == 0)
                    {
                        //we set settings too
                        _chainSetting = GetChainSettings();
                    }
                }
            }
        }

        public void AddTransaction(List<Transaction> trans, long height)
        {
            trans.ForEach(x => AddTransaction(x, height));
        }

        private void AddTransaction(Transaction trans, long height)
        {

            //data
            long TransID = -1;

            string insertPool = "INSERT INTO Transactions (Hash, Time, _FROM, Signature, Mode, BlockID, MinerReward, PoolHeight) " +
                                $"SELECT'{trans.Hash}', {trans.Time}, '{trans.From}', '{trans.Signature}', {trans.Mode}, {height}, {trans.ComputeMinerReward()}, {height}" +
                                $" WHERE NOT EXISTS (SELECT 1 FROM Transactions WHERE Hash='{trans.Hash}' AND Time={trans.Time}); SELECT last_insert_rowid();";

            //if normal trans is already there because of it was included in transpool, we need to update it first.
            string sql = $"UPDATE Transactions SET BlockID={height}, PoolHeight=NULL WHERE Hash='{trans.Hash}' AND Time={trans.Time} AND PoolHeight IS NOT NULL;";

            int numOfAffected = NoQuerySQL(sql);

            //means we didnt update anything and just added it straight to db. So we have to add it normally
            if (numOfAffected == 0)
            {
                using (SQLiteDataReader reader = QuerySQL(insertPool))
                {
                    reader.Read();
                    TransID = (long)reader[0];
                }

                //output & data
                StoreTransactionData(trans, TransID);

            }

            //if transaction triggers smartcontract
            if (trans.Mode == 2 && trans.OutputReceiver.Count == 1)
            {

                Smartcontract smart = GetSmartcontract(trans.OutputReceiver[0]);

                //if the transaction has a dead end, nothing happens, but money is lost
                if (smart != null)
                {

                    Computer comp = new Computer(smart);

                    //the smartcontract could be buggy or the transaction could be not correctly calling the smartcontract
                    try
                    {
                        var result = comp.Run(trans);
                        smart.Code.Variables = comp.GetCompleteState();

                        //we need to check if the result is correct and spendable:
                        //we include this handmade transaction in our DB if true
                        if (GetBalance(smart.ReceivingAddress) > result.ComputeOutgoingValues())
                        {
                            AddTransaction(result, height);
                            UpdateSmartcontract(smart);
                        }
                    }
                    catch (Exception)
                    {
                        //nothing happens... you just lost money
                    }
                }
            }
        }

        private void StoreTransactionData(Transaction trans, long TransID)
        {
            //add data too
            for (int i = 0; i < trans.Data.Count; i++)
            {
                string sql2 = $"INSERT INTO Data (_ArrayIndex, Data, TransID) VALUES({i},'{trans.Data[i]}',{TransID});";

                NoQuerySQL(sql2);
            }

            //add receivers + output
            for (int i = 0; i < trans.OutputReceiver.Count; i++)
            {
                string sql2 = $"INSERT INTO Output (_Values, _ArrayIndex, Receiver, TransID) VALUES({trans.OutputValue[i]},{i},'{trans.OutputReceiver[i]}',{TransID});";

                NoQuerySQL(sql2);
            }
        }

        private void UpdateSmartcontract(Smartcontract smart)
        {
            //get smart id first
            long id = GetSmartcontractID(smart.ReceivingAddress) ?? throw new ArgumentException("Smartcontract with the given receiving address doesnt exist");

            //update the balance
            long balance = GetBalance(smart.ReceivingAddress);

            string updateBalance = $"UPDATE Smartcontracts SET Balance={balance} WHERE ID={id};";
            NoQuerySQL(updateBalance);

            //update the states:
            var state = smart.Code.Variables;
            foreach (var key in state.Keys)
            {
                string updateVars =
                    $"UPDATE Variables SET Value='{state[key].GetValueAs<string>()}' WHERE  ID={id} AND Name='{key}';";
                NoQuerySQL(updateVars);
            }
        }

        private long? GetSmartcontractID(string receivingAddr)
        {
            string query = $"SELECT ID FROM Smartcontracts WHERE ReceivingAddress='{receivingAddr}';";

            using (SQLiteDataReader reader = QuerySQL(query))
            {
                if (!reader.Read())
                    return null;

                return (long)reader[0];
            }
        }

        private void AddSmartcontract(Smartcontract smart, long height)
        {
            long SmartID = -1;

            string insertPool = "INSERT INTO Smartcontracts (Name, Hash, Balance, Code, _FROM, Signature, Fee, SendTo, ReceivingAddress, PoolHeight, BlockID) " +
                $"SELECT'{smart.Name}', '{smart.Hash}', {smart.Balance}, '{smart.Code}', '{smart.From}','{smart.Signature}',{smart.TransactionFee},'{smart.SendTo}','{smart.ReceivingAddress}'," +
                $" {IsNull(height)}" +
                $" WHERE NOT EXISTS (SELECT 1 FROM Smartcontracts WHERE ReceivingAddress='{smart.ReceivingAddress}'); SELECT last_insert_rowid();"; ;

            //if normal smartcontract is already there because of it was included in transpool, we need to update it first.
            string sql = $"UPDATE Smartcontracts SET ID={height}, PoolHeight=NULL WHERE ReceivingAddress='{smart.ReceivingAddress}' AND PoolHeight IS NOT NULL;";

            int numOfAffected = NoQuerySQL(sql);

            //means we didnt had a smartcontract before
            if (numOfAffected == 0)
            {
                using (SQLiteDataReader reader = QuerySQL(insertPool))
                {
                    reader.Read();
                    SmartID = (long)reader[0];
                }

                StoreSmartcontractData(smart, SmartID);
            }
        }

        private void StoreSmartcontractData(Smartcontract smart, long SmartID)
        {
            var state = smart.Code.Variables;

            foreach (var key in state.Keys)
            {
                string updateVars = $"INSERT INTO Variables (Name,Value,SmartID) VALUES ('{key}','{state[key].GetValueAs<string>()}',{SmartID});";
                NoQuerySQL(updateVars);
            }
        }

        public void AddSmartcontract(List<Smartcontract> smart, long height)
        {
            smart.ForEach(x => AddSmartcontract(x, height));
        }

        public ChainSettings GetChainSettings()
        {
            string sql = "SELECT Data FROM Block AS b " +
                "JOIN Transactions AS t ON b.Height = t.BlockID " +
                "JOIN Data as d ON t.ID = d.TransID " +
                "WHERE Height = 0 " +
                "ORDER BY _ArrayIndex;";

            using (SQLiteDataReader reader = QuerySQL(sql))
            {

                if (!reader.Read())
                    return null;

                ChainSettings settings = new ChainSettings(reader);
                return settings;
            }
        }

        #region SQL Utils

        public SQLiteDataReader QuerySQL(string sql)
        {

            Db = new SQLiteConnection($@"Data Source={IXISettings.DataBasePath}{_coinName}\chain.db; Version=3;");
            Db.Open();

            SQLiteCommand command = new SQLiteCommand(Db);
            command.CommandText = sql;

            SQLiteDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);

            return reader;
        }

        public int NoQuerySQL(string sql)
        {

            Db = new SQLiteConnection($@"Data Source={IXISettings.DataBasePath}{_coinName}\chain.db; Version=3;");
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

        public string IsNull(long? num)
        {

            if (num == null)
                return "NULL";
            return num.ToString();

        }

        #region BalanceStuff


        public long GetBalance(string user)
        {

            //count all incoming money
            //get all block rewards & transfees
            var blockReward = GetBlockReward(user);

            //get all outputs who point towards you
            var OutputSum = GetIncomingOutputs(user);

            //count now all removing money
            //remove all outputs which are outgoing
            var OutgoingOutputs = GetOutgoingOutputs(user);

            //remove all transaction fees (transactions)
            var OutgoingTransfees = GetOutcomingTransFees(user);

            //remove all smartcontract fees
            var OutgoingSmartfees = GetOutcomingSmartFees(user);

            return blockReward + OutputSum + OutgoingOutputs + OutgoingTransfees + OutgoingSmartfees;
        }

        public Block GetLatestBlock()
        {
            //we first get highest ID
            using (SQLiteDataReader reader = QuerySQL($"SELECT IFNULL(MAX(Height),0) FROM Block"))
            {

                if (!reader.Read())
                    return null;

                return GetBlock((long)reader[0]);
            }
        }

        public List<Transaction> GetTransactionFromBlock(Block block)
        {

            var list = block.TransactionHashes;

            var transList = new List<Transaction>();

            list.ForEach(x =>
            {
                var addr = Utils.GetTransactionPoolAddress(block.Height, _chainSetting.TransactionPoolInterval, _coinName);
                var trans = _tangleAccessor.GetTransaction(x, addr);
                transList.Add(trans);
            });

            return transList;
        }

        public List<Smartcontract> GetSmartcontractsFromBlock(Block block)
        {

            var list = block.SmartcontractHashes;

            var smartList = new List<Smartcontract>();

            list.ForEach(x =>
            {
                var addr = Utils.GetTransactionPoolAddress(block.Height, _chainSetting.TransactionPoolInterval, _coinName);
                var smart = _tangleAccessor.GetSmartcontract(x, addr);
                smartList.Add(smart);

            });

            return smartList;
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
            string sql_Outgoing = $"SELECT IFNULL(SUM(_Values),0) FROM Transactions JOIN Output ON Transactions.ID = Output.TransID WHERE _From='{user}' AND NOT Mode = -1;";

            using (SQLiteDataReader reader = QuerySQL(sql_Outgoing))
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

            string sql_blockRewards = $"SELECT IFNULL(COUNT(PublicKey),0) FROM Block WHERE PublicKey='{user}'";

            string sql_TransFees = "SELECT IFNULL(SUM(Data),0) FROM Block AS b JOIN Transactions AS t ON " +
                "b.Height = t.BlockID JOIN Data as d ON t.ID = d.TransID " +
                $"WHERE PublicKey = '{user}' AND _ArrayIndex = 0;";

            using (SQLiteDataReader reader = QuerySQL(sql_blockRewards), reader2 = QuerySQL(sql_TransFees))
            {

                reader.Read();
                reader2.Read();

                long blockReward = (long)reader[0] * _chainSetting.BlockReward;

                long TransFees = (long)reader2[0];

                return blockReward + TransFees;
            }
        }

        #endregion

        #endregion
    }
}
