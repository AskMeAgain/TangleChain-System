using LiteDB;
using System;
using System.IO;

namespace TangleChain.Classes {

    public class DataBase {

        LiteDatabase Db;

        public DataBase(string name) {

            //create folder structure
            if (!Directory.Exists(@"C:\TangleChain\Chains\" + name + @"\")) {
                Directory.CreateDirectory(@"C:\TangleChain\Chains\" + name + @"\");
                Console.WriteLine("created");
            }

            //assign DB
            Db = new LiteDatabase(@"C:\TangleChain\Chains\" + name + @"\chain.db");
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
