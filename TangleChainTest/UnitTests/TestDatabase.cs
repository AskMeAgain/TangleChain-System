
using System.Collections.Generic;
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
        public void DownloadAndStorage_Block() {

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
        public void UploadDownloadAndStorage_Transaction() {

            string sendTo = "BIGEFDHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAAA";

            Transaction uploadTrans = Transaction.CreateTransaction("ME", sendTo, 0, 1000);

            Core.UploadTransaction(uploadTrans);

            List<Transaction> transList = Core.GetAllTransactionsFromAddress(sendTo);

            Transaction trans = transList[0];

            DataBase db = new DataBase("TestUploadDownload");

            trans.Print();

            db.AddTransactionToDatabase(transList);

            Transaction compare = db.GetTransaction(trans.Identity.SendTo, trans.Identity.Hash);

            Assert.AreEqual(trans, compare);

        }

        [TestMethod]
        public void GetBalance() {

            //string sendTo = "FAAAAAHKBPMYIDWVOQMO9JZCUMIQYWFDIT9SFNWBRLEGX9LKLZGZFRCGLGSBZGMSDYMLMCO9UMAXAOAPA";

            //Transaction uploadTrans = Transaction.CreateTransaction("ME", sendTo, 0, 1000);
            //uploadTrans.AddOutput(-100, "LOL");
            //uploadTrans.AddOutput(-200, "LOL");

            //uploadTrans.Sign("private key!");

            //Core.UploadTransaction(uploadTrans);

            //List<Transaction> transList = Core.GetAllTransactionsFromAddress(sendTo);

            DataBase db = new DataBase("TestGetBalance");

            //db.AddTransactionToDatabase(transList);

            int balance = db.GetBalance("LOL");

            Assert.AreEqual(300, balance);

        }

    }
}
