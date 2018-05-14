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

            //all fees and reduction of your address
            sum += GetAllOrderFees(user, collection);

            Console.WriteLine("Sum here: " + sum);

            //all receiving transactions
            sum += GetAllReceivingOrders(user, collection);

            return sum;
        }

        private int GetAllReceivingOrders(string user, LiteCollection<Order> collection) {

            int sum = 0;

            var incoming = collection.Find(m => m.Output_Receiver.Contains(user));

            foreach (Order order in incoming) {
                Console.WriteLine("count trans in: " + order.Output_Receiver[0] + " " + user);

                for (int i = 0; i < order.Output_Receiver.Count; i++) {
                    if (order.Output_Receiver[i].Equals(user)) {
                        sum += order.Output_Value[i];
                    }
                }
            }

            return sum * -1;
        }

        public int GetAllOrderFees(string user, LiteCollection<Order> collection) {

            List<Order> outcoming = collection.Find(m => m.From.Equals(user)).ToList();

            int sum = 0;

            foreach (Order order in outcoming) {
                sum -= int.Parse(order.Data[0]);

                if (order.Output_Value.Count > 0) {
                    order.Output_Value.ForEach(m => { sum += m; });
                }
            }

            return sum;
        }
    }
}
