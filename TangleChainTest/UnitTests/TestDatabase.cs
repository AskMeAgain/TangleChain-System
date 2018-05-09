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
        public void TestCompleteDownloadAndStorage() {

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
    }
}
