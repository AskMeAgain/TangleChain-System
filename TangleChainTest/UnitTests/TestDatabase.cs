
using System;
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

            db.AddBlock(test,false);

            Block compare = db.GetBlock(test.Height);

            Assert.AreEqual(test, compare);

        }

        [TestMethod]
        public void DownloadChainAndStorage() {

            //testing download function 

            string address = "ROHTDPFMZXBLDLJEZZGLDHIFX9VQKOESYIDSIOVKOGQWT9RQAELIK9HLKIUFCYPOVIICTLEXBXDERBLRT";
            string hash = "KPSF9CBDSEADWUXRZWS9FOXPXZZSKHXPCWBPGFDUQWPQVMZERHUBQDLTEWQTFFY9KXJURYNPWTRNGZ9XW";
            int difficulty = 5;

            string expectedHash = "TBAWNFVWXSQAXUMIIZIW9NAVQZHWWQK9LPOUGRKHGGROOZUGYHACXJTUTM9GVPUYAYCOO9EGSUYOCJNKJ";

            Block latest = Core.DownloadChain(address, hash, difficulty, true);

            Assert.AreEqual(latest.Hash, expectedHash);

            DataBase Db = new DataBase(latest.CoinName);

            Block storedBlock = Db.GetBlock(latest.Height);

            Assert.AreEqual(latest, storedBlock);

        }

        [TestMethod]
        public void GetBalance() {

            DataBase db = new DataBase("IQXOGNUS9C");
            
            int balance = db.GetBalance("ME");

            Assert.AreEqual(9100, balance);

            int transFees = db.GetAllTransactionFees("ME");

            Assert.AreEqual(-900,transFees);

        }


        [TestMethod]
        public void UploadDownloadAndStorage_Transaction() {

            string sendTo = Utils.GenerateRandomAddress();

            Transaction uploadTrans = Transaction.CreateTransaction("ME", sendTo, 0, 1000);

            Core.UploadTransaction(uploadTrans);

            List<Transaction> transList = Core.GetAllTransactionsFromAddress(sendTo);

            Transaction trans = transList[0];

            DataBase db = new DataBase(Utils.GenerateRandomString(10));

            trans.Print();

            db.AddTransactionToDatabase(transList);

            Transaction compare = db.GetTransaction(trans.Identity.SendTo, trans.Identity.Hash);

            Assert.AreEqual(trans, compare);

        }

    }
}
