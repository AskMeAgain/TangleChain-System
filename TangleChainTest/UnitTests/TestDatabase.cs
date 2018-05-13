using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TangleChain;
using TangleChain.Classes;

namespace TangleChainTest.UnitTests {

    [TestClass]
    public class TestDatabase {

        [TestMethod]
        public void TestInit() {

            DataBase db = new DataBase("Test");

            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsWorking());

        }

        [TestMethod]
        public void TestAddAndGetBlock() {

            Block test = new Block();
            DataBase db = new DataBase("Test");

            db.AddBlock(test);

            Block compare = db.GetBlock(test.Height);

            Assert.AreEqual(test, compare);

        }

        [TestMethod]
        public void TestCompleteDownloadAndStorage_Blocks() {

            //testing download function in a more sophisticated split 1 22 33 4  

            string address = "JIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";
            string hash = "A9XGUQSNWXYEYZICOCHC9B9GV9EFNOWBHPCX9TSKSPDINXXCFKJJAXNHMIWCXELEBGUL9EOTGNWYTLGNO";
            int difficulty = 5;

            string expectedHash = "YNQ9PRSBFKKCMO9DZUPAQPWMYVDQFDYXNJBWISBOHZZHLPMCRS9KSJOIZAQPYLIOCJPLNORCASITPUJUV";

            Block latest = Core.DownloadChain(address, hash, difficulty, true);
            Assert.AreEqual(latest.Hash, expectedHash);

            DataBase Db = new DataBase(latest.CoinName);

            Block storedBlock = Db.GetBlock(latest.Height);

            Assert.AreEqual(latest, storedBlock);

        }

        [TestMethod]
        public void TestCompleteUploadDownloadAndStore_Orders() {

            string sendTo = "BIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";

            Order uploadOrder = Core.CreateOrder("ME", sendTo, 0, 1000);

            Core.UploadOrder(uploadOrder);

            List<Order> listOrders = Core.GetAllOrdersFromAddress(sendTo);

            Order order = listOrders[0];

            DataBase db = new DataBase("Test");

            db.AddOrders(listOrders);

            Order compare = db.GetOrder(order.Identity.SendTo, order.Identity.Hash);

            Assert.AreEqual(order, compare);

        }

        [TestMethod]
        public void TestGetBalance() {

            string sendTo = "FAAAAAHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPH";

            //Order uploadOrder = Core.CreateOrder("ME", sendTo, 0, 1000);
            //uploadOrder.AddOutput(-100, "LOL");
            //uploadOrder.AddOutput(-200, "LOL");

            //uploadOrder.Sign("private key!");

            //Core.UploadOrder(uploadOrder);

            List<Order> listOrders = Core.GetAllOrdersFromAddress(sendTo);

            DataBase db = new DataBase("Test06");

            db.AddOrders(listOrders);

            int balance = db.GetBalance("ME");

            Console.WriteLine(balance);

            Assert.AreEqual(-1300, balance);

        }

    }
}
