using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
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
            GetDatabase(CoinName);

            Databases[CoinName].ChainSettings = cSett;
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

        public static void Add<T>(string CoinName, T obj, long? blockHeight = null, long? poolHeight = null) where T : IDownloadable
        {
            GetDatabase(CoinName).Add(obj, blockHeight, poolHeight);
        }

        public static void Add<T>(string CoinName, List<T> obj, long? blockHeight = null, long? poolHeight = null) where T : IDownloadable
        {
            GetDatabase(CoinName).Add(obj, blockHeight, poolHeight);
        }

        public static void Add(Block block)
        {
            GetDatabase(block.CoinName).Add(block);
        }


        public static List<T> GetFromPool<T>(string name, long height, int num) where T : ISignable
        {
            return GetDatabase(name).GetFromTransPool<T>(height, num);
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
