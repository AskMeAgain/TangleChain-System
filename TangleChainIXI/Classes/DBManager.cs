using System;
using System.Collections.Generic;
using System.Text;

namespace TangleChainIXI.Classes {
    public static class DBManager {

        private static Dictionary<string, DataBase> Databases;

        public static DataBase GetDatabase(string name) {

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

        public static void AddBlock(string name, Block block, bool storeTransactions) {
            GetDatabase(name).AddBlock(block, storeTransactions);
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



    }
}
