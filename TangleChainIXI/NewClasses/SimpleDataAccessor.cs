using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI.NewClasses
{
    public class SimpleDataAccessor : IDataAccessor
    {

        private readonly string _coinName;
        private ChainSettings _chainSetting;

        private IBalance _balanceComputer;
        private ITangleAccessor _tangleAccessor;

        private SQLiteConnection Db { get; set; }

        public SimpleDataAccessor(string coinName, IBalance balance, ITangleAccessor tangleAccessor)
        {
            _coinName = coinName;
            _balanceComputer = balance;
            _tangleAccessor = tangleAccessor;
        }

        public Block GetBlockFromWeb(string hash, string address)
        {
            return _tangleAccessor.get
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

        public Transaction GetTransaction(string hash, string height)
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
                    SendTo = Utils.GetTransactionPoolAddress(height, _coinName)
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
            throw new NotImplementedException();
        }


        private Transaction GetTransactionByHash(string hash)
        {
            throw new NotImplementedException();

        }

        private Smartcontract GetSmartcontractByHash(string hash)
        {
            throw new NotImplementedException();
        }

        public List<Block> GetBlocks(string address)
        {
            throw new NotImplementedException();
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

            hashList.ForEach(x => smartList.Add(GetSmartcontractByHash(x)));

            return smartList;

        }

        private List<Transaction> GetTransactionsFromBlock(Block block)
        {

            var hashList = block.TransactionHashes;
            var transList = new List<Transaction>();

            hashList.ForEach(x => transList.Add(GetTransactionByHash(x)));

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
                        if (_balanceComputer.GetBalance(smart.ReceivingAddress) > result.ComputeOutgoingValues())
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
            long balance = _balanceComputer.GetBalance(smart.ReceivingAddress);

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
            throw new NotImplementedException();
        }

        public void AddSmartcontract(List<Smartcontract> smart, long height)
        {
            smart.ForEach(x => AddSmartcontract(x, height));
        }

        public void SetChainSettings(ChainSettings settings)
        {
            throw new NotImplementedException();
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

        private SQLiteDataReader QuerySQL(string sql)
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

        private string IsNull(long? num)
        {

            if (num == null)
                return "NULL";
            return num.ToString();

        }

        #endregion
    }
}
