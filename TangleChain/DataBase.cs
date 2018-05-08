using LiteDB;
using System;
using System.IO;

namespace TangleChain.Classes {

    public class DataBase {

        LiteDatabase Db;

        public DataBase(string name) {

            //create folder structure
            if (Directory.Exists(@"C:\TangleChain\Chain"))
                Directory.CreateDirectory(@"C:\TangleChain\Chain");

            //assign DB
            Db = new LiteDatabase(@"C:\TangleChain\Chain\" + name + ".db");
        }

        public bool IsWorking() {
            return (Db != null) ? true : false;
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
