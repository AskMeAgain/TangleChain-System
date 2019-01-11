using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Classes
{

    //public class DataBase
    //{

    //    public string CoinName { get; set; }
    //    public bool ExistedBefore { get; set; }

    //    private ChainSettings cSett;
    //    public ChainSettings ChainSettings {
    //        get => cSett ?? GetChainSettingsFromDB();
    //        set => cSett = value;
    //    }

    //    internal DataBase(string name)
    //    {

    //        //if the db exists we set this flag
    //        ExistedBefore = Exists(name) ? true : false;

    //        CoinName = name;
    //        string path = IXISettings.DataBasePath;

    //        //first we create file structure
    //        if (!Directory.Exists($@"{path}{name}\") || !File.Exists($@"{path}{name}\chain.db"))
    //        {
    //            Directory.CreateDirectory($@"{path}{name}\");

    //            string sql =
    //                "CREATE TABLE IF NOT EXISTS Block (Height INT PRIMARY KEY, Nonce INT NOT NULL, Time LONG NOT NULL, Hash CHAR(20) NOT NULL, " +
    //                "NextAddress CHAR(81) NOT NULL, PublicKey CHAR(81) NOT NULL, SendTo CHAR(81) NOT NULL, Difficulty INT NOT NULL);";

    //            string sql2 =
    //                "CREATE TABLE IF NOT EXISTS Transactions (ID INTEGER PRIMARY KEY AUTOINCREMENT, Hash CHAR(81), Time LONG, _From CHAR(81), Signature CHAR(81)," +
    //                "Mode INT,BlockID INT ,MinerReward INT NOT NULL,PoolHeight INT, FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

    //            string sql3 =
    //                "CREATE TABLE IF NOT EXISTS Data (ID INTEGER PRIMARY KEY AUTOINCREMENT, _ArrayIndex INT NOT NULL, " +
    //                "Data CHAR, TransID ,FOREIGN KEY (TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

    //            string sql4 =
    //                "CREATE TABLE IF NOT EXISTS Output (ID INTEGER PRIMARY KEY AUTOINCREMENT, _Values INT NOT NULL,_ArrayIndex INT NOT NULL, " +
    //                "Receiver CHAR, TransID ID NOT NULL,FOREIGN KEY(TransID) REFERENCES Transactions(ID) ON DELETE CASCADE);";

    //            string sql5 =
    //                "CREATE TABLE IF NOT EXISTS Smartcontracts (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name CHAR NOT NULL, Hash CHAR NOT NULL," +
    //                " Balance INT NOT NULL, Code CHAR NOT NULL, _FROM CHAR(81) NOT NULL, Signature CHAR NOT NULL, Fee INT NOT NULL" +
    //                ", SendTo CHAR(81) NOT NULL,ReceivingAddress CHAR(81) NOT NULL ,BlockID INT ,PoolHeight INT ,FOREIGN KEY(BlockID) REFERENCES Block(Height) ON DELETE CASCADE);";

    //            string sql6 =
    //                "CREATE TABLE IF NOT EXISTS Variables (ID INTEGER PRIMARY KEY AUTOINCREMENT,Name CHAR, Value CHAR, SmartID INT,FOREIGN KEY(SmartID) REFERENCES" +
    //                " Smartcontracts(ID) ON DELETE CASCADE);";

    //            NoQuerySQL(sql);
    //            NoQuerySQL(sql2);
    //            NoQuerySQL(sql3);
    //            NoQuerySQL(sql4);
    //            NoQuerySQL(sql5);
    //            NoQuerySQL(sql6);

    //        }

    //    }

    //    public static bool Exists(string name)
    //    {
    //        return File.Exists($@"{IXISettings.DataBasePath}{name}\chain.db");
    //    }

    //    #region Helper Helper Helper Helper Helper Helper Helper Helper Helper Helper Helper Helper 

    //    private void AddSmartcontract(Smartcontract smart, long? blockID, long? poolHeight)
    //    {
    //        long SmartID = -1;

    //        string insertPool = "INSERT INTO Smartcontracts (Name, Hash, Balance, Code, _FROM, Signature, Fee, SendTo, ReceivingAddress, PoolHeight, BlockID) " +
    //                            $"SELECT'{smart.Name}', '{smart.Hash}', {smart.Balance}, '{smart.Code}', '{smart.From}','{smart.Signature}',{smart.TransactionFee},'{smart.SendTo}','{smart.ReceivingAddress}'," +
    //                            $" {IsNull(poolHeight)},{IsNull(blockID)}" +
    //                            $" WHERE NOT EXISTS (SELECT 1 FROM Smartcontracts WHERE ReceivingAddress='{smart.ReceivingAddress}'); SELECT last_insert_rowid();"; ;

    //        //Case 1: insert a transpool smartcontract
    //        if (poolHeight != null)
    //        {

    //            using (SQLiteDataReader reader = QuerySQL(insertPool))
    //            {
    //                reader.Read();
    //                SmartID = (long)reader[0];
    //            }

    //            //ADD DATA OF SMARTCONTRACT
    //            StoreSmartcontractData(smart, SmartID);

    //        }

    //        //Case 2: insert a smart from block:
    //        if (blockID != null)
    //        {

    //            //if normal smartcontract is already there because of it was included in transpool, we need to update it first.
    //            string sql = $"UPDATE Smartcontracts SET ID={blockID}, PoolHeight=NULL WHERE ReceivingAddress='{smart.ReceivingAddress}' AND PoolHeight IS NOT NULL;";

    //            int numOfAffected = NoQuerySQL(sql);

    //            //means we didnt had a smartcontract before
    //            if (numOfAffected == 0)
    //            {
    //                using (SQLiteDataReader reader = QuerySQL(insertPool))
    //                {
    //                    reader.Read();
    //                    SmartID = (long)reader[0];
    //                }

    //                StoreSmartcontractData(smart, SmartID);
    //            }
    //        }
    //    }

    //    private void AddTransaction(Transaction trans, long? blockID, long? poolHeight)
    //    {
    //        //data
    //        long TransID = -1;

    //        string insertPool = "INSERT INTO Transactions (Hash, Time, _FROM, Signature, Mode, BlockID, MinerReward, PoolHeight) " +
    //                            $"SELECT'{trans.Hash}', {trans.Time}, '{trans.From}', '{trans.Signature}', {trans.Mode}, {IsNull(blockID)}, {trans.ComputeMinerReward()}, {IsNull(poolHeight)}" +
    //                            $" WHERE NOT EXISTS (SELECT 1 FROM Transactions WHERE Hash='{trans.Hash}' AND Time={trans.Time}); SELECT last_insert_rowid();";

    //        //Case 1: insert a transpool transaction
    //        if (poolHeight != null)
    //        {

    //            using (SQLiteDataReader reader = QuerySQL(insertPool))
    //            {
    //                reader.Read();
    //                TransID = (long)reader[0];
    //            }

    //            StoreTransactionData(trans, TransID);

    //        }

    //        //Case 2: insert a normal trans from block:
    //        if (blockID != null)
    //        {

    //            //if normal trans is already there because of it was included in transpool, we need to update it first.
    //            string sql = $"UPDATE Transactions SET BlockID={blockID}, PoolHeight=NULL WHERE Hash='{trans.Hash}' AND Time={trans.Time} AND PoolHeight IS NOT NULL;";

    //            int numOfAffected = NoQuerySQL(sql);

    //            //means we didnt update anything and just added it straight to db. So we have to add it normally
    //            if (numOfAffected == 0)
    //            {
    //                using (SQLiteDataReader reader = QuerySQL(insertPool))
    //                {
    //                    reader.Read();
    //                    TransID = (long)reader[0];
    //                }

    //                //output & data
    //                StoreTransactionData(trans, TransID);

    //            }

    //            //if transaction triggers smartcontract
    //            if (trans.Mode == 2 && trans.OutputReceiver.Count == 1)
    //            {

    //                Smartcontract smart = GetSmartcontract(trans.OutputReceiver[0]);

    //                //if the transaction has a dead end, nothing happens, but money is lost
    //                if (smart != null)
    //                {

    //                    Computer comp = new Computer(smart);

    //                    //the smartcontract could be buggy or the transaction could be not correctly calling the smartcontract
    //                    try
    //                    {
    //                        var result = comp.Run(trans);
    //                        smart.Code.Variables = comp.GetCompleteState();

    //                        //we need to check if the result is correct and spendable:
    //                        //we include this handmade transaction in our DB if true
    //                        if (GetBalance(smart.ReceivingAddress) > result.ComputeOutgoingValues())
    //                        {
    //                            AddTransaction(result, blockID, null);
    //                            UpdateSmartcontract(smart);
    //                        }

    //                    }
    //                    catch (Exception)
    //                    {
    //                        //nothing happens... you just lost money
    //                    }
    //                }
    //            }
    //        }
    //    }



    //    private void StoreSmartcontractData(Smartcontract smart, long SmartID)
    //    {
    //        var state = smart.Code.Variables;

    //        foreach (var key in state.Keys)
    //        {
    //            string updateVars = $"INSERT INTO Variables (Name,Value,SmartID) VALUES ('{key}','{state[key].GetValueAs<string>()}',{SmartID});";
    //            NoQuerySQL(updateVars);
    //        }
    //    }







    //    private List<Smartcontract> GetSmartcontractsFromTransPool(long poolHeight, int num)
    //    {
    //        //get normal data
    //        string sql = $"SELECT ReceivingAddress FROM Smartcontracts WHERE PoolHeight={poolHeight} ORDER BY Fee DESC LIMIT {num};";

    //        var smartList = new List<Smartcontract>();

    //        using (SQLiteDataReader reader = QuerySQL(sql))
    //        {
    //            for (int i = 0; i < num; i++)
    //            {

    //                if (!reader.Read())
    //                    break;

    //                string receivingAddr = (string)reader[0];

    //                Smartcontract smart = GetSmartcontract(receivingAddr);

    //                smartList.Add(smart);

    //            }
    //        }

    //        return smartList;
    //    }

    //    private List<Transaction> GetTransactionsFromTransPool(long height, int num)
    //    {

    //        //get normal data
    //        string sql = $"SELECT * FROM Transactions WHERE PoolHeight={height} ORDER BY MinerReward DESC LIMIT {num};";

    //        var transList = new List<Transaction>();

    //        using (SQLiteDataReader reader = QuerySQL(sql))
    //        {
    //            for (int i = 0; i < num; i++)
    //            {

    //                if (!reader.Read())
    //                    break;

    //                long ID = (long)reader[0];
    //                var output = GetTransactionOutput(ID);

    //                Transaction trans = new Transaction(reader, output.Item1, output.Item2, GetTransactionData(ID))
    //                {
    //                    SendTo = Utils.GetTransactionPoolAddress(height, CoinName)
    //                };

    //                transList.Add(trans);

    //            }
    //        }

    //        return transList;
    //    }

    //    #endregion

    //    #region Set Set Set Set Set Set Set Set Set Set Set Set Set Set Set 

    //    public void Add<T>(List<T> obj, long? BlockHeight = null, long? poolHeight = null) where T : IDownloadable
    //    {
    //        obj.ForEach(x => Add(x, BlockHeight, poolHeight));
    //    }

    //    public void Add<T>(T obj, long? BlockHeight = null, long? poolHeight = null) where T : IDownloadable
    //    {

    //        if (typeof(T) == typeof(Block))
    //        {
    //            AddBlock((Block)(object)obj);
    //        }
    //        else if (typeof(T) == typeof(Transaction))
    //        {
    //            AddTransaction((Transaction)(object)obj, BlockHeight, poolHeight);
    //        }
    //        else if (typeof(T) == typeof(Smartcontract))
    //        {
    //            AddSmartcontract((Smartcontract)(object)obj, BlockHeight, poolHeight);
    //        }
    //        else
    //        {
    //            throw new ArgumentException("WRONG TYPE SOMEHOW THIS SHOULD NEVER APPEAR!");
    //        }
    //    }

    //    #endregion

    //    #region Get Get Get Get Get Get Get Get Get Get Get Get Get 



    //    public Block GetBlock(long height)
    //    {


    //    }

    //    public Smartcontract GetSmartcontract(string receivingAddr)
    //    {

    //        string sql = $"SELECT * FROM Smartcontracts WHERE ReceivingAddress='{receivingAddr}';";

    //        using (SQLiteDataReader reader = QuerySQL(sql))
    //        {

    //            if (!reader.Read())
    //            {
    //                return null;
    //            }

    //            Smartcontract smart = new Smartcontract(reader);

    //            //we also need to add the variables:
    //            //using(SQLiteDataReader reader = QuerySQL($"SELECT * FROM Variables WHERE "))
    //            smart.Code.Variables = GetVariablesFromDB((long)reader[0]);
    //            return smart;
    //        }
    //    }

    //    public Transaction GetTransaction(string hash, long height)
    //    {


    //    }

    //    public Block GetLatestBlock()
    //    {

    //        //we first get highest ID
    //        using (SQLiteDataReader reader = QuerySQL($"SELECT IFNULL(MAX(Height),0) FROM Block"))
    //        {

    //            if (!reader.Read())
    //                return null;

    //            return GetBlock((long)reader[0]);
    //        }
    //    }

    //    public Dictionary<string, ISCType> GetVariablesFromDB(long ID)
    //    {

    //        string sql = $"SELECT Name,Value FROM Variables WHERE SmartID={ID}";

    //        using (SQLiteDataReader reader = QuerySQL(sql)) {
    //            var list = new Dictionary<string, ISCType>();

    //            while (reader.Read())
    //            {
    //                list.Add((string)reader[0], ((string)reader[1]).ConvertToInternalType());
    //            }
    //            return list;
    //        }

    //    }

}
