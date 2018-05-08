using LiteDB;
using System;
using System.IO;

namespace TangleChain.Classes {

    public class DataBase {

        static LiteDatabase Db;

        public DataBase() {
            Init();
        }

        public bool IsWorking() {
            return (Db != null) ? true : false;
        }

        void Init() {

            //create folder structure
            if (Directory.Exists(@"C:\TangleChain\Chain"))
                Directory.CreateDirectory(@"C:\TangleChain\Chain");

            //assign DB
            Db = new LiteDatabase(@"C:\TangleChain\Chain\Database.db");

        }

        public void AddBlock(Block block) {

            LiteCollection<Block> collection = Db.GetCollection<Block>("Blocks");

            if (!collection.Exists(m => m.Height == block.Height)) {
                collection.Insert(block);
                collection.EnsureIndex("Height");
            } else {
                collection.Update(block);
            }

        }

        public Block GetBlock(int height) {

            LiteCollection<Block> collection = Db.GetCollection<Block>("Blocks");

            return collection.FindOne(m => m.Height == height);
        }

    }
}
