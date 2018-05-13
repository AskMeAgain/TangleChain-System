using LiteDB;
using System;
using System.Collections.Generic;
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

        public void AddOrders(List<Order> list) {

            LiteCollection<Order> collection = Db.GetCollection<Order>("Orders");

            foreach (Order order in list) {

                order.Print();

                if (!collection.Exists(m => m.SendTo.Equals(order.SendTo) && m.Hash.Equals(order.Hash))) {
                    collection.Insert(order);
                    collection.EnsureIndex("SendTo");
                    collection.EnsureIndex("ID");
                } else {
                    collection.Update(order);
                }

            }
        }

        public Order GetOrder(string sendTo, string Hash) {

            LiteCollection<Order> collection = Db.GetCollection<Order>("Orders");

            Console.WriteLine("Get Order Collection has {0} orders", collection.Count());

            return collection.FindOne(m => m.SendTo.Equals(sendTo) && m.Hash.Equals(Hash));
        }

    }
}
