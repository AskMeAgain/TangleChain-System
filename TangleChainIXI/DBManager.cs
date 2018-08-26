﻿using System;
using System.Collections.Generic;
using System.Text;
using TangleChainIXI.Classes;

namespace TangleChainIXI {
    public static class DBManager {

        private static Dictionary<string, DataBase> Databases;

        private static DataBase GetDatabase(string name) {

            if (Databases == null)
                Databases = new Dictionary<string, DataBase>();

            if (!Databases.ContainsKey(name))
                Load(name);

            if (Databases[name] == null)
                Load(name);

            return Databases[name];
        }

        private static void Load(string name) {

            DataBase Db = new DataBase(name);

            Databases[name] = Db;

        }

        public static bool AddBlock(string name, Block block, bool storeTransactions) {
            return GetDatabase(name).AddBlock(block, storeTransactions);
        }

        public static Block GetLatestBlock(string name) {
            return GetDatabase(name).GetLatestBlock();
        }

        public static Block GetBlock(string name, long height) {
            return GetDatabase(name).GetBlock(height);
        }

        public static void DeleteBlock(string name, long height) {
            GetDatabase(name).DeleteBlock(height);
        }

        public static void AddBlocks(string name, List<Block> list, bool storeTransactions) {
            GetDatabase(name).AddBlocks(list, storeTransactions);
        }

        public static Difficulty GetDifficulty(string name, Way way) {
            return GetDatabase(name).GetDifficulty(way);
        }

        public static Difficulty GetDifficulty(string name, long height) {
            return GetDatabase(name).GetDifficulty(height);
        }

        public static void SetChainSettings(string name, ChainSettings cSett) {
            GetDatabase(name).ChainSettings = cSett;
        }

        public static ChainSettings GetChainSettings(string name) {
            return GetDatabase(name).ChainSettings;
        }

        public static long GetBalance(string name, string user) {
            return GetDatabase(name).GetBalance(user);
        }

        public static void AddTransaction(string name,List<Transaction> list, long? BlockID, long? PoolHeight ) {
            GetDatabase(name).AddTransaction(list, BlockID, PoolHeight);
        }

        public static void AddTransaction(string name, Transaction trans, long? BlockID, long? PoolHeight) {
            GetDatabase(name).AddTransaction(trans, BlockID, PoolHeight);
        }

        public static List<Transaction> GetTransactionsFromTransPool(string name, long height, int num) {
            return GetDatabase(name).GetTransactionsFromTransPool(height, num);
        }

        public static Transaction GetTransaction(string name,string hash, long height) {
            return GetDatabase(name).GetTransaction(hash, height);
        }


    }
}