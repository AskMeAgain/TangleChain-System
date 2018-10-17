using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Smartcontracts;

namespace TangleChainIXI
{

    public static class DBManager
    {

        private static Dictionary<string, DataBase> Databases;

        /// <summary>
        /// Internal method which caches database objects
        /// </summary>
        /// <param CoinName="name"></param>
        /// <returns></returns>
        private static DataBase GetDatabase(string name)
        {

            if (Databases == null)
                Databases = new Dictionary<string, DataBase>();

            if (!Databases.ContainsKey(name))
                Load(name);

            if (Databases[name] == null)
                Load(name);

            return Databases[name];
        }

        /// <summary>
        /// Internal method which loads a new Database object into cache
        /// </summary>
        /// <param name="name"></param>
        private static void Load(string name)
        {

            DataBase Db = new DataBase(name);
            Databases[name] = Db;

        }

        /// <summary>
        /// Adds the given block to a database. Returns if the block got correctly added or not
        /// </summary>
        /// <param name="block">The Block</param>
        /// <returns>If true, the block got added</returns>
        public static bool AddBlock(Block block)
        {
            return GetDatabase(block.CoinName).AddBlock(block);
        }

        /// <summary>
        /// Returns the latest block from a given coin
        /// </summary>
        /// <param name="CoinName"></param>
        /// <returns></returns>
        public static Block GetLatestBlock(string CoinName)
        {
            return GetDatabase(CoinName).GetLatestBlock();
        }

        /// <summary>
        /// Gets the block from a given height
        /// </summary>
        /// <param name="CoinName">Coinname</param>
        /// <param name="height">Height of the requested block</param>
        /// <returns></returns>
        public static Block GetBlock(string CoinName, long height)
        {
            return GetDatabase(CoinName).GetBlock(height);
        }

        /// <summary>
        /// Deletes the block with the given height
        /// </summary>
        /// <param name="CoinName"></param>
        /// <param name="height"></param>
        public static void DeleteBlock(string CoinName, long height)
        {
            GetDatabase(CoinName).DeleteBlock(height);
        }

        /// <summary>
        /// Adds multiple blocks to the Database
        /// </summary>
        /// <param name="CoinName"></param>
        /// <param name="list"></param>
        /// <param name="storeTransactions"></param>
        /// <param name="includeSmartcontracts"></param>
        public static bool AddBlocks(string CoinName, List<Block> list)
        {
            return GetDatabase(CoinName).AddBlocks(list);
        }

        /// <summary>
        /// Adds a smartcontract to the Database
        /// </summary>
        /// <param name="CoinName">Name of the coin</param>
        /// <param name="smart">The smartcontract</param>
        /// <param name="height">The height where the smartcontract got stored</param>
        public static void AddSmartcontract(string CoinName, Smartcontract smart, long? BlockID, long? poolHeight)
        {
            GetDatabase(CoinName).AddSmartcontract(smart, BlockID, poolHeight);
        }

        /// <summary>
        /// Adds multiple smartcontracts to the db
        /// </summary>
        /// <param name="CoinName"></param>
        /// <param name="list"></param>
        /// <param name="BlockID"></param>
        /// <param name="poolHeight"></param>
        public static void AddSmartcontracts(string CoinName, List<Smartcontract> list, long? BlockID, long? poolHeight)
        {
            GetDatabase(CoinName).AddSmartcontracts(list, BlockID, poolHeight);
        }

        /// <summary>
        /// Returns the Smartcontract with the given receiving addr
        /// </summary>
        /// <param name="CoinName">The coinname</param>
        /// <param name="receivingAddr">The receiving address of the smartcontract</param>
        /// <returns></returns>
        public static Smartcontract GetSmartcontract(string CoinName, string receivingAddr)
        {
            return GetDatabase(CoinName).GetSmartcontract(receivingAddr);
        }

        /// <summary>
        /// Gets the new difficulty from a given way
        /// </summary>
        /// <param name="CoinName">The coinname</param>
        /// <param name="way">The Way</param>
        /// <returns></returns>
        public static int GetDifficulty(string CoinName, Way way)
        {
            return GetDatabase(CoinName).GetDifficulty(way);
        }

        /// <summary>
        /// Gets the difficulty from the DB
        /// </summary>
        /// <param name="CoinName">The Coinname</param>
        /// <param name="height">The height of the new block</param>
        /// <returns></returns>
        public static int GetDifficulty(string CoinName, long height)
        {
            return GetDatabase(CoinName).GetDifficulty(height);
        }

        /// <summary>
        /// Sets the chainsettings for a specific coin
        /// </summary>
        /// <param name="CoinName"></param>
        /// <param name="cSett"></param>
        public static void SetChainSettings(string CoinName, ChainSettings cSett)
        {
            GetDatabase(CoinName).ChainSettings = cSett;
        }

        /// <summary>
        /// Gets the chainsetting from DB
        /// </summary>
        /// <param name="CoinName"></param>
        /// <returns></returns>
        public static ChainSettings GetChainSettings(string CoinName)
        {
            return GetDatabase(CoinName).ChainSettings;
        }

        /// <summary>
        /// Gets the balance for a given address
        /// </summary>
        /// <param name="CoinName"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static long GetBalance(string CoinName, string address)
        {
            return GetDatabase(CoinName).GetBalance(address);
        }

        /// <summary>
        /// Adds transactions to the DB
        /// </summary>
        /// <param name="CoinName">The name of the coin</param>
        /// <param name="list">List of transactions</param>
        /// <param name="Height">The height of the block. If null then we add a block to the transaction pool</param>
        /// <param name="PoolHeight">If not null then we add a transaction to a specific pool height</param>
        public static void AddTransactions(string CoinName, List<Transaction> list, long? Height, long? PoolHeight)
        {
            GetDatabase(CoinName).AddTransactions(list, Height, PoolHeight);
        }

        /// <summary>
        /// Adds a transaction to the DB
        /// </summary>
        /// <param name="CoinName">The name of the coin</param>
        /// <param name="list">List of transactions</param>
        /// <param name="Height">The height of the block. If null then we add a block to the transaction pool</param>
        /// <param name="PoolHeight">If not null then we add a transaction to a specific pool height</param>
        public static void AddTransaction(string name, Transaction trans, long? BlockID, long? PoolHeight)
        {
            GetDatabase(name).AddTransaction(trans, BlockID, PoolHeight);
        }

        /// <summary>
        /// Gets a specific amount of transactions from the transactionpool in the database
        /// </summary>
        /// <param name="name">Name of the coin</param>
        /// <param name="height">The height of the transactionpool</param>
        /// <param name="num">The number of transactions which are requested</param>
        /// <returns></returns>
        public static List<Transaction> GetTransactionsFromTransPool(string name, long height, int num)
        {
            return GetDatabase(name).GetTransactionsFromTransPool(height, num);
        }

        /// <summary>
        /// Returns a speific transaction from the database
        /// </summary>
        /// <param name="name">The name of the coin</param>
        /// <param name="hash">The hash of the transaction</param>
        /// <param name="height">The height of the transaction</param>
        /// <returns></returns>
        public static Transaction GetTransaction(string name, string hash, long height)
        {
            return GetDatabase(name).GetTransaction(hash, height);
        }
    }
}
