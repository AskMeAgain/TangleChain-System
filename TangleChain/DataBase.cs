using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            collection.Upsert(list);
            collection.EnsureIndex("SendTo");
            collection.EnsureIndex("ID");
        }

        public Order GetOrder(string sendTo, string Hash) {

            LiteCollection<Order> collection = Db.GetCollection<Order>("Orders");

            Console.WriteLine("Get Order Collection has {0} orders", collection.Count());

            return collection.FindOne(m => m.Identity.SendTo.Equals(sendTo) && m.Identity.Hash.Equals(Hash));
        }

        public int GetBalance(string user) {

            LiteCollection<Order> collection = Db.GetCollection<Order>("Orders").Include("Outputs");

            int sum = 0;

            sum += GetAllOrderFees(user, collection);

            return sum;
        }

        public int GetAllOrderFees(string user, LiteCollection<Order> collection) {

            List<Order> outcoming = collection.Find(m => m.From.Equals(user)).ToList();
            //var incoming = collection.Find(m => m.Outputs.Exists(t => t.Receiver.Equals(user)));

            int sum = 0;

            foreach (Order order in outcoming) {
                sum -= int.Parse(order.Data[0]);

                if (order.Trans_In.Count > 0) {
                    order.Trans_In.ForEach(m => { sum += m; });
                    Console.WriteLine("happened?");
                }
            }

            return sum;
        }
    }
}
